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
using System.Diagnostics;

namespace CodeWalker.Rendering
{

    public struct TerrainShaderVSSceneVars
    {
        public Matrix ViewProj;
    }
    public struct TerrainShaderVSEntityVars
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
    public struct TerrainShaderVSModelVars
    {
        public Matrix Transform;
    }
    public struct TerrainShaderVSGeomVars
    {
        public uint EnableTint;
        public float TintYVal;
        public uint Pad4;
        public uint Pad5;
    }
    public struct TerrainShaderPSSceneVars
    {
        public ShaderGlobalLightParams GlobalLights;
        public uint EnableShadows;
        public uint RenderMode; //0=default, 1=normals, 2=tangents, 3=colours, 4=texcoords, 5=diffuse, 6=normalmap, 7=spec, 8=direct
        public uint RenderModeIndex; //colour/texcoord index
        public uint RenderSamplerCoord; //which texcoord to use in single texture mode
    }
    public struct TerrainShaderPSGeomVars
    {
        public uint EnableTexture0;
        public uint EnableTexture1;
        public uint EnableTexture2;
        public uint EnableTexture3;
        public uint EnableTexture4;
        public uint EnableTextureMask;
        public uint EnableNormalMap;
        public uint ShaderName;
        public uint EnableTint;
        public uint EnableVertexColour;
        public float bumpiness;
        public uint Pad102;
    }

    public class TerrainShader : Shader, IDisposable
    {
        bool disposed = false;

        VertexShader pncctvs;
        VertexShader pnccttvs;
        VertexShader pnccttxvs;
        VertexShader pncctttxvs;
        VertexShader pncctxvs;
        VertexShader pnctttxvs;
        VertexShader pncttxvs;
        PixelShader terrainps;
        PixelShader terrainpsdef;
        GpuVarsBuffer<TerrainShaderVSSceneVars> VSSceneVars;
        GpuVarsBuffer<TerrainShaderVSEntityVars> VSEntityVars;
        GpuVarsBuffer<TerrainShaderVSModelVars> VSModelVars;
        GpuVarsBuffer<TerrainShaderVSGeomVars> VSGeomVars;
        GpuVarsBuffer<TerrainShaderPSSceneVars> PSSceneVars;
        GpuVarsBuffer<TerrainShaderPSGeomVars> PSGeomVars;
        SamplerState texsampler;
        SamplerState texsampleranis;
        SamplerState texsamplertnt;
        public bool AnisotropicFilter = false;
        public WorldRenderMode RenderMode = WorldRenderMode.Default;
        public int RenderVertexColourIndex = 1;
        public int RenderTextureCoordIndex = 1;
        public int RenderTextureSamplerCoord = 1;
        public ShaderParamNames RenderTextureSampler = ShaderParamNames.DiffuseSampler;
        public bool Deferred = false;

        private Dictionary<VertexType, InputLayout> layouts = new Dictionary<VertexType, InputLayout>();


        public TerrainShader(Device device)
        {
            string folder = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "Shaders");
            byte[] vspncct = File.ReadAllBytes(Path.Combine(folder, "TerrainVS_PNCCT.cso"));
            byte[] vspncctt = File.ReadAllBytes(Path.Combine(folder, "TerrainVS_PNCCTT.cso"));
            byte[] vspnccttx = File.ReadAllBytes(Path.Combine(folder, "TerrainVS_PNCCTTX.cso"));
            byte[] vspncctttx = File.ReadAllBytes(Path.Combine(folder, "TerrainVS_PNCCTTTX.cso"));
            byte[] vspncctx = File.ReadAllBytes(Path.Combine(folder, "TerrainVS_PNCCTX.cso"));
            byte[] vspnctttx = File.ReadAllBytes(Path.Combine(folder, "TerrainVS_PNCTTTX.cso"));
            byte[] vspncttx = File.ReadAllBytes(Path.Combine(folder, "TerrainVS_PNCTTX.cso"));
            byte[] psbytes = File.ReadAllBytes(Path.Combine(folder, "TerrainPS.cso"));
            byte[] psdefbytes = File.ReadAllBytes(Path.Combine(folder, "TerrainPS_Deferred.cso"));

