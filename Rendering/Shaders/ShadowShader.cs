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

    public struct ShadowShaderVSSceneVars
    {
        public Matrix ViewProj;
        public Vector4 WindVector;
    }
    public struct ShadowShaderVSEntityVars
    {
        public Vector4 CamRel;
        public Quaternion Orientation;
        public uint HasSkeleton;
        public uint HasTransforms;
        public uint TintPaletteIndex;
        public uint Pad1;
        public Vector3 Scale;
        public uint Pad2;
    }
    public struct ShadowShaderVSModelVars
    {
        public Matrix Transform;
    }
    //public struct ShadowShaderVSGeomVars
    //{
    //    public uint EnableTint;
    //    public float TintYVal;
    //    public uint IsDecal;
    //    public uint Pad5;
    //}
    public struct ShadowShaderGeomVars
    {
        public uint EnableTexture;
        public uint EnableTint;
        public uint IsDecal;
        public uint EnableWind;
        public Vector4 WindOverrideParams;
    }

    public class ShadowShader : Shader, IDisposable
    {
        bool disposed = false;

        VertexShader shadowvs;
        VertexShader shadowvs_skin;
        PixelShader shadowps;

        GpuVarsBuffer<ShadowShaderVSSceneVars> VSSceneVars;
        GpuVarsBuffer<ShadowShaderVSEntityVars> VSEntityVars;
        GpuVarsBuffer<ShadowShaderVSModelVars> VSModelVars;
        GpuVarsBuffer<ShadowShaderGeomVars> GeomVars;
        GpuABuffer<Matrix3_s> BoneMatrices;

        SamplerState texsampler;
        SamplerState texsamplerc;
        //public bool DecalMode = false;

        public Vector4 WindVector { get; set; }

        Matrix3_s[] defaultBoneMatrices;
        bool defaultBoneMatricesBound = false;


        private Dictionary<VertexType, InputLayout> layouts = new Dictionary<VertexType, InputLayout>();

        public ShadowShader(Device device)
        {
            byte[] vsbytes = File.ReadAllBytes("Shaders\\ShadowVS.cso");
            byte[] vssbytes = File.ReadAllBytes("Shaders\\ShadowVS_Skin.cso");
            byte[] psbytes = File.ReadAllBytes("Shaders\\ShadowPS.cso");

            shadowvs = new VertexShader(device, vsbytes);
            shadowvs_skin = new VertexShader(device, vssbytes);
            shadowps = new PixelShader(device, psbytes);


            VSSceneVars = new GpuVarsBuffer<ShadowShaderVSSceneVars>(device);
            VSEntityVars = new GpuVarsBuffer<ShadowShaderVSEntityVars>(device);
            VSModelVars = new GpuVarsBuffer<ShadowShaderVSModelVars>(device);
            GeomVars = new GpuVarsBuffer<ShadowShaderGeomVars>(device);
            BoneMatrices = new GpuABuffer<Matrix3_s>(device, 255);


            //supported layouts - requires Position, Normal, Colour, Texcoord
            layouts.Add(VertexType.Default, new InputLayout(device, vsbytes, VertexTypeGTAV.GetLayout(VertexType.Default)));
            layouts.Add(VertexType.DefaultEx, new InputLayout(device, vsbytes, VertexTypeGTAV.GetLayout(VertexType.DefaultEx)));
            layouts.Add(VertexType.PNCCT, new InputLayout(device, vsbytes, VertexTypeGTAV.GetLayout(VertexType.PNCCT)));
            layouts.Add(VertexType.PNCCTTTT, new InputLayout(device, vsbytes, VertexTypeGTAV.GetLayout(VertexType.PNCCTTTT)));
            layouts.Add(VertexType.PNCTTTX, new InputLayout(device, vsbytes, VertexTypeGTAV.GetLayout(VertexType.PNCTTTX)));
            layouts.Add(VertexType.PNCTTTX_2, new InputLayout(device, vsbytes, VertexTypeGTAV.GetLayout(VertexType.PNCTTTX_2)));
            layouts.Add(VertexType.PNCTTTX_3, new InputLayout(device, vsbytes, VertexTypeGTAV.GetLayout(VertexType.PNCTTTX_3)));
            layouts.Add(VertexType.PNCTTTTX, new InputLayout(device, vsbytes, VertexTypeGTAV.GetLayout(VertexType.PNCTTTTX)));
            layouts.Add(VertexType.PNCTTX, new InputLayout(device, vsbytes, VertexTypeGTAV.GetLayout(VertexType.PNCTTX)));
            layouts.Add(VertexType.PNCCTTX, new InputLayout(device, vsbytes, VertexTypeGTAV.GetLayout(VertexType.PNCCTTX)));
            layouts.Add(VertexType.PNCCTTX_2, new InputLayout(device, vsbytes, VertexTypeGTAV.GetLayout(VertexType.PNCCTTX_2)));
            layouts.Add(VertexType.PNCCTTTX, new InputLayout(device, vsbytes, VertexTypeGTAV.GetLayout(VertexType.PNCCTTTX)));
            layouts.Add(VertexType.PNCCTT, new InputLayout(device, vsbytes, VertexTypeGTAV.GetLayout(VertexType.PNCCTT)));
            layouts.Add(VertexType.PNCCTX, new InputLayout(device, vsbytes, VertexTypeGTAV.GetLayout(VertexType.PNCCTX)));
            layouts.Add(VertexType.PNCH2, new InputLayout(device, vsbytes, VertexTypeGTAV.GetLayout(VertexType.PNCH2, VertexDeclarationTypes.Types3)));
            layouts.Add(VertexType.PCCH2H4, new InputLayout(device, vsbytes, VertexTypeGTAV.GetLayout(VertexType.PCCH2H4, VertexDeclarationTypes.Types2)));
            layouts.Add(VertexType.PNCTT, new InputLayout(device, vsbytes, VertexTypeGTAV.GetLayout(VertexType.PNCTT)));
            layouts.Add(VertexType.PNCTTT, new InputLayout(device, vsbytes, VertexTypeGTAV.GetLayout(VertexType.PNCTTT)));

            layouts.Add(VertexType.PBBNCT, new InputLayout(device, vssbytes, VertexTypeGTAV.GetLayout(VertexType.PBBNCT)));
            layouts.Add(VertexType.PBBNCTX, new InputLayout(device, vssbytes, VertexTypeGTAV.GetLayout(VertexType.PBBNCTX)));
            layouts.Add(VertexType.PBBNCTT, new InputLayout(device, vssbytes, VertexTypeGTAV.GetLayout(VertexType.PBBNCTT)));
            layouts.Add(VertexType.PBBNCTTT, new InputLayout(device, vssbytes, VertexTypeGTAV.GetLayout(VertexType.PBBNCTTT)));
            layouts.Add(VertexType.PBBNCCT, new InputLayout(device, vssbytes, VertexTypeGTAV.GetLayout(VertexType.PBBNCCT)));
            layouts.Add(VertexType.PBBNCCTT, new InputLayout(device, vssbytes, VertexTypeGTAV.GetLayout(VertexType.PBBNCCTT)));//TODO
            layouts.Add(VertexType.PBBNCCTX, new InputLayout(device, vssbytes, VertexTypeGTAV.GetLayout(VertexType.PBBNCCTX)));
            layouts.Add(VertexType.PBBNCTTX, new InputLayout(device, vssbytes, VertexTypeGTAV.GetLayout(VertexType.PBBNCTTX)));
            layouts.Add(VertexType.PBBNCTTTX, new InputLayout(device, vssbytes, VertexTypeGTAV.GetLayout(VertexType.PBBNCTTTX)));//TODO
            layouts.Add(VertexType.PBBNCCTTX, new InputLayout(device, vssbytes, VertexTypeGTAV.GetLayout(VertexType.PBBNCCTTX)));//TODO
            //PBBCCT todo
            //PBBNC todo



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
            texsamplerc = new SamplerState(device, new SamplerStateDescription()
            {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                BorderColor = Color.Black,
                ComparisonFunction = Comparison.Always,
                Filter = Filter.MinMagMipPoint,
                MaximumAnisotropy = 1,
                MaximumLod = float.MaxValue,
                MinimumLod = 0,
                MipLodBias = 0,
            });



            defaultBoneMatrices = new Matrix3_s[255];
            for (int i = 0; i < 255; i++)
            {
                defaultBoneMatrices[i].Row1 = Vector4.UnitX;
                defaultBoneMatrices[i].Row2 = Vector4.UnitY;
                defaultBoneMatrices[i].Row3 = Vector4.UnitZ;
            }

        }



        public override void SetShader(DeviceContext context)
        {
            context.PixelShader.Set(shadowps);
        }

        public override bool SetInputLayout(DeviceContext context, VertexType type)
        {
            InputLayout l;
            if (layouts.TryGetValue(type, out l))
            {
                VertexShader vs = shadowvs;
                switch (type)
                {
                    case VertexType.PBBNCT:
                    case VertexType.PBBNCTX:
                    case VertexType.PBBNCTT:
                    case VertexType.PBBNCTTT:
                    case VertexType.PBBNCCT:
                    case VertexType.PBBNCCTT:
                    case VertexType.PBBNCCTX:
                    case VertexType.PBBNCTTX:
                    case VertexType.PBBNCTTTX:
                    case VertexType.PBBNCCTTX:
                        vs = shadowvs_skin;
                        break;
                }

                context.VertexShader.Set(vs);


                context.InputAssembler.InputLayout = l;
                return true;
            }
            return false;
        }

        public override void SetSceneVars(DeviceContext context, Camera camera, Shadowmap shadowmap, ShaderGlobalLights lights)
        {
        }
        public void SetSceneVars(DeviceContext context, Matrix shadowviewproj)
        {
            VSSceneVars.Vars.ViewProj = Matrix.Transpose(shadowviewproj);
            VSSceneVars.Vars.WindVector = WindVector;
            VSSceneVars.Update(context);
            VSSceneVars.SetVSCBuffer(context, 0);
        }

        public override void SetEntityVars(DeviceContext context, ref RenderableInst rend)
        {
            VSEntityVars.Vars.CamRel = new Vector4(rend.CamRel, 0.0f);
            VSEntityVars.Vars.Orientation = rend.Orientation;
            VSEntityVars.Vars.Scale = rend.Scale;
            VSEntityVars.Vars.HasSkeleton = rend.Renderable.HasSkeleton ? 1u : 0;
            VSEntityVars.Vars.HasTransforms = rend.Renderable.HasTransforms ? 1u : 0;
            VSEntityVars.Vars.TintPaletteIndex = rend.TintPaletteIndex;
            VSEntityVars.Update(context);
            VSEntityVars.SetVSCBuffer(context, 1);
        }

        public override void SetModelVars(DeviceContext context, RenderableModel model)
        {
            if (model.Owner.BoneTransforms != null)
            {
                SetBoneMatrices(context, model.Owner.BoneTransforms);
                defaultBoneMatricesBound = false;
            }
            else if (!defaultBoneMatricesBound)
            {
                SetBoneMatrices(context, defaultBoneMatrices);
                defaultBoneMatricesBound = true;
            }

            if (!model.UseTransform) return;
            VSModelVars.Vars.Transform = Matrix.Transpose(model.Transform);
            VSModelVars.Update(context);
            VSModelVars.SetVSCBuffer(context, 2);
        }

        public override void SetGeomVars(DeviceContext context, RenderableGeometry geom)
        {
            RenderableTexture texture = null; // ((geom.Textures != null) && (geom.Textures.Length > 0)) ? geom.Textures[0] : null;
            //RenderableTexture tintpal = null;

            //float tntpalind = 0.0f;

            if ((geom.RenderableTextures != null) && (geom.RenderableTextures.Length > 0))
            {
                for (int i = 0; i < geom.RenderableTextures.Length; i++)
                {
                    var itex = geom.RenderableTextures[i];
                    var ihash = geom.TextureParamHashes[i];
                    switch (ihash)
                    {
                        case ShaderParamNames.DiffuseSampler:
                        case ShaderParamNames.TextureSampler_layer0:
                        case ShaderParamNames.TextureSampler_layer1:
                        case ShaderParamNames.TextureSampler_layer2:
                        case ShaderParamNames.TextureSampler_layer3:
                            texture = itex;
                            break;
                    }
                    if (texture != null) break;
                }

                ////try get default diffuse texture
                //if ((geom.DiffuseSampler >= 0) && (geom.DiffuseSampler < geom.Textures.Length))
                //{
                //    texture = geom.Textures[geom.DiffuseSampler];
                //}
                //if ((texture == null) && (geom.TextureSampler_layer0 >= 0) && (geom.TextureSampler_layer0 < geom.Textures.Length))
                //{
                //    texture = geom.Textures[geom.TextureSampler_layer0];
                //}
                //if ((texture == null) && (geom.TextureSampler_layer1 >= 0) && (geom.TextureSampler_layer1 < geom.Textures.Length))
                //{
                //    texture = geom.Textures[geom.TextureSampler_layer1];
                //}
                //if ((texture == null) && (geom.TextureSampler_layer2 >= 0) && (geom.TextureSampler_layer2 < geom.Textures.Length))
                //{
                //    texture = geom.Textures[geom.TextureSampler_layer2];
                //}
                //if ((texture == null) && (geom.TextureSampler_layer3 >= 0) && (geom.TextureSampler_layer3 < geom.Textures.Length))
                //{
                //    texture = geom.Textures[geom.TextureSampler_layer3];
                //}

                //fallback try get first texture... eventaully remove this! (helps with water for now)
                int index = 0;
                while (((texture == null)) && (index < geom.RenderableTextures.Length))
                {
                    texture = geom.RenderableTextures[index];
                    index++;
                }


                //if ((geom.PaletteTexture >= 0) && (geom.PaletteTexture < geom.Textures.Length))
                //{
                //    tintpal = geom.Textures[geom.PaletteTexture];
                //    if (tintpal.Texture != null)
                //    {
                //        //this is slightly dodgy but vsentvarsdata should have the correct value in it...
                //        tntpalind = (vsentvarsdata.TintPaletteIndex + 0.5f) / tintpal.Texture.Height;
                //    }
                //}
            }


            bool usediff = ((texture != null) && (texture.ShaderResourceView != null));


            uint windflag = 0;
            var shaderFile = geom.DrawableGeom.Shader.FileName;
            switch (shaderFile.Hash)
            {
                case 2245870123: //trees_normal_diffspec_tnt.sps
                case 3334613197: //trees_tnt.sps
                case 1229591973://{trees_normal_spec_tnt.sps}
                case 2322653400://{trees.sps}
                case 3192134330://{trees_normal.sps}
                case 1224713457://{trees_normal_spec.sps}
                case 4265705004://{trees_normal_diffspec.sps}
                    windflag = 1;
                    break;
            }




            GeomVars.Vars.EnableTexture = usediff ? 1u : 0u;
            GeomVars.Vars.EnableTint = 0u;// usetint ? 1u : 0u;
            GeomVars.Vars.IsDecal = 0u;// DecalMode ? 1u : 0u;
            GeomVars.Vars.EnableWind = windflag;
            GeomVars.Vars.WindOverrideParams = geom.WindOverrideParams;
            GeomVars.Update(context);
            GeomVars.SetPSCBuffer(context, 0);
            GeomVars.SetVSCBuffer(context, 3);

            context.VertexShader.SetSampler(0, texsamplerc);
            context.PixelShader.SetSampler(0, texsampler);
            //context.PixelShader.SetSampler(1, texsamplerc);
            if (usediff)
            {
                texture.SetPSResource(context, 0);
            }


            if (geom.BoneTransforms != null)
            {
                SetBoneMatrices(context, geom.BoneTransforms);
                defaultBoneMatricesBound = false;
            }

        }

        public void SetBoneMatrices(DeviceContext context, Matrix3_s[] matrices)
        {
            BoneMatrices.Update(context, matrices);
            BoneMatrices.SetVSCBuffer(context, 7);
        }


        public override void UnbindResources(DeviceContext context)
        {
            context.VertexShader.SetConstantBuffer(0, null);
            context.VertexShader.SetConstantBuffer(1, null);
            context.VertexShader.SetConstantBuffer(2, null);
            context.VertexShader.SetConstantBuffer(3, null);
            context.PixelShader.SetConstantBuffer(0, null);
            context.VertexShader.SetSampler(0, null);
            context.PixelShader.SetSampler(0, null);
            context.PixelShader.SetShaderResource(0, null);
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
            GeomVars.Dispose();
            BoneMatrices.Dispose();


            shadowps.Dispose();
            shadowvs.Dispose();
            shadowvs_skin.Dispose();

            disposed = true;
        }
    }
}
