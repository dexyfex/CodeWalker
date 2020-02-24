using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using Resource = SharpDX.Direct3D11.Resource;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using CodeWalker.World;
using CodeWalker.Properties;

namespace CodeWalker.Rendering
{
    public class Shadowmap
    {
        Texture2D DepthTexture;
        SamplerState DepthTextureSS;
        ShaderResourceView DepthTextureSRV;
        DepthStencilView DepthTextureDSV;
        RasterizerState DepthRenderRS;
        DepthStencilState DepthRenderDS;
        ViewportF DepthRenderVP;
        GpuVarsBuffer<ShadowmapVars> ShadowVars;
        RenderTargetSwitch RTS = new RenderTargetSwitch();
        public List<ShadowmapCascade> Cascades;
        Matrix SceneCamView;
        Matrix LightView;
        Vector3 LightDirection;
        int TextureSize;
        public int CascadeCount;
        int PCFSize;
        float PCFOffset;
        float BlurBetweenCascades;
        Vector3 WorldMin;
        Vector3 WorldMax;
        public Vector3 SceneOrigin;
        Vector3 SceneCamPos;
        Vector3 SceneMin;
        Vector3 SceneMax;
        Vector3 SceneCenter;
        Vector3 SceneExtent;

        float[] fCascadeIntervals = { 7.0f, 20.0f, 65.0f, 160.0f, 600.0f, 3000.0f, 5000.0f, 10000.0f };
        public float maxShadowDistance = 3000.0f;


        long graphicsMemoryUsage = 0;
        public long VramUsage
        {
            get
            {
                return graphicsMemoryUsage;
            }
        }

        public Shadowmap(Device device)
        {
            TextureSize = 1024; //todo: make this a setting...
            CascadeCount = Math.Min(Settings.Default.ShadowCascades, 8);// 6; //use setting
            PCFSize = 3;
            PCFOffset = 0.000125f; //0.002f
            BlurBetweenCascades = 0.05f;

            ShadowVars = new GpuVarsBuffer<ShadowmapVars>(device);


            DepthTexture = DXUtility.CreateTexture2D(device, TextureSize* CascadeCount, TextureSize, 1, 1, Format.R32_Typeless, 1, 0, ResourceUsage.Default, BindFlags.DepthStencil | BindFlags.ShaderResource, 0, 0);
            DepthTextureSS = DXUtility.CreateSamplerState(device, TextureAddressMode.Border, new Color4(0.0f), Comparison.Less, Filter.ComparisonMinMagLinearMipPoint, 0, 0.0f, 0.0f, 0.0f);
            DepthTextureSRV = DXUtility.CreateShaderResourceView(device, DepthTexture, Format.R32_Float, ShaderResourceViewDimension.Texture2D, 1, 0, 0, 0);
            DepthTextureDSV = DXUtility.CreateDepthStencilView(device, DepthTexture, Format.D32_Float, DepthStencilViewDimension.Texture2D);

            Cascades = new List<ShadowmapCascade>(CascadeCount);
            for (int i = 0; i < CascadeCount; i++)
            {
                ShadowmapCascade c = new ShadowmapCascade();
                c.Owner = this;
                c.Index = i;
                c.ZNear = 0.0f;
                c.ZFar = 1.0f;
                c.IntervalNear = 0.0f;
                c.IntervalFar = 1.0f;
                c.DepthRenderVP = new ViewportF()
                {
                    Height = (float)TextureSize,
                    Width = (float)TextureSize,
                    MaxDepth = 1.0f,
                    MinDepth = 0.0f,
                    X = (float)(TextureSize * i),
                    Y = 0,
                };
                Cascades.Add(c);
            }

            DepthRenderRS = DXUtility.CreateRasterizerState(device, FillMode.Solid, CullMode.None, true, false, true, 0, 0.0f, 1.0f);
            DepthRenderDS = DXUtility.CreateDepthStencilState(device, true, DepthWriteMask.All);

            DepthRenderVP = new ViewportF();
            DepthRenderVP.Height = (float)TextureSize;
            DepthRenderVP.Width = (float)TextureSize;
            DepthRenderVP.MaxDepth = 1.0f;
            DepthRenderVP.MinDepth = 0.0f;
            DepthRenderVP.X = 0;
            DepthRenderVP.Y = 0;

            graphicsMemoryUsage = (long)(TextureSize * TextureSize * CascadeCount * 4);
        }
        public void Dispose()
        {
            graphicsMemoryUsage = 0;
            if (DepthTexture != null)
            {
                DepthTexture.Dispose();
                DepthTexture = null;
            }
            if (DepthTextureSS != null)
            {
                DepthTextureSS.Dispose();
                DepthTextureSS = null;
            }
            if (DepthTextureSRV != null)
            {
                DepthTextureSRV.Dispose();
                DepthTextureSRV = null;
            }
            if (DepthTextureDSV != null)
            {
                DepthTextureDSV.Dispose();
                DepthTextureDSV = null;
            }
            if (DepthRenderRS != null)
            {
                DepthRenderRS.Dispose();
                DepthRenderRS = null;
            }
            if (DepthRenderDS != null)
            {
                DepthRenderDS.Dispose();
                DepthRenderDS = null;
            }
            if (ShadowVars != null)
            {
                ShadowVars.Dispose();
                ShadowVars = null;
            }
            if (Cascades != null)
            {
                Cascades.Clear();
                Cascades = null;
            }
        }


