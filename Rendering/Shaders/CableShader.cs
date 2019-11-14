using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using SharpDX;
using CodeWalker.GameFiles;
using CodeWalker.World;

namespace CodeWalker.Rendering
{

    public struct CableShaderVSSceneVars
    {
        public Matrix ViewProj;
    }
    public struct CableShaderVSEntityVars
    {
        public Vector4 CamRel;
        public Quaternion Orientation;
        public uint HasSkeleton;
        public uint HasTransforms;
        public uint TintPaletteIndex;
        public uint Pad1;
        public Vector3 Scale;
        public uint IsInstanced;
    }
    public struct CableShaderVSModelVars
    {
        public Matrix Transform;
    }
    public struct CableShaderVSGeomVars
    {
        public uint EnableTint;
        public float TintYVal;
        public uint IsDecal;
        public uint EnableWind;
        public Vector4 WindOverrideParams;
        public Vector4 globalAnimUV0;
        public Vector4 globalAnimUV1;
    }
    public struct CableShaderPSSceneVars
    {
        public ShaderGlobalLightParams GlobalLights;
        public uint EnableShadows;
        public uint RenderMode;//0=default, 1=normals, 2=tangents, 3=colours, 4=texcoords, 5=diffuse, 6=normalmap, 7=spec, 8=direct
        public uint RenderModeIndex; //colour/texcoord index
        public uint RenderSamplerCoord; //which texcoord to use in single texture mode
    }
    public struct CableShaderPSGeomVars
    {
        public uint EnableTexture;
        public uint EnableTint;
        public uint Pad100;
        public uint Pad101;
    }

    public class CableShader : Shader, IDisposable
    {
        bool disposed = false;

        VertexShader vs;
        PixelShader ps;
        GpuVarsBuffer<CableShaderVSSceneVars> VSSceneVars;
        GpuVarsBuffer<CableShaderVSEntityVars> VSEntityVars;
        GpuVarsBuffer<CableShaderVSModelVars> VSModelVars;
        GpuVarsBuffer<CableShaderVSGeomVars> VSGeomVars;
        GpuVarsBuffer<CableShaderPSSceneVars> PSSceneVars;
        GpuVarsBuffer<CableShaderPSGeomVars> PSGeomVars;
        SamplerState texsampler;

        public WorldRenderMode RenderMode = WorldRenderMode.Default;
        public int RenderVertexColourIndex = 1;
        public int RenderTextureCoordIndex = 1;
        public int RenderTextureSamplerCoord = 1;
        public ShaderParamNames RenderTextureSampler = ShaderParamNames.DiffuseSampler;


        private Dictionary<VertexType, InputLayout> layouts = new Dictionary<VertexType, InputLayout>();

        public CableShader(Device device)
        {
            byte[] vsbytes = File.ReadAllBytes("Shaders\\CableVS.cso");
            byte[] psbytes = File.ReadAllBytes("Shaders\\CablePS.cso");

            vs = new VertexShader(device, vsbytes);
            ps = new PixelShader(device, psbytes);


            VSSceneVars = new GpuVarsBuffer<CableShaderVSSceneVars>(device);
            VSEntityVars = new GpuVarsBuffer<CableShaderVSEntityVars>(device);
            VSModelVars = new GpuVarsBuffer<CableShaderVSModelVars>(device);
            VSGeomVars = new GpuVarsBuffer<CableShaderVSGeomVars>(device);
            PSSceneVars = new GpuVarsBuffer<CableShaderPSSceneVars>(device);
            PSGeomVars = new GpuVarsBuffer<CableShaderPSGeomVars>(device);


            //supported layout - requires Position, Normal, Colour, Texcoord
            layouts.Add(VertexType.Default, new InputLayout(device, vsbytes, VertexTypeGTAV.GetLayout(VertexType.Default)));



            texsampler = new SamplerState(device, new SamplerStateDescription()
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                BorderColor = Color.Black,
                ComparisonFunction = Comparison.Always,
                Filter = Filter.MinMagMipLinear,
                MaximumAnisotropy = 1,
                MaximumLod = float.MaxValue,
                MinimumLod = 0,
                MipLodBias = 0,
            });
        }