            pncctvs = new VertexShader(device, vspncct);
            pnccttvs = new VertexShader(device, vspncctt);
            pnccttxvs = new VertexShader(device, vspnccttx);
            pncctttxvs = new VertexShader(device, vspncctttx);
            pncctxvs = new VertexShader(device, vspncctx);
            pnctttxvs = new VertexShader(device, vspnctttx);
            pncttxvs = new VertexShader(device, vspncttx);
            terrainps = new PixelShader(device, psbytes);
            terrainpsdef = new PixelShader(device, psdefbytes);

            VSSceneVars = new GpuVarsBuffer<TerrainShaderVSSceneVars>(device);
            VSEntityVars = new GpuVarsBuffer<TerrainShaderVSEntityVars>(device);
            VSModelVars = new GpuVarsBuffer<TerrainShaderVSModelVars>(device);
            VSGeomVars = new GpuVarsBuffer<TerrainShaderVSGeomVars>(device);
            PSSceneVars = new GpuVarsBuffer<TerrainShaderPSSceneVars>(device);
            PSGeomVars = new GpuVarsBuffer<TerrainShaderPSGeomVars>(device);

            //supported layouts - requires Position, Normal, Colour, Texcoord
            layouts.Add(VertexType.PNCCT, new InputLayout(device, vspncct, VertexTypeGTAV.GetLayout(VertexType.PNCCT)));
            layouts.Add(VertexType.PNCCTT, new InputLayout(device, vspncctt, VertexTypeGTAV.GetLayout(VertexType.PNCCTT)));
            layouts.Add(VertexType.PNCTTX, new InputLayout(device, vspncttx, VertexTypeGTAV.GetLayout(VertexType.PNCTTX)));
            layouts.Add(VertexType.PNCTTTX_3, new InputLayout(device, vspnctttx, VertexTypeGTAV.GetLayout(VertexType.PNCTTTX_3)));
            layouts.Add(VertexType.PNCCTX, new InputLayout(device, vspncctx, VertexTypeGTAV.GetLayout(VertexType.PNCCTX)));
            layouts.Add(VertexType.PNCCTTX, new InputLayout(device, vspnccttx, VertexTypeGTAV.GetLayout(VertexType.PNCCTTX)));
            layouts.Add(VertexType.PNCCTTX_2, new InputLayout(device, vspnccttx, VertexTypeGTAV.GetLayout(VertexType.PNCCTTX_2)));
            layouts.Add(VertexType.PNCCTTTX, new InputLayout(device, vspncctttx, VertexTypeGTAV.GetLayout(VertexType.PNCCTTTX)));



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
            texsampleranis = new SamplerState(device, new SamplerStateDescription()
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                BorderColor = Color.Black,
                ComparisonFunction = Comparison.Always,
                Filter = Filter.Anisotropic,
                MaximumAnisotropy = 8,
                MaximumLod = float.MaxValue,
                MinimumLod = 0,
                MipLodBias = 0,
            });
            texsamplertnt = new SamplerState(device, new SamplerStateDescription()
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                BorderColor = Color.White,
                ComparisonFunction = Comparison.Always,
                Filter = Filter.MinMagMipPoint,
                MaximumAnisotropy = 1,
                MaximumLod = float.MaxValue,
                MinimumLod = 0,
                MipLodBias = 0,
            });

        }


        private void SetVertexShader(DeviceContext context, uint shaderhash)
        {
            //no longer used.


            //PNCCT     terrain_cb_w_4lyr_lod (terrain_cb_w_4lyr_lod.sps)
            //PNCCTT    terrain_cb_w_4lyr_2tex_blend_lod (terrain_cb_w_4lyr_2tex_blend_lod.sps)
            //PNCTTX    terrain_cb_w_4lyr_cm (terrain_cb_w_4lyr_cm.sps)
            //PNCTTX    terrain_cb_w_4lyr_cm_tnt (terrain_cb_w_4lyr_cm_tnt.sps)
            //PNCTTTX_3 terrain_cb_w_4lyr_cm_pxm (terrain_cb_w_4lyr_cm_pxm.sps)
            //PNCTTTX_3 terrain_cb_w_4lyr_cm_pxm_tnt (terrain_cb_w_4lyr_cm_pxm_tnt.sps)
            //PNCCTX    terrain_cb_w_4lyr (terrain_cb_w_4lyr.sps)
            //PNCCTX    terrain_cb_w_4lyr_spec (terrain_cb_w_4lyr_spec.sps)
            //PNCCTTX   terrain_cb_w_4lyr_2tex (terrain_cb_w_4lyr_2tex.sps)
            //PNCCTTX   terrain_cb_w_4lyr_2tex_blend (terrain_cb_w_4lyr_2tex_blend.sps)
            //PNCCTTX_2 terrain_cb_w_4lyr_pxm (terrain_cb_w_4lyr_pxm.sps)
            //PNCCTTX_2 terrain_cb_w_4lyr_pxm_spm (terrain_cb_w_4lyr_pxm_spm.sps)
            //PNCCTTX_2 terrain_cb_w_4lyr_spec_pxm (terrain_cb_w_4lyr_spec_pxm.sps)
            //PNCCTTTX  terrain_cb_w_4lyr_2tex_pxm (terrain_cb_w_4lyr_2tex_pxm.sps)
            //PNCCTTTX  terrain_cb_w_4lyr_2tex_blend_pxm (terrain_cb_w_4lyr_2tex_blend_pxm.sps)
            //PNCCTTTX  terrain_cb_w_4lyr_2tex_blend_pxm_spm (terrain_cb_w_4lyr_2tex_blend_pxm_spm.sps)


            VertexType vt = VertexType.Default;
            switch (shaderhash)
            {
                case 295525123: //terrain_cb_w_4lyr_cm (terrain_cb_w_4lyr_cm.sps)  vt: PNCTTX
                case 417637541: //terrain_cb_w_4lyr_cm_tnt (terrain_cb_w_4lyr_cm_tnt.sps)  vt: PNCTTX
                    vt = VertexType.PNCTTX;
                    break;
                case 3965214311: //terrain_cb_w_4lyr_cm_pxm_tnt (terrain_cb_w_4lyr_cm_pxm_tnt.sps)  vt: PNCTTTX_3
                case 4186046662: //terrain_cb_w_4lyr_cm_pxm (terrain_cb_w_4lyr_cm_pxm.sps)  vt: PNCTTTX_3
                    vt = VertexType.PNCTTTX; //cs6_08_struct08
                    break;
                case 3051127652: //terrain_cb_w_4lyr (terrain_cb_w_4lyr.sps)  vt: PNCCTX
                case 646532852: //terrain_cb_w_4lyr_spec (terrain_cb_w_4lyr_spec.sps)  vt: PNCCTX
                    vt = VertexType.PNCCTX; //hw1_07_grnd_c..
                    break;
                case 2535953532: //terrain_cb_w_4lyr_2tex_blend_lod (terrain_cb_w_4lyr_2tex_blend_lod.sps)  vt: PNCCTT
                    vt = VertexType.PNCCTT; //cs1_12_riverbed1_lod..
                    break;
                case 137526804: //terrain_cb_w_4lyr_lod (terrain_cb_w_4lyr_lod.sps)  vt: PNCCT
                    vt = VertexType.PNCCT; //brdgeplatform_01_lod..
                    break;
                case 2316006813: //terrain_cb_w_4lyr_2tex_blend (terrain_cb_w_4lyr_2tex_blend.sps)  vt: PNCCTTX
                case 3112820305: //terrain_cb_w_4lyr_2tex (terrain_cb_w_4lyr_2tex.sps)  vt: PNCCTTX
                case 2601000386: //terrain_cb_w_4lyr_spec_pxm (terrain_cb_w_4lyr_spec_pxm.sps)  vt: PNCCTTX_2
                case 4105814572: //terrain_cb_w_4lyr_pxm (terrain_cb_w_4lyr_pxm.sps)  vt: PNCCTTX_2
                case 3400824277: //terrain_cb_w_4lyr_pxm_spm (terrain_cb_w_4lyr_pxm_spm.sps)  vt: PNCCTTX_2
                    vt = VertexType.PNCCTTX; //ch2_04_land02b, ch2_06_terrain01a .. vb_35_beacha
                    break;
                case 653544224: //terrain_cb_w_4lyr_2tex_blend_pxm_spm (terrain_cb_w_4lyr_2tex_blend_pxm_spm.sps)  vt: PNCCTTTX
                case 2486206885: //terrain_cb_w_4lyr_2tex_blend_pxm (terrain_cb_w_4lyr_2tex_blend_pxm.sps)  vt: PNCCTTTX
                case 1888432890: //terrain_cb_w_4lyr_2tex_pxm (terrain_cb_w_4lyr_2tex_pxm.sps)  vt: PNCCTTTX
                    vt = VertexType.PNCCTTTX; //ch1_04b_vineland01..
                    break;
                default:
                    break;
            }
            SetVertexShader(context, vt);
        }
        private void SetVertexShader(DeviceContext context, VertexType type)
        {
            VertexShader vs = pnccttxvs;
            switch (type)
            {
                case VertexType.PNCCT: vs = pncctvs; break;
                case VertexType.PNCCTT: vs = pnccttvs; break;
                case VertexType.PNCTTX: vs = pncttxvs; break;
                case VertexType.PNCTTTX_3: vs = pnctttxvs; break;
                case VertexType.PNCCTX: vs = pncctxvs; break;
                case VertexType.PNCCTTX: vs = pnccttxvs; break;
                case VertexType.PNCCTTX_2: vs = pnccttxvs; break;
                case VertexType.PNCCTTTX: vs = pncctttxvs; break;
            }
            context.VertexShader.Set(vs);
        }

        public override void SetShader(DeviceContext context)
        {
            context.PixelShader.Set(Deferred ? terrainpsdef : terrainps);
        }

        public override bool SetInputLayout(DeviceContext context, VertexType type)
        {
            InputLayout l;
            if (layouts.TryGetValue(type, out l))
            {
                SetVertexShader(context, type); //need to use the correct VS.
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
                    switch (RenderTextureSampler)
                    {
                        case ShaderParamNames.DiffuseSampler:
                            rendermode = 5;
                            break;
                        case ShaderParamNames.BumpSampler:
                            rendermode = 6;
                            break;
                        case ShaderParamNames.SpecSampler:
                            rendermode = 7;
                            break;
                        default:
                            rendermode = 8;
                            break;
                    }
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
            RenderableTexture texture0 = null;
            RenderableTexture texture1 = null;
            RenderableTexture texture2 = null;
            RenderableTexture texture3 = null;
            RenderableTexture texture4 = null;
            RenderableTexture texturemask = null;
            RenderableTexture tintpal = null;
            RenderableTexture normals0 = null;
            RenderableTexture normals1 = null;
            RenderableTexture normals2 = null;
            RenderableTexture normals3 = null;
            RenderableTexture normals4 = null;
            float tntpalind = 0.0f;
            bool usevc = true;

            if ((geom.RenderableTextures != null) && (geom.RenderableTextures.Length > 0))
            {
                for (int i = 0; i < geom.RenderableTextures.Length; i++)
                {
                    var itex = geom.RenderableTextures[i];
                    if (geom.HDTextureEnable)
                    {
                        var hdtex = geom.RenderableTexturesHD[i];
                        if ((hdtex != null) && (hdtex.IsLoaded))
                        {
                            itex = hdtex;
                        }
                    }
                    var ihash = geom.TextureParamHashes[i];
                    switch (ihash)
                    {
                        case ShaderParamNames.DiffuseSampler:
                            texture0 = itex;
                            break;
                        case ShaderParamNames.TextureSampler_layer0:
                            texture1 = itex;
                            break;
                        case ShaderParamNames.TextureSampler_layer1:
                            texture2 = itex;
                            break;
                        case ShaderParamNames.TextureSampler_layer2:
                            texture3 = itex;
                            break;
                        case ShaderParamNames.TextureSampler_layer3:
                            texture4 = itex;
                            break;
                        case ShaderParamNames.BumpSampler:
                            normals0 = itex;
                            break;
                        case ShaderParamNames.BumpSampler_layer0:
                            normals1 = itex;
                            break;
                        case ShaderParamNames.BumpSampler_layer1:
                            normals2 = itex;
                            break;
                        case ShaderParamNames.BumpSampler_layer2:
                            normals3 = itex;
                            break;
                        case ShaderParamNames.BumpSampler_layer3:
                            normals4 = itex;
                            break;
                        case ShaderParamNames.lookupSampler:
                            texturemask = itex;
                            break;
                        case ShaderParamNames.TintPaletteSampler:
                            tintpal = itex;
                            if (tintpal.Key != null)
                            {
                                //this is slightly dodgy but vsentvarsdata should have the correct value in it...
                                tntpalind = (VSEntityVars.Vars.TintPaletteIndex + 0.5f) / tintpal.Key.Height;
                            }
                            break;
                    }
                }

                //if ((geom.DiffuseSampler >= 0) && (geom.DiffuseSampler < geom.Textures.Length))
                //{
                //    texture0 = geom.Textures[geom.DiffuseSampler];
                //}
                //if ((geom.TextureSampler_layer0 >= 0) && (geom.TextureSampler_layer0 < geom.Textures.Length))
                //{
                //    texture1 = geom.Textures[geom.TextureSampler_layer0];
                //}
                //if ((geom.TextureSampler_layer1 >= 0) && (geom.TextureSampler_layer1 < geom.Textures.Length))
                //{
                //    texture2 = geom.Textures[geom.TextureSampler_layer1];
                //}
                //if ((geom.TextureSampler_layer2 >= 0) && (geom.TextureSampler_layer2 < geom.Textures.Length))
                //{
                //    texture3 = geom.Textures[geom.TextureSampler_layer2];
                //}
                //if ((geom.TextureSampler_layer3 >= 0) && (geom.TextureSampler_layer3 < geom.Textures.Length))
                //{
                //    texture4 = geom.Textures[geom.TextureSampler_layer3];
                //}
                //if ((geom.BumpSampler >= 0) && (geom.BumpSampler < geom.Textures.Length))
                //{
                //    normals0 = geom.Textures[geom.BumpSampler];
                //}
                //if ((geom.BumpSampler_layer0 >= 0) && (geom.BumpSampler_layer0 < geom.Textures.Length))
                //{
                //    normals1 = geom.Textures[geom.BumpSampler_layer0];
                //}
                //if ((geom.BumpSampler_layer1 >= 0) && (geom.BumpSampler_layer1 < geom.Textures.Length))
                //{
                //    normals2 = geom.Textures[geom.BumpSampler_layer1];
                //}
                //if ((geom.BumpSampler_layer2 >= 0) && (geom.BumpSampler_layer2 < geom.Textures.Length))
                //{
                //    normals3 = geom.Textures[geom.BumpSampler_layer2];
                //}
                //if ((geom.BumpSampler_layer3 >= 0) && (geom.BumpSampler_layer3 < geom.Textures.Length))
                //{
                //    normals4 = geom.Textures[geom.BumpSampler_layer3];
                //}
                //if ((geom.lookupSampler >= 0) && (geom.lookupSampler < geom.Textures.Length))
                //{
                //    texturemask = geom.Textures[geom.lookupSampler];
                //}
                //if ((geom.TintPaletteSampler >= 0) && (geom.TintPaletteSampler < geom.Textures.Length))
                //{
                //    tintpal = geom.Textures[geom.TintPaletteSampler];
                //    if (tintpal.Texture != null)
                //    {
                //        //this is slightly dodgy but vsentvarsdata should have the correct value in it...
                //        tntpalind = (vsentvarsdata.TintPaletteIndex + 0.5f) / tintpal.Texture.Height;
                //    }
                //}



            }

            if (RenderMode == WorldRenderMode.SingleTexture)
            {
                usevc = false;
                switch (RenderTextureSampler)
                {
                    case ShaderParamNames.DiffuseSampler:
                    case ShaderParamNames.BumpSampler:
                    case ShaderParamNames.SpecSampler:
                        break;
                    default:
                        for (int i = 0; i < geom.RenderableTextures.Length; i++)
                        {
                            var itex = geom.RenderableTextures[i];
                            var ihash = geom.TextureParamHashes[i];
                            if (ihash == RenderTextureSampler)
                            {
                                texture0 = itex;
                                break;
                            }
                        }

                        //int directtexind = geom.GetTextureSamplerIndex(RenderTextureSampler);
                        //if ((directtexind >= 0) && (directtexind < geom.Textures.Length))
                        //{
                        //    texture0 = geom.Textures[directtexind];
                        //}
                        break;
                }
            }

            bool usediff0 = ((texture0 != null) && (texture0.ShaderResourceView != null));
            bool usediff1 = ((texture1 != null) && (texture1.ShaderResourceView != null));
            bool usediff2 = ((texture2 != null) && (texture2.ShaderResourceView != null));
            bool usediff3 = ((texture3 != null) && (texture3.ShaderResourceView != null));
            bool usediff4 = ((texture4 != null) && (texture4.ShaderResourceView != null));
            bool usemask = ((texturemask != null) && (texturemask.ShaderResourceView != null));
            bool usetint = ((tintpal != null) && (tintpal.ShaderResourceView != null));
            bool usenm = (((normals0 != null) && (normals0.ShaderResourceView != null)) || ((normals1 != null) && (normals1.ShaderResourceView != null)));


            float bumpiness = 1.0f;
            if (usenm)
            {
                bumpiness = geom.bumpiness;
            }


            PSGeomVars.Vars.EnableTexture0 = usediff0 ? 1u : 0u;
            PSGeomVars.Vars.EnableTexture1 = usediff1 ? 1u : 0u;
            PSGeomVars.Vars.EnableTexture2 = usediff2 ? 1u : 0u;
            PSGeomVars.Vars.EnableTexture3 = usediff3 ? 1u : 0u;
            PSGeomVars.Vars.EnableTexture4 = usediff4 ? 1u : 0u;
            PSGeomVars.Vars.EnableTextureMask = usemask ? 1u : 0u;
            PSGeomVars.Vars.EnableNormalMap = usenm ? 1u : 0u;
            PSGeomVars.Vars.ShaderName = geom.DrawableGeom.Shader.Name.Hash;
            PSGeomVars.Vars.EnableTint = usetint ? 1u : 0u;
            PSGeomVars.Vars.EnableVertexColour = usevc ? 1u : 0u;
            PSGeomVars.Vars.bumpiness = bumpiness;//
            PSGeomVars.Update(context);
            PSGeomVars.SetPSCBuffer(context, 2);

            VSGeomVars.Vars.EnableTint = usetint ? 1u : 0u;
            VSGeomVars.Vars.TintYVal = tntpalind;
            VSGeomVars.Update(context);
            VSGeomVars.SetVSCBuffer(context, 4);


            context.VertexShader.SetSampler(0, texsamplertnt);
            context.PixelShader.SetSampler(0, AnisotropicFilter ? texsampleranis : texsampler);

            if (usediff0) texture0.SetPSResource(context, 0);
            if (usediff1) texture1.SetPSResource(context, 2);
            if (usediff2) texture2.SetPSResource(context, 3);
            if (usediff3) texture3.SetPSResource(context, 4);
            if (usediff4) texture4.SetPSResource(context, 5);
            if (usemask) texturemask.SetPSResource(context, 6);
            if (usetint) tintpal.SetVSResource(context, 0);
            if (normals0 != null) normals0.SetPSResource(context, 7);
            if (normals1 != null) normals1.SetPSResource(context, 8);
            if (normals2 != null) normals2.SetPSResource(context, 9);
            if (normals3 != null) normals3.SetPSResource(context, 10);
            if (normals4 != null) normals4.SetPSResource(context, 11);

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
            context.VertexShader.SetShaderResource(0, null);
            context.PixelShader.SetShaderResource(0, null);
            context.PixelShader.SetShaderResource(2, null);
            context.PixelShader.SetShaderResource(3, null);
            context.PixelShader.SetShaderResource(4, null);
            context.PixelShader.SetShaderResource(5, null);
            context.PixelShader.SetShaderResource(6, null);
            context.PixelShader.SetShaderResource(7, null);
            context.PixelShader.SetShaderResource(8, null);
            context.PixelShader.SetShaderResource(9, null);
            context.PixelShader.SetShaderResource(10, null);
            context.PixelShader.SetShaderResource(11, null);
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
            if (texsampleranis != null)
            {
                texsampleranis.Dispose();
                texsampleranis = null;
            }
            if (texsamplertnt != null)
            {
                texsamplertnt.Dispose();
                texsamplertnt = null;
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

            terrainps.Dispose();
            terrainpsdef.Dispose();
            pncctvs.Dispose();
            pnccttvs.Dispose();
            pnccttxvs.Dispose();
            pncctttxvs.Dispose();
            pncctxvs.Dispose();
            pnctttxvs.Dispose();
            pncttxvs.Dispose();

            disposed = true;
        }
    }
}