        public void BeginUpdate(DeviceContext context, Camera cam, Vector3 lightDir, List<RenderableGeometryInst> items)
        {
            //items should be potential shadow casters.

            RTS.Set(context);

            var ppos = cam.Position;
            var view = cam.ViewMatrix;
            var proj = cam.ProjMatrix;
            var viewproj = cam.ViewProjMatrix;


            //need to compute a local scene space for the shadows. use a snapped version of the camera coords...
            Vector3 pp = ppos;
            float snapsize = 20.0f; //20m snap... //ideally should snap to texel size
            SceneOrigin.X = pp.X - (pp.X % snapsize);
            SceneOrigin.Y = pp.Y - (pp.Y % snapsize);
            SceneOrigin.Z = pp.Z - (pp.Z % snapsize);
            SceneCamPos = (pp - SceneOrigin);


            //the items passed in here are visible items. need to compute the scene bounds from these.
            Vector4 vFLTMAX = new Vector4(float.MaxValue);
            Vector4 vFLTMIN = new Vector4(float.MinValue);
            Vector3 vHMAX = new Vector3(float.MaxValue);
            Vector3 vHMIN = new Vector3(float.MinValue);
            WorldMin = vHMAX;
            WorldMax = vHMIN;
            for (int i = 0; i < items.Count; i++)
            {
                var inst = items[i].Inst;

                Vector3 imin = inst.BBMin - 100.0f; //extra bias to make sure scene isn't too small in model view...
                Vector3 imax = inst.BBMax + 100.0f;

                WorldMin = Vector3.Min(WorldMin, imin);
                WorldMax = Vector3.Max(WorldMax, imax);
            }
            SceneMin = (WorldMin - SceneOrigin);
            SceneMax = (WorldMax - SceneOrigin);
            SceneCenter = (SceneMax + SceneMin) * 0.5f;
            SceneExtent = (SceneMax - SceneMin) * 0.5f;

            Matrix sceneCamTrans = Matrix.Translation(-SceneCamPos);
            SceneCamView = Matrix.Multiply(sceneCamTrans, view);
            Matrix camViewInv = Matrix.Invert(SceneCamView);

            Vector3 lightUp = new Vector3(0.0f, 1.0f, 0.0f); //BUG: should select this depending on light dir!?
            LightView = Matrix.LookAtLH(lightDir, Vector3.Zero, lightUp); //BUG?: pos/lightdir wrong way around??
            LightDirection = lightDir;

            Vector4[] vSceneAABBPointsLightSpace = new Vector4[8];
            // This function simply converts the center and extents of an AABB into 8 points
            CreateAABBPoints(ref vSceneAABBPointsLightSpace, SceneCenter, SceneExtent);
            // Transform the scene AABB to Light space.
            for (int index = 0; index < 8; ++index)
            {
                vSceneAABBPointsLightSpace[index] = LightView.Multiply(vSceneAABBPointsLightSpace[index]);
            }


            float fFrustumIntervalBegin, fFrustumIntervalEnd;
            Vector4 vLightCameraOrthographicMin;  // light space frustrum aabb 
            Vector4 vLightCameraOrthographicMax;
            //float[] fCascadeIntervals = { 7.5f, 20.0f, 60.0f, 150.0f, 500.0f, 1000.0f, 1500.0f, 2500.0f };
            //float[] fCascadeIntervals = { 7.0f, 20.0f, 65.0f, 160.0f, 650.0f, 2000.0f, 5000.0f, 10000.0f };

            Vector4 vWorldUnitsPerTexel = Vector4.Zero;
            float fInvTexelCount = 1.0f / (float)TextureSize;

            // We loop over the cascades to calculate the orthographic projection for each cascade.
            for (int iCascadeIndex = 0; iCascadeIndex < CascadeCount; ++iCascadeIndex)
            {
                ShadowmapCascade cascade = Cascades[iCascadeIndex];

                fFrustumIntervalBegin = 0.0f;
                // Scale the intervals between 0 and 1. They are now percentages that we can scale with.
                fFrustumIntervalEnd = fCascadeIntervals[iCascadeIndex];
                //fFrustumIntervalBegin = fFrustumIntervalBegin * fCameraNearFarRange;
                //fFrustumIntervalEnd = fFrustumIntervalEnd * fCameraNearFarRange;
                Vector4[] vFrustumPoints = new Vector4[8];


                // This function takes the began and end intervals along with the projection matrix and returns the 8
                // points that repreresent the cascade Interval
                CreateFrustumPointsFromCascadeInterval(fFrustumIntervalBegin, fFrustumIntervalEnd, proj, ref vFrustumPoints);

                vLightCameraOrthographicMin = vFLTMAX;
                vLightCameraOrthographicMax = vFLTMIN;

                Vector4 vTempTranslatedCornerPoint;
                // This next section of code calculates the min and max values for the orthographic projection.
                for (int icpIndex = 0; icpIndex < 8; ++icpIndex)
                {
                    // Transform the frustum from camera view space to world space.
                    vFrustumPoints[icpIndex] = camViewInv.Multiply(vFrustumPoints[icpIndex]);
                    // Transform the point from world space to Light Camera Space.
                    vTempTranslatedCornerPoint = LightView.Multiply(vFrustumPoints[icpIndex]);
                    // Find the closest point.
                    vLightCameraOrthographicMin = Vector4.Min(vTempTranslatedCornerPoint, vLightCameraOrthographicMin);
                    vLightCameraOrthographicMax = Vector4.Max(vTempTranslatedCornerPoint, vLightCameraOrthographicMax);
                }

                // This code removes the shimmering effect along the edges of shadows due to
                // the light changing to fit the camera.
                // Fit the ortho projection to the cascades far plane and a near plane of zero. 
                // Pad the projection to be the size of the diagonal of the Frustum partition. 
                // 
                // To do this, we pad the ortho transform so that it is always big enough to cover 
                // the entire camera view frustum.
                Vector4 vDiagonal = (vFrustumPoints[0] - vFrustumPoints[6]);

                // The bound is the length of the diagonal of the frustum interval.
                float fCascadeBound = vDiagonal.XYZ().Length();
                vDiagonal = new Vector4(fCascadeBound);

                // The offset calculated will pad the ortho projection so that it is always the same size 
                // and big enough to cover the entire cascade interval.
                Vector4 vBorderOffset = (vDiagonal - (vLightCameraOrthographicMax - vLightCameraOrthographicMin)) * 0.5f;
                // Set the Z and W components to zero.
                //vBoarderOffset *= g_vMultiplySetzwToZero;
                vBorderOffset.Z = 0.0f;
                vBorderOffset.W = 0.0f;

                // Add the offsets to the projection.
                vLightCameraOrthographicMax += vBorderOffset;
                vLightCameraOrthographicMin -= vBorderOffset;

                // The world units per texel are used to snap the shadow the orthographic projection
                // to texel sized increments.  This keeps the edges of the shadows from shimmering.
                float fWorldUnitsPerTexel = fCascadeBound / (float)TextureSize;
                vWorldUnitsPerTexel = new Vector4(fWorldUnitsPerTexel, fWorldUnitsPerTexel, 1.0f, 1.0f); //1.0 instead of 0.0 to remove divide by 0

                // We snap the camera to 1 pixel increments so that moving the camera does not cause the shadows to jitter.
                // This is a matter of integer dividing by the world space size of a texel
                vLightCameraOrthographicMin = vLightCameraOrthographicMin / vWorldUnitsPerTexel;
                vLightCameraOrthographicMin = vLightCameraOrthographicMin.Floor();
                vLightCameraOrthographicMin = vLightCameraOrthographicMin * vWorldUnitsPerTexel;

                vLightCameraOrthographicMax = vLightCameraOrthographicMax / vWorldUnitsPerTexel;
                vLightCameraOrthographicMax = vLightCameraOrthographicMax.Floor();
                vLightCameraOrthographicMax = vLightCameraOrthographicMax * vWorldUnitsPerTexel;


                //These are the unconfigured near and far plane values.  They are purposly awful to show 
                // how important calculating accurate near and far planes is.
                float fNearPlane;
                float fFarPlane;


                // By intersecting the light frustum with the scene AABB we can get a tighter bound on the near and far plane.
                ComputeNearAndFar(out fNearPlane, out fFarPlane, vLightCameraOrthographicMin, vLightCameraOrthographicMax, vSceneAABBPointsLightSpace);


                // Create the orthographic projection for this cascade.
                cascade.Ortho = Matrix.OrthoOffCenterLH(vLightCameraOrthographicMin.X, vLightCameraOrthographicMax.X, vLightCameraOrthographicMin.Y, vLightCameraOrthographicMax.Y, fNearPlane, fFarPlane);

                cascade.ZNear = fNearPlane;
                cascade.ZFar = fFarPlane;
                cascade.IntervalNear = fFrustumIntervalBegin;
                cascade.IntervalFar = fFrustumIntervalEnd;

                cascade.Matrix = Matrix.Multiply(LightView, cascade.Ortho);
                cascade.MatrixInv = Matrix.Invert(cascade.Matrix);
                cascade.WorldUnitsPerTexel = fWorldUnitsPerTexel;
                cascade.WorldUnitsToCascadeUnits = 2.0f / fCascadeBound;
            }


            context.ClearDepthStencilView(DepthTextureDSV, DepthStencilClearFlags.Depth, 1.0f, 0);
            // Set a null render target so as not to render color.
            context.OutputMerger.SetRenderTargets(DepthTextureDSV, (RenderTargetView)null);

            context.OutputMerger.SetDepthStencilState(DepthRenderDS);

            context.Rasterizer.State = DepthRenderRS;

        }