        private void SetVertexShader(DeviceContext context, VertexType type)
        {
            switch (type)
            {
                case VertexType.Default:
                    break;
                default:
                    break;
            }
            context.VertexShader.Set(vs);
        }


        public override void SetShader(DeviceContext context)
        {
            context.PixelShader.Set(ps);
        }

        public override bool SetInputLayout(DeviceContext context, VertexType type)
        {
            InputLayout l;
            if (layouts.TryGetValue(type, out l))
            {
                SetVertexShader(context, type);
                context.InputAssembler.InputLayout = l;
                return true;
            }
            return false;
        }

        public override void SetSceneVars(DeviceContext context, Camera camera, Shadowmap shadowmap, ShaderGlobalLights lights)
        {
            uint rendermode = 0;
            uint rendermodeind = 1;

            switch (RenderMode)
            {
                case WorldRenderMode.VertexNormals:
                    rendermode = 1;
                    break;
                case WorldRenderMode.VertexTangents:
                    rendermode = 2;
                    break;
                case WorldRenderMode.VertexColour:
                    rendermode = 3;
                    rendermodeind = (uint)RenderVertexColourIndex;
                    break;
                case WorldRenderMode.TextureCoord:
                    rendermode = 4;
                    rendermodeind = (uint)RenderTextureCoordIndex;
                    break;
                case WorldRenderMode.SingleTexture:
                    rendermode = 8;//direct mode
                    break;
            }


            VSSceneVars.Vars.ViewProj = Matrix.Transpose(camera.ViewProjMatrix);
            VSSceneVars.Update(context);
            VSSceneVars.SetVSCBuffer(context, 0);

            PSSceneVars.Vars.GlobalLights = lights.Params;
            PSSceneVars.Vars.EnableShadows = (shadowmap != null) ? 1u : 0u;
            PSSceneVars.Vars.RenderMode = rendermode;
            PSSceneVars.Vars.RenderModeIndex = rendermodeind;
            PSSceneVars.Vars.RenderSamplerCoord = (uint)RenderTextureSamplerCoord;
            PSSceneVars.Update(context);
            PSSceneVars.SetPSCBuffer(context, 0);

            if (shadowmap != null)
            {
                shadowmap.SetFinalRenderResources(context);
            }
        }

        public override void SetEntityVars(DeviceContext context, ref RenderableInst rend)
        {
            VSEntityVars.Vars.CamRel = new Vector4(rend.CamRel, 0.0f);
            VSEntityVars.Vars.Orientation = rend.Orientation;
            VSEntityVars.Vars.Scale = rend.Scale;
            VSEntityVars.Vars.HasSkeleton = rend.Renderable.HasSkeleton ? 1u : 0;
            VSEntityVars.Vars.HasTransforms = rend.Renderable.HasTransforms ? 1u : 0;
            VSEntityVars.Vars.TintPaletteIndex = rend.TintPaletteIndex;
            VSEntityVars.Vars.IsInstanced = 0;
            VSEntityVars.Update(context);
            VSEntityVars.SetVSCBuffer(context, 2);
        }

        public override void SetModelVars(DeviceContext context, RenderableModel model)
        {
            if (!model.UseTransform) return;
            VSModelVars.Vars.Transform = Matrix.Transpose(model.Transform);
            VSModelVars.Update(context);
            VSModelVars.SetVSCBuffer(context, 3);
        }

