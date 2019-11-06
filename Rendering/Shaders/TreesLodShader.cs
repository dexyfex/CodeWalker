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

    public struct TreesLodShaderVSSceneVars
    {
        public Matrix ViewProj;
    }
    public struct TreesLodShaderVSEntityVars
    {
        public Vector4 CamRel;
        public Quaternion Orientation;
        public uint HasSkeleton;
        public uint HasTransforms;
        public uint Pad0;
        public uint Pad1;
        public Vector3 Scale;
        public uint Pad2;
    }
    public struct TreesLodShaderVSModelVars
    {
        public Matrix Transform;
    }
    public struct TreesLodShaderVSGeometryVars
    {
        public Vector4 AlphaTest;
        public Vector4 AlphaScale;
        public Vector4 UseTreeNormals;
        public Vector4 treeLod2Normal;
        public Vector4 treeLod2Params;
    }
    public struct TreesLodShaderPSSceneVars
    {
        public ShaderGlobalLightParams GlobalLights;
    }
    public struct TreesLodShaderPSEntityVars
    {
        public uint EnableTexture;
        public uint Pad1;
        public uint Pad2;
        public uint Pad3;
    }

    public class TreesLodShader : Shader, IDisposable
    {
        bool disposed = false;

        VertexShader basicvs;
        PixelShader basicps;
        GpuVarsBuffer<TreesLodShaderVSSceneVars> VSSceneVars;
        GpuVarsBuffer<TreesLodShaderVSEntityVars> VSEntityVars;
        GpuVarsBuffer<TreesLodShaderVSModelVars> VSModelVars;
        GpuVarsBuffer<TreesLodShaderVSGeometryVars> VSGeomVars;
        GpuVarsBuffer<TreesLodShaderPSSceneVars> PSSceneVars;
        GpuVarsBuffer<TreesLodShaderPSEntityVars> PSEntityVars;
        SamplerState texsampler;

        private Dictionary<VertexType, InputLayout> layouts = new Dictionary<VertexType, InputLayout>();


        public TreesLodShader(Device device)
        {
            byte[] vsbytes = File.ReadAllBytes("Shaders\\TreesLodVS.cso");
            byte[] psbytes = File.ReadAllBytes("Shaders\\TreesLodPS.cso");

            basicvs = new VertexShader(device, vsbytes);
            basicps = new PixelShader(device, psbytes);

            VSSceneVars = new GpuVarsBuffer<TreesLodShaderVSSceneVars>(device);
            VSEntityVars = new GpuVarsBuffer<TreesLodShaderVSEntityVars>(device);
            VSModelVars = new GpuVarsBuffer<TreesLodShaderVSModelVars>(device);
            VSGeomVars = new GpuVarsBuffer<TreesLodShaderVSGeometryVars>(device);
            PSSceneVars = new GpuVarsBuffer<TreesLodShaderPSSceneVars>(device);
            PSEntityVars = new GpuVarsBuffer<TreesLodShaderPSEntityVars>(device);

            //layouts.Add(VertexType.PNCCT, new InputLayout(device, vsbytes, VertexTypePNCCT.GetLayout()));
            layouts.Add(VertexType.PNCCTTTT, new InputLayout(device, vsbytes, VertexTypeGTAV.GetLayout(VertexDecl.Type1, VertexType.PNCCTTTT)));



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



        public override void SetShader(DeviceContext context)
        {
            context.VertexShader.Set(basicvs);
            context.PixelShader.Set(basicps);
        }

        public override bool SetInputLayout(DeviceContext context, VertexType type)
        {
            InputLayout l;
            if (layouts.TryGetValue(type, out l))
            {
                context.InputAssembler.InputLayout = l;
                return true;
            }
            return false;
        }

        public override void SetSceneVars(DeviceContext context, Camera camera, Shadowmap shadowmap, ShaderGlobalLights lights)
        {
            VSSceneVars.Vars.ViewProj = Matrix.Transpose(camera.ViewProjMatrix);
            VSSceneVars.Update(context);
            VSSceneVars.SetVSCBuffer(context, 0);

            PSSceneVars.Vars.GlobalLights = lights.Params;
            PSSceneVars.Update(context);
            PSSceneVars.SetPSCBuffer(context, 0);
        }

        public override void SetEntityVars(DeviceContext context, ref RenderableInst rend)
        {
            VSEntityVars.Vars.CamRel = new Vector4(rend.CamRel, 0.0f);
            VSEntityVars.Vars.Orientation = rend.Orientation;
            VSEntityVars.Vars.Scale = rend.Scale;
            VSEntityVars.Vars.HasSkeleton = rend.Renderable.HasSkeleton ? 1u : 0;
            VSEntityVars.Vars.HasTransforms = rend.Renderable.HasTransforms ? 1u : 0;
            VSEntityVars.Update(context);
            VSEntityVars.SetVSCBuffer(context, 1);
        }

        public override void SetModelVars(DeviceContext context, RenderableModel model)
        {
            if (!model.UseTransform) return;
            VSModelVars.Vars.Transform = Matrix.Transpose(model.Transform);
            VSModelVars.Update(context);
            VSModelVars.SetVSCBuffer(context, 2);
        }

        public override void SetGeomVars(DeviceContext context, RenderableGeometry geom)
        {
            RenderableTexture texture = null; // ((geom.Textures != null) && (geom.Textures.Length > 0)) ? geom.Textures[0] : null;

            //trees_lod2
            //PNCCTTTT: texcoord2 seems to be 0-1 for the billboard, vertex pos is billboard root.
            //param for billboard dir... (treeLod2Normal)

            //trees_lod
            //PNCCT:


            if (geom.VertexType != VertexType.PNCCTTTT)
            { }

            var shader = geom.DrawableGeom.Shader;
            if (shader.Name.Hash == 1874959840)
            {

                int nparams = 0;
                MetaName[] hashes = null;
                ShaderParameter[] sparams = null;

                if (shader.ParametersList != null)
                {
                    nparams = shader.ParametersList.Hashes.Length;
                    hashes = shader.ParametersList.Hashes;
                    sparams = shader.ParametersList.Parameters;
                }

                for (int i = 0; i < nparams; i++)
                {
                    var h = (ShaderParamNames)shader.ParametersList.Hashes[i];
                    switch (h)
                    {
                        case ShaderParamNames.AlphaTest: VSGeomVars.Vars.AlphaTest = (Vector4)sparams[i].Data; break;
                        case ShaderParamNames.AlphaScale: VSGeomVars.Vars.AlphaScale = (Vector4)sparams[i].Data; break;
                        case ShaderParamNames.UseTreeNormals: VSGeomVars.Vars.UseTreeNormals = (Vector4)sparams[i].Data; break;
                        case ShaderParamNames.treeLod2Normal: VSGeomVars.Vars.treeLod2Normal = (Vector4)sparams[i].Data; break;
                        case ShaderParamNames.treeLod2Params: VSGeomVars.Vars.treeLod2Params = (Vector4)sparams[i].Data; break;
                    }
                }


            }
            else
            {
                VSGeomVars.Vars.AlphaTest = Vector4.Zero;
                VSGeomVars.Vars.AlphaScale = Vector4.Zero;
                VSGeomVars.Vars.UseTreeNormals = Vector4.Zero;
                VSGeomVars.Vars.treeLod2Normal = Vector4.Zero;
                VSGeomVars.Vars.treeLod2Params = Vector4.Zero;
            }

            VSGeomVars.Update(context);
            VSGeomVars.SetVSCBuffer(context, 3);

            if ((geom.RenderableTextures != null) && (geom.RenderableTextures.Length > 0))
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

                //fallback try get first texture...
                int index = 0;
                while (((texture == null) || (texture.Texture2D == null)) && (index < geom.RenderableTextures.Length))
                {
                    texture = geom.RenderableTextures[index];
                    index++;
                }
            }


            bool usediff = ((texture != null) && (texture.Texture2D != null) && (texture.ShaderResourceView != null));
            PSEntityVars.Vars.EnableTexture = usediff ? 1u : 0u;
            PSEntityVars.Update(context);
            PSEntityVars.SetPSCBuffer(context, 1);

            if (usediff)
            {
                context.PixelShader.SetSampler(0, texsampler);
                //context.PixelShader.SetShaderResource(0, difftex.ShaderResourceView);
                texture.SetPSResource(context, 0);
            }
        }

        public override void UnbindResources(DeviceContext context)
        {
            context.VertexShader.SetConstantBuffer(0, null);
            context.PixelShader.SetConstantBuffer(0, null);
            context.VertexShader.SetConstantBuffer(1, null);
            context.VertexShader.SetConstantBuffer(2, null);
            context.VertexShader.SetConstantBuffer(3, null);
            context.PixelShader.SetConstantBuffer(1, null);
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
            VSGeomVars.Dispose();
            PSSceneVars.Dispose();
            PSEntityVars.Dispose();

            basicps.Dispose();
            basicvs.Dispose();

            disposed = true;
        }
    }
}