        public void BeginDepthRender(DeviceContext context, int cascadeIndex)
        {
            ShadowmapCascade cascade = Cascades[cascadeIndex];

            context.Rasterizer.SetViewport(cascade.DepthRenderVP);
        }

        public void EndUpdate(DeviceContext context)
        {
            RTS.Reset(context);
        }

        public void SetFinalRenderResources(DeviceContext context)
        {

            ShadowVars.Vars.CamScenePos = new Vector4(SceneCamPos, 0.0f); //in shadow scene coords
            ShadowVars.Vars.CamSceneView = Matrix.Transpose(SceneCamView);
            ShadowVars.Vars.LightView = Matrix.Transpose(LightView);
            ShadowVars.Vars.LightDir = new Vector4(LightDirection, 0.0f);

            Matrix dxmatTextureScale = Matrix.Scaling(0.5f, -0.5f, 1.0f);
            Matrix dxmatTextureTranslation = Matrix.Translation(0.5f, 0.5f, 0.0f);
            Matrix dxmatTextureST = Matrix.Multiply(dxmatTextureScale, dxmatTextureTranslation);

            for (int i = 0; i < CascadeCount; ++i)
            {
                ShadowmapCascade cascade = Cascades[i];
                Matrix mShadowTexture = Matrix.Multiply(cascade.Ortho, dxmatTextureST);
                ShadowVars.Vars.CascadeScales.Set(i, new Vector4(mShadowTexture.M11, mShadowTexture.M22, mShadowTexture.M33, 1.0f));
                ShadowVars.Vars.CascadeOffsets.Set(i, new Vector4(mShadowTexture.M41, mShadowTexture.M42, mShadowTexture.M43, 0.0f));
                ShadowVars.Vars.CascadeDepths.Set(i, new Vector4(cascade.IntervalFar, 0.0f, 0.0f, 0.0f));
            }

            ShadowVars.Vars.CascadeCount = CascadeCount;
            ShadowVars.Vars.CascadeVisual = 0;

            ShadowVars.Vars.PCFLoopStart = (PCFSize) / -2;
            ShadowVars.Vars.PCFLoopEnd = (PCFSize) / 2 + 1;


            // The border padding values keep the pixel shader from reading the borders during PCF filtering.
            float txs = (float)TextureSize;
            ShadowVars.Vars.BorderPaddingMax = (txs - 1.0f) / txs;
            ShadowVars.Vars.BorderPaddingMin = 1.0f / txs;

            ShadowVars.Vars.Bias = PCFOffset;
            ShadowVars.Vars.BlurBetweenCascades = BlurBetweenCascades;
            ShadowVars.Vars.CascadeCountInv = 1.0f / (float)CascadeCount;
            ShadowVars.Vars.TexelSize = 1.0f / txs;
            ShadowVars.Vars.TexelSizeX = ShadowVars.Vars.TexelSize / (float)CascadeCount;
            ShadowVars.Vars.ShadowMaxDistance = maxShadowDistance;

            ShadowVars.Update(context);


            SetBuffers(context);

        }