        public override void SetGeomVars(DeviceContext context, RenderableGeometry geom)
        {
            RenderableTexture texture = null; // ((geom.Textures != null) && (geom.Textures.Length > 0)) ? geom.Textures[0] : null;

            if ((geom.RenderableTextures != null) && (geom.RenderableTextures.Length > 0))
            {

                if (RenderMode == WorldRenderMode.Default)
                {
                    for (int i = 0; i < geom.RenderableTextures.Length; i++)
                    {
                        var itex = geom.RenderableTextures[i];
                        var ihash = geom.TextureParamHashes[i];
                        switch (ihash)
                        {
                            case ShaderParamNames.DiffuseSampler:
                                texture = itex;
                                break;
                        }
                    }


                    ////fallback try get first texture... eventaully remove this! (helps with water for now)
                    //int index = 0;
                    //while (((texture == null) || (texture.Texture2D == null)) && (index < geom.Textures.Length))
                    //{
                    //    texture = geom.Textures[index];
                    //    index++;
                    //}
                }
                else if (RenderMode == WorldRenderMode.SingleTexture)
                {
                    for (int i = 0; i < geom.RenderableTextures.Length; i++)
                    {
                        var itex = geom.RenderableTextures[i];
                        var ihash = geom.TextureParamHashes[i];
                        if (ihash == RenderTextureSampler)
                        {
                            texture = itex;
                            break;
                        }

                    }
                }

            }


            bool usediff = ((texture != null) && (texture.Texture2D != null) && (texture.ShaderResourceView != null));

            PSGeomVars.Vars.EnableTexture = usediff ? 1u : 0u;
            PSGeomVars.Vars.EnableTint = 0u;
            PSGeomVars.Update(context);
            PSGeomVars.SetPSCBuffer(context, 2);

            VSGeomVars.Vars.EnableTint = 0u;
            VSGeomVars.Vars.TintYVal = 0u;
            VSGeomVars.Vars.IsDecal = 0u;
            VSGeomVars.Vars.EnableWind = 0u;
            VSGeomVars.Vars.WindOverrideParams = Vector4.Zero;
            VSGeomVars.Vars.globalAnimUV0 = Vector4.Zero;
            VSGeomVars.Vars.globalAnimUV1 = Vector4.Zero;
            VSGeomVars.Update(context);
            VSGeomVars.SetVSCBuffer(context, 4);


            //context.VertexShader.SetSampler(0, texsampler);
            context.PixelShader.SetSampler(0, texsampler);
            //context.PixelShader.SetSampler(1, texsamplerc);
            if (usediff)
            {
                texture.SetPSResource(context, 0);
                //context.PixelShader.SetShaderResource(0, difftex.ShaderResourceView);
            }
        }



        public override void UnbindResources(DeviceContext context)
        {
            context.VertexShader.SetConstantBuffer(0, null);
            context.PixelShader.SetConstantBuffer(0, null);
            context.VertexShader.SetConstantBuffer(1, null); //shadowmap
            context.PixelShader.SetConstantBuffer(1, null); //shadowmap
            context.PixelShader.SetShaderResource(1, null);//shadowmap
            context.PixelShader.SetSampler(1, null); //shadowmap
            context.VertexShader.SetConstantBuffer(2, null);
            context.VertexShader.SetConstantBuffer(3, null);
            context.PixelShader.SetConstantBuffer(2, null);
            context.VertexShader.SetConstantBuffer(4, null);
            context.VertexShader.SetSampler(0, null);
            context.PixelShader.SetSampler(0, null);
            context.PixelShader.SetShaderResource(0, null);
            context.VertexShader.SetShaderResource(0, null);
            context.VertexShader.SetShaderResource(1, null);
            context.VertexShader.Set(null);
            context.PixelShader.Set(null);
        }


        public void Dispose()
        {
            if (disposed) return;


            if (texsampler != null)
            {
                texsampler.Dispose();
                texsampler = null;
            }

            foreach (InputLayout layout in layouts.Values)
            {
                layout.Dispose();
            }
            layouts.Clear();

            VSSceneVars.Dispose();
            VSEntityVars.Dispose();
            VSModelVars.Dispose();
            VSGeomVars.Dispose();
            PSSceneVars.Dispose();
            PSGeomVars.Dispose();


            ps.Dispose();
            vs.Dispose();

            disposed = true;
        }
    }
}