        public void SetBuffers(DeviceContext context)
        {
            context.VertexShader.SetConstantBuffer(1, ShadowVars.Buffer); //todo: set resource slots
            context.PixelShader.SetConstantBuffer(1, ShadowVars.Buffer);
            context.PixelShader.SetShaderResource(1, DepthTextureSRV);
            context.PixelShader.SetSampler(1, DepthTextureSS);
        }




        static readonly Vector3[] vExtentsMap =
        {
            new Vector3(1.0f, 1.0f, -1.0f),
            new Vector3( -1.0f, 1.0f, -1.0f ),
            new Vector3(1.0f, -1.0f, -1.0f ),
            new Vector3( -1.0f, -1.0f, -1.0f ),
            new Vector3( 1.0f, 1.0f, 1.0f ),
            new Vector3( -1.0f, 1.0f, 1.0f ),
            new Vector3( 1.0f, -1.0f, 1.0f ),
            new Vector3( -1.0f, -1.0f, 1.0f )
        };
        void CreateAABBPoints(ref Vector4[] vAABBPoints, Vector3 vCenter, Vector3 vExtents)
        {
            //--------------------------------------------------------------------------------------
            // This function converts the "center, extents" version of an AABB into 8 points.
            //--------------------------------------------------------------------------------------
            //This map enables us to use a for loop and do vector math.
            for (int index = 0; index < 8; ++index)
            {
                vAABBPoints[index] = new Vector4((vExtentsMap[index] * vExtents) + vCenter, 1.0f);
            }
        }

        static readonly Vector4[] HomogeneousPoints =
        {
            // Corners of the projection frustum in homogeneous space.
            new Vector4( 1.0f,  0.0f, 1.0f, 1.0f ),   // right (at far plane)
            new Vector4( -1.0f,  0.0f, 1.0f, 1.0f ),   // left
		    new Vector4( 0.0f,  1.0f, 1.0f, 1.0f ),   // top
		    new Vector4( 0.0f, -1.0f, 1.0f, 1.0f ),   // bottom
		    new Vector4( 0.0f, 0.0f, 0.0f, 1.0f ),     // near
		    new Vector4( 0.0f, 0.0f, 1.0f, 1.0f )      // far
	    };
        void ComputeFrustumFromProjection(out ShadowmapFrustum sf, Matrix pProjection)
        {
            //-----------------------------------------------------------------------------
            // Build a frustum from a persepective projection matrix.  The matrix may only
            // contain a projection; any rotation, translation or scale will cause the
            // constructed frustum to be incorrect.
            //-----------------------------------------------------------------------------


            Matrix matInverse;

            Matrix.Invert(ref pProjection, out matInverse);

            // Compute the frustum corners in world space.
            Vector4[] Points = new Vector4[6];

            for (int i = 0; i < 6; i++)
            {
                // Transform point.
                Points[i] = matInverse.Multiply(HomogeneousPoints[i]);
            }

            sf = new ShadowmapFrustum();

            sf.Origin = new Vector3(0.0f, 0.0f, 0.0f);
            sf.Orientation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);

            // Compute the slopes.
            Points[0] = Points[0] * (1.0f / Points[0].Z);
            Points[1] = Points[1] * (1.0f / Points[1].Z);
            Points[2] = Points[2] * (1.0f / Points[2].Z);
            Points[3] = Points[3] * (1.0f / Points[3].Z);

            sf.RightSlope = Points[0].X;
            sf.LeftSlope = Points[1].X;
            sf.TopSlope = Points[2].Y;
            sf.BottomSlope = Points[3].Y;

            // Compute near and far.
            Points[4] = Points[4] * (1.0f / Points[4].W);
            Points[5] = Points[5] * (1.0f / Points[5].W);

            sf.Near = Points[4].Z;
            sf.Far = Points[5].Z;
        }


        void CreateFrustumPointsFromCascadeInterval(float fCascadeIntervalBegin, float fCascadeIntervalEnd, Matrix vProjection, ref Vector4[] pvCornerPointsWorld)
        {
            //--------------------------------------------------------------------------------------
            // This function takes the camera's projection matrix and returns the 8
            // points that make up a view frustum.
            // The frustum is scaled to fit within the Begin and End interval paramaters.
            //--------------------------------------------------------------------------------------

            ShadowmapFrustum vViewFrust;

            ComputeFrustumFromProjection(out vViewFrust, vProjection);
            vViewFrust.Near = -fCascadeIntervalBegin; //negative due to negative aspect ratio projection matrix..
            vViewFrust.Far = -fCascadeIntervalEnd;

            Vector3 vGrabY = new Vector3(0.0f, 1.0f, 0.0f);
            Vector3 vGrabX = new Vector3(1.0f, 0.0f, 0.0f);

            Vector3 vRightTop = new Vector3(vViewFrust.RightSlope, vViewFrust.TopSlope, 1.0f);
            Vector3 vLeftBottom = new Vector3(vViewFrust.LeftSlope, vViewFrust.BottomSlope, 1.0f);
            Vector3 vNear = new Vector3(vViewFrust.Near, vViewFrust.Near, vViewFrust.Near);
            Vector3 vFar = new Vector3(vViewFrust.Far, vViewFrust.Far, vViewFrust.Far);
            Vector3 vRightTopNear = vRightTop * vNear;
            Vector3 vRightTopFar = vRightTop * vFar;
            Vector3 vLeftBottomNear = vLeftBottom * vNear;
            Vector3 vLeftBottomFar = vLeftBottom * vFar;

            pvCornerPointsWorld[0] = new Vector4(vRightTopNear, 1.0f);
            pvCornerPointsWorld[1] = new Vector4(V3FSelect(vRightTopNear, vLeftBottomNear, vGrabX), 1.0f);
            pvCornerPointsWorld[2] = new Vector4(vLeftBottomNear, 1.0f);
            pvCornerPointsWorld[3] = new Vector4(V3FSelect(vRightTopNear, vLeftBottomNear, vGrabY), 1.0f);

            pvCornerPointsWorld[4] = new Vector4(vRightTopFar, 1.0f);
            pvCornerPointsWorld[5] = new Vector4(V3FSelect(vRightTopFar, vLeftBottomFar, vGrabX), 1.0f);
            pvCornerPointsWorld[6] = new Vector4(vLeftBottomFar, 1.0f);
            pvCornerPointsWorld[7] = new Vector4(V3FSelect(vRightTopFar, vLeftBottomFar, vGrabY), 1.0f);
        }
        Vector3 V3FSelect(Vector3 v1, Vector3 v2, Vector3 control)
        {
            Vector3 r;
            r.X = (control.X == 0.0f) ? v1.X : v2.X;
            r.Y = (control.Y == 0.0f) ? v1.Y : v2.Y;
            r.Z = (control.Z == 0.0f) ? v1.Z : v2.Z;
            return r;
        }

        static readonly int[] iAABBTriIndexes =
        {
            // These are the indices used to tesselate an AABB into a list of triangles.
            0,1,2,  1,2,3,
            4,5,6,  5,6,7,
            0,2,4,  2,4,6,
            1,3,5,  3,5,7,
            0,1,4,  1,4,5,
            2,3,6,  3,6,7
        };
        void ComputeNearAndFar(out float fNearPlane, out float fFarPlane, Vector4 vLightCameraOrthographicMin, Vector4 vLightCameraOrthographicMax, Vector4[] pvPointsInCameraView)
        {
            //--------------------------------------------------------------------------------------
            // Computing an accurate near and flar plane will decrease surface acne and Peter-panning.
            // Surface acne is the term for erroneous self shadowing.  Peter-panning is the effect where
            // shadows disappear near the base of an object.
            // As offsets are generally used with PCF filtering due self shadowing issues, computing the
            // correct near and far planes becomes even more important.
            // This concept is not complicated, but the intersection code is.
            //--------------------------------------------------------------------------------------

            // Initialize the near and far planes
            fNearPlane = float.MaxValue;
            fFarPlane = float.MinValue;

            ShadowmapTriangle[] triangleList = new ShadowmapTriangle[16];
            int iTriangleCnt = 1;

            triangleList[0].pt0 = pvPointsInCameraView[0];
            triangleList[0].pt1 = pvPointsInCameraView[1];
            triangleList[0].pt2 = pvPointsInCameraView[2];
            triangleList[0].culled = false;


            int[] iPointPassesCollision = new int[3];

            // At a high level: 
            // 1. Iterate over all 12 triangles of the AABB.  
            // 2. Clip the triangles against each plane. Create new triangles as needed.
            // 3. Find the min and max z values as the near and far plane.

            //This is easier because the triangles are in camera spacing making the collisions tests simple comparisions.

            float fLightCameraOrthographicMinX = vLightCameraOrthographicMin.X;
            float fLightCameraOrthographicMaxX = vLightCameraOrthographicMax.X;
            float fLightCameraOrthographicMinY = vLightCameraOrthographicMin.Y;
            float fLightCameraOrthographicMaxY = vLightCameraOrthographicMax.Y;

            for (int AABBTriIter = 0; AABBTriIter < 12; ++AABBTriIter)
            {

                triangleList[0].pt0 = pvPointsInCameraView[iAABBTriIndexes[AABBTriIter * 3 + 0]];
                triangleList[0].pt1 = pvPointsInCameraView[iAABBTriIndexes[AABBTriIter * 3 + 1]];
                triangleList[0].pt2 = pvPointsInCameraView[iAABBTriIndexes[AABBTriIter * 3 + 2]];
                iTriangleCnt = 1;
                triangleList[0].culled = false;

                // Clip each invidual triangle against the 4 frustums.  When ever a triangle is clipped into new triangles, 
                //add them to the list.
                for (int frustumPlaneIter = 0; frustumPlaneIter < 4; ++frustumPlaneIter)
                {

                    float fEdge;
                    int iComponent;

                    if (frustumPlaneIter == 0)
                    {
                        fEdge = fLightCameraOrthographicMinX; // todo make float temp
                        iComponent = 0;
                    }
                    else if (frustumPlaneIter == 1)
                    {
                        fEdge = fLightCameraOrthographicMaxX;
                        iComponent = 0;
                    }
                    else if (frustumPlaneIter == 2)
                    {
                        fEdge = fLightCameraOrthographicMinY;
                        iComponent = 1;
                    }
                    else
                    {
                        fEdge = fLightCameraOrthographicMaxY;
                        iComponent = 1;
                    }

                    for (int triIter = 0; triIter < iTriangleCnt; ++triIter)
                    {
                        // We don't delete triangles, so we skip those that have been culled.
                        if (!triangleList[triIter].culled)
                        {
                            int iInsideVertCount = 0;
                            Vector4 tempOrder;
                            // Test against the correct frustum plane.
                            // This could be written more compactly, but it would be harder to understand.

                            if (frustumPlaneIter == 0)
                            {
                                for (int triPtIter = 0; triPtIter < 3; ++triPtIter)
                                {
                                    if (triangleList[triIter].pt(triPtIter).X > vLightCameraOrthographicMin.X)
                                    {
                                        iPointPassesCollision[triPtIter] = 1;
                                    }
                                    else
                                    {
                                        iPointPassesCollision[triPtIter] = 0;
                                    }
                                    iInsideVertCount += iPointPassesCollision[triPtIter];
                                }
                            }
                            else if (frustumPlaneIter == 1)
                            {
                                for (int triPtIter = 0; triPtIter < 3; ++triPtIter)
                                {
                                    if (triangleList[triIter].pt(triPtIter).X < vLightCameraOrthographicMax.X)
                                    {
                                        iPointPassesCollision[triPtIter] = 1;
                                    }
                                    else
                                    {
                                        iPointPassesCollision[triPtIter] = 0;
                                    }
                                    iInsideVertCount += iPointPassesCollision[triPtIter];
                                }
                            }
                            else if (frustumPlaneIter == 2)
                            {
                                for (int triPtIter = 0; triPtIter < 3; ++triPtIter)
                                {
                                    if (triangleList[triIter].pt(triPtIter).Y > vLightCameraOrthographicMin.Y)
                                    {
                                        iPointPassesCollision[triPtIter] = 1;
                                    }
                                    else
                                    {
                                        iPointPassesCollision[triPtIter] = 0;
                                    }
                                    iInsideVertCount += iPointPassesCollision[triPtIter];
                                }
                            }
                            else
                            {
                                for (int triPtIter = 0; triPtIter < 3; ++triPtIter)
                                {
                                    if (triangleList[triIter].pt(triPtIter).Y < vLightCameraOrthographicMax.Y)
                                    {
                                        iPointPassesCollision[triPtIter] = 1;
                                    }
                                    else
                                    {
                                        iPointPassesCollision[triPtIter] = 0;
                                    }
                                    iInsideVertCount += iPointPassesCollision[triPtIter];
                                }
                            }

                            // Move the points that pass the frustum test to the begining of the array.
                            if ((iPointPassesCollision[1] != 0) && (iPointPassesCollision[0] == 0))
                            {
                                tempOrder = triangleList[triIter].pt0;
                                triangleList[triIter].pt0 = triangleList[triIter].pt1;
                                triangleList[triIter].pt1 = tempOrder;
                                iPointPassesCollision[0] = 1;
                                iPointPassesCollision[1] = 0;
                            }
                            if ((iPointPassesCollision[2] != 0) && (iPointPassesCollision[1] == 0))
                            {
                                tempOrder = triangleList[triIter].pt1;
                                triangleList[triIter].pt1 = triangleList[triIter].pt2;
                                triangleList[triIter].pt2 = tempOrder;
                                iPointPassesCollision[1] = 1;
                                iPointPassesCollision[2] = 0;
                            }
                            if ((iPointPassesCollision[1] != 0) && (iPointPassesCollision[0] == 0))
                            {
                                tempOrder = triangleList[triIter].pt0;
                                triangleList[triIter].pt0 = triangleList[triIter].pt1;
                                triangleList[triIter].pt1 = tempOrder;
                                iPointPassesCollision[0] = 1;
                                iPointPassesCollision[1] = 0;
                            }

                            if (iInsideVertCount == 0)
                            { // All points failed. We're done,  
                                triangleList[triIter].culled = true;
                            }
                            else if (iInsideVertCount == 1)
                            {// One point passed. Clip the triangle against the Frustum plane
                                triangleList[triIter].culled = false;

                                // 
                                Vector4 vVert0ToVert1 = triangleList[triIter].pt1 - triangleList[triIter].pt0;
                                Vector4 vVert0ToVert2 = triangleList[triIter].pt2 - triangleList[triIter].pt0;

                                // Find the collision ratio.
                                float fHitPointTimeRatio = fEdge - triangleList[triIter].pt0[iComponent];
                                // Calculate the distance along the vector as ratio of the hit ratio to the component.
                                float fDistanceAlongVector01 = fHitPointTimeRatio / vVert0ToVert1[iComponent];
                                float fDistanceAlongVector02 = fHitPointTimeRatio / vVert0ToVert2[iComponent];
                                // Add the point plus a percentage of the vector.
                                vVert0ToVert1 = vVert0ToVert1 * fDistanceAlongVector01;
                                vVert0ToVert1 = vVert0ToVert1 + triangleList[triIter].pt0;
                                vVert0ToVert2 = vVert0ToVert2 * fDistanceAlongVector02;
                                vVert0ToVert2 = vVert0ToVert2 + triangleList[triIter].pt0;

                                triangleList[triIter].pt1 = vVert0ToVert2;
                                triangleList[triIter].pt2 = vVert0ToVert1;

                            }
                            else if (iInsideVertCount == 2)
                            { // 2 in  // tesselate into 2 triangles


                                // Copy the triangle\(if it exists) after the current triangle out of
                                // the way so we can override it with the new triangle we're inserting.
                                triangleList[iTriangleCnt] = triangleList[triIter + 1];

                                triangleList[triIter].culled = false;
                                triangleList[triIter + 1].culled = false;

                                // Get the vector from the outside point into the 2 inside points.
                                Vector4 vVert2ToVert0 = triangleList[triIter].pt0 - triangleList[triIter].pt2;
                                Vector4 vVert2ToVert1 = triangleList[triIter].pt1 - triangleList[triIter].pt2;

                                // Get the hit point ratio.
                                float fHitPointTime_2_0 = fEdge - triangleList[triIter].pt2[iComponent];
                                float fDistanceAlongVector_2_0 = fHitPointTime_2_0 / vVert2ToVert0[iComponent];
                                // Calcaulte the new vert by adding the percentage of the vector plus point 2.
                                vVert2ToVert0 = vVert2ToVert0 * fDistanceAlongVector_2_0;
                                vVert2ToVert0 = vVert2ToVert0 + triangleList[triIter].pt2;

                                // Add a new triangle.
                                triangleList[triIter + 1].pt0 = triangleList[triIter].pt0;
                                triangleList[triIter + 1].pt1 = triangleList[triIter].pt1;
                                triangleList[triIter + 1].pt2 = vVert2ToVert0;

                                //Get the hit point ratio.
                                float fHitPointTime_2_1 = fEdge - triangleList[triIter].pt2[iComponent];
                                float fDistanceAlongVector_2_1 = fHitPointTime_2_1 / vVert2ToVert1[iComponent];
                                vVert2ToVert1 = vVert2ToVert1 * fDistanceAlongVector_2_1;
                                vVert2ToVert1 = vVert2ToVert1 + triangleList[triIter].pt2;
                                triangleList[triIter].pt0 = triangleList[triIter + 1].pt1;
                                triangleList[triIter].pt1 = triangleList[triIter + 1].pt2;
                                triangleList[triIter].pt2 = vVert2ToVert1;
                                // Increment triangle count and skip the triangle we just inserted.
                                ++iTriangleCnt;
                                ++triIter;


                            }
                            else
                            { // all in
                                triangleList[triIter].culled = false;

                            }
                        }// end if !culled loop            
                    }
                }
                for (int index = 0; index < iTriangleCnt; ++index)
                {
                    if (!triangleList[index].culled)
                    {
                        // Set the near and far plan and the min and max z values respectivly.
                        for (int vertind = 0; vertind < 3; ++vertind)
                        {
                            float fTriangleCoordZ = triangleList[index].pt(vertind).Z;
                            if (fNearPlane > fTriangleCoordZ)
                            {
                                fNearPlane = fTriangleCoordZ;
                            }
                            if (fFarPlane < fTriangleCoordZ)
                            {
                                fFarPlane = fTriangleCoordZ;
                            }
                        }
                    }
                }
            }

        }


    }



    public struct ShadowmapVars
    {
        public Vector4 CamScenePos; //in shadow scene coords
        public Matrix CamSceneView;
        public Matrix LightView;
        public Vector4 LightDir;
        public ShadowmapVarsCascadeData CascadeOffsets;
        public ShadowmapVarsCascadeData CascadeScales;
        public ShadowmapVarsCascadeData CascadeDepths; //in scene eye space
        public int CascadeCount;
        public int CascadeVisual;
        public int PCFLoopStart;
        public int PCFLoopEnd;
        public float BorderPaddingMin;
        public float BorderPaddingMax;
        public float Bias;
        public float BlurBetweenCascades;
        public float CascadeCountInv;
        public float TexelSize;
        public float TexelSizeX;
        public float ShadowMaxDistance;
    }

    public struct ShadowmapVarsCascadeData
    {
        public Vector4 V00;
        public Vector4 V01;
        public Vector4 V02;
        public Vector4 V03;
        public Vector4 V04;
        public Vector4 V05;
        public Vector4 V06;
        public Vector4 V07;
        public Vector4 V08;
        public Vector4 V09;
        public Vector4 V10;
        public Vector4 V11;
        public Vector4 V12;
        public Vector4 V13;
        public Vector4 V14;
        public Vector4 V15;

        public void Set(int index, Vector4 v)
        {
            switch (index)
            {
                case 0: V00 = v; break;
                case 1: V01 = v; break;
                case 2: V02 = v; break;
                case 3: V03 = v; break;
                case 4: V04 = v; break;
                case 5: V05 = v; break;
                case 6: V06 = v; break;
                case 7: V07 = v; break;
                case 8: V08 = v; break;
                case 9: V09 = v; break;
                case 10: V10 = v; break;
                case 11: V11 = v; break;
                case 12: V12 = v; break;
                case 13: V13 = v; break;
                case 14: V14 = v; break;
                case 15: V15 = v; break;
            }
        }
        public Vector4 Get(int index)
        {
            switch (index)
            {
                case 0: return V00;
                case 1: return V01;
                case 2: return V02;
                case 3: return V03;
                case 4: return V04;
                case 5: return V05;
                case 6: return V06;
                case 7: return V07;
                case 8: return V08;
                case 9: return V09;
                case 10: return V10;
                case 11: return V11;
                case 12: return V12;
                case 13: return V13;
                case 14: return V14;
                case 15: return V15;
            }
            return Vector4.Zero;
        }

    }

    public class ShadowmapCascade
    {
        public Shadowmap Owner { get; set; }
        public int Index { get; set; }
        public float IntervalNear { get; set; }
        public float IntervalFar { get; set; }
        public float ZNear { get; set; }
        public float ZFar { get; set; }
        public Matrix Ortho { get; set; }
        public Matrix Matrix { get; set; }
        public Matrix MatrixInv { get; set; }
        public ViewportF DepthRenderVP { get; set; }
        public float WorldUnitsPerTexel { get; set; } //updated each frame for culling
        public float WorldUnitsToCascadeUnits { get; set; }
    }

    public struct ShadowmapFrustum
    {
        public Vector3 Origin;            // Origin of the frustum (and projection).
        public Quaternion Orientation;       // Unit quaternion representing rotation.
        public float RightSlope;           // Positive X slope (X/Z).
        public float LeftSlope;            // Negative X slope.
        public float TopSlope;             // Positive Y slope (Y/Z).
        public float BottomSlope;          // Negative Y slope.
        public float Near, Far;            // Z of the near plane and far plane.
    }

    public struct ShadowmapTriangle
    {
        //--------------------------------------------------------------------------------------
        // Used to compute an intersection of the orthographic projection and the Scene AABB
        //--------------------------------------------------------------------------------------
        public Vector4 pt0;
        public Vector4 pt1;
        public Vector4 pt2;
        public bool culled;

        public Vector4 pt(int i)
        {
            switch (i)
            {
                default:
                case 0: return pt0;
                case 1: return pt1;
                case 2: return pt2;
            }
        }
        public void pt(int i, Vector4 v)
        {
            switch (i)
            {
                default:
                case 0: pt0 = v; break;
                case 1: pt1 = v; break;
                case 2: pt2 = v; break;
            }
        }
    }


}
