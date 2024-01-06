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
using CodeWalker.Rendering.Utils;

namespace CodeWalker.Rendering
{

    public struct BasicShaderVSSceneVars
    {
        public Matrix ViewProj;
        public Vector4 WindVector;
    }
    public struct BasicShaderVSEntityVars
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
    public struct BasicShaderVSModelVars
    {
        public Matrix Transform;
    }
    public struct BasicShaderVSGeomVars
    {
        public uint EnableTint;
        public float TintYVal;
        public uint IsDecal;
        public uint EnableWind;
        public Vector4 WindOverrideParams;
        public Vector4 globalAnimUV0;
        public Vector4 globalAnimUV1;
    }
    public struct BasicShaderPSSceneVars
    {
        public ShaderGlobalLightParams GlobalLights;
        public uint EnableShadows;
        public uint RenderMode;//0=default, 1=normals, 2=tangents, 3=colours, 4=texcoords, 5=diffuse, 6=normalmap, 7=spec, 8=direct
        public uint RenderModeIndex; //colour/texcoord index
        public uint RenderSamplerCoord; //which texcoord to use in single texture mode
    }
    public struct BasicShaderPSGeomVars
    {
        public uint EnableTexture;//1+=diffuse1, 2+=diffuse2
        public uint EnableTint;
        public uint EnableNormalMap;
        public uint EnableSpecMap;
        public uint EnableDetailMap;
        public uint IsDecal;
        public uint IsEmissive;
        public uint IsDistMap;
        public float bumpiness;
        public float AlphaScale;
        public float HardAlphaBlend;
        public float useTessellation;
        public Vector4 detailSettings;
        public Vector3 specMapIntMask;
        public float specularIntensityMult;
        public float specularFalloffMult;
        public float specularFresnel;
        public float wetnessMultiplier;
        public uint SpecOnly;
        public Vector4 TextureAlphaMask;
    }
    public struct BasicShaderInstGlobalMatrix
    {
        public Vector4 Row1;
        public Vector4 Row2;
        public Vector4 Row3;
    }
    public struct BasicShaderInstGlobals
    {
        public BasicShaderInstGlobalMatrix M0;
        public BasicShaderInstGlobalMatrix M1;
        public BasicShaderInstGlobalMatrix M2;
        public BasicShaderInstGlobalMatrix M3;
        public BasicShaderInstGlobalMatrix M4;
        public BasicShaderInstGlobalMatrix M5;
        public BasicShaderInstGlobalMatrix M6;
        public BasicShaderInstGlobalMatrix M7;
    }
    public struct BasicShaderInstLocals
    {
        public Vector3 vecBatchAabbMin;
        public float instPad0;
        public Vector3 vecBatchAabbDelta;
        public float instPad1;
        public Vector4 vecPlayerPos;
        public Vector2 _vecCollParams;
        public Vector2 instPad2;
        public Vector4 fadeAlphaDistUmTimer;
        public Vector4 uMovementParams;
        public Vector4 _fakedGrassNormal;
        public Vector3 gScaleRange;
        public float instPad3;
        public Vector4 gWindBendingGlobals;
        public Vector2 gWindBendScaleVar;
        public float gAlphaTest;
        public float gAlphaToCoverageScale;
        public Vector3 gLodFadeInstRange;
        public uint gUseComputeShaderOutputBuffer;
    }

    public class BasicShader : Shader, IDisposable
    {
        bool disposed = false;

        VertexShader basicvspnct;
        VertexShader basicvspnctt;
        VertexShader basicvspncttt;
        VertexShader basicvspncct;
        VertexShader basicvspncctt;
        VertexShader basicvspnccttt;
        VertexShader basicvspnctx;
        VertexShader basicvspncctx;
        VertexShader basicvspncttx;
        VertexShader basicvspnccttx;
        VertexShader basicvspnctttx;
        VertexShader basicvspncctttx;
        VertexShader basicvspbbnct;
        VertexShader basicvspbbnctx;
        VertexShader basicvspbbnctt;
        VertexShader basicvspbbncttt;
        VertexShader basicvspbbncct;
        VertexShader basicvspbbncctx;
        VertexShader basicvspbbnccttx;
        VertexShader basicvspbbncttx;
        VertexShader basicvsbox;
        VertexShader basicvssphere;
        VertexShader basicvscapsule;
        VertexShader basicvscylinder;
        PixelShader basicps;
        PixelShader basicpsdef;
        GpuVarsBuffer<BasicShaderVSSceneVars> VSSceneVars;
        GpuVarsBuffer<BasicShaderVSEntityVars> VSEntityVars;
        GpuVarsBuffer<BasicShaderVSModelVars> VSModelVars;
        GpuVarsBuffer<BasicShaderVSGeomVars> VSGeomVars;
        GpuVarsBuffer<BasicShaderPSSceneVars> PSSceneVars;
        GpuVarsBuffer<BasicShaderPSGeomVars> PSGeomVars;
        GpuVarsBuffer<BasicShaderInstGlobals> InstGlobalVars;
        GpuVarsBuffer<BasicShaderInstLocals> InstLocalVars;
        GpuABuffer<Matrix3_s> BoneMatrices;
        GpuABuffer<Vector4> ClothVertices;
        SamplerState texsampler;
        SamplerState texsampleranis;
        SamplerState texsamplertnt;
        SamplerState texsamplertntyft;
        UnitCube cube; //for collision box render
        UnitSphere sphere; //for collision sphere render
        UnitCapsule capsule; //for collision capsule render
        UnitCylinder cylinder; //for collision cylinder render

        public bool AnisotropicFilter = false;
        public bool DecalMode = false;
        public float AlphaScale = 1.0f;
        public Vector4 WindVector = Vector4.Zero;
        public WorldRenderMode RenderMode = WorldRenderMode.Default;
        public int RenderVertexColourIndex = 1;
        public int RenderTextureCoordIndex = 1;
        public int RenderTextureSamplerCoord = 1;
        public ShaderParamNames RenderTextureSampler = ShaderParamNames.DiffuseSampler;
        public bool SpecularEnable = true;
        public bool Deferred = false;

        Matrix3_s[] defaultBoneMatrices;
        bool defaultBoneMatricesBound = false;

        public ShaderManager _shaderManager;

        private Dictionary<VertexType, InputLayout> layouts = new Dictionary<VertexType, InputLayout>();

        public BasicShader(Device device, ShaderManager shaderManager)
        {
            _shaderManager = shaderManager;
            string folder = ShaderManager.GetShaderFolder();
            byte[] vspnctbytes = File.ReadAllBytes(Path.Combine(folder, "BasicVS_PNCT.cso"));
            byte[] vspncttbytes = File.ReadAllBytes(Path.Combine(folder, "BasicVS_PNCTT.cso"));
            byte[] vspnctttbytes = File.ReadAllBytes(Path.Combine(folder, "BasicVS_PNCTTT.cso"));
            byte[] vspncctbytes = File.ReadAllBytes(Path.Combine(folder, "BasicVS_PNCCT.cso"));
            byte[] vspnccttbytes = File.ReadAllBytes(Path.Combine(folder, "BasicVS_PNCCTT.cso"));
            byte[] vspncctttbytes = File.ReadAllBytes(Path.Combine(folder, "BasicVS_PNCCTTT.cso"));
            byte[] vspnctxbytes = File.ReadAllBytes(Path.Combine(folder, "BasicVS_PNCTX.cso"));
            byte[] vspncctxbytes = File.ReadAllBytes(Path.Combine(folder, "BasicVS_PNCCTX.cso"));
            byte[] vspncttxbytes = File.ReadAllBytes(Path.Combine(folder, "BasicVS_PNCTTX.cso"));
            byte[] vspnccttxbytes = File.ReadAllBytes(Path.Combine(folder, "BasicVS_PNCCTTX.cso"));
            byte[] vspnctttxbytes = File.ReadAllBytes(Path.Combine(folder, "BasicVS_PNCTTTX.cso"));
            byte[] vspncctttxbytes = File.ReadAllBytes(Path.Combine(folder, "BasicVS_PNCCTTTX.cso"));

            byte[] vspbbnctbytes = File.ReadAllBytes(Path.Combine(folder, "BasicVS_PBBNCT.cso"));
            byte[] vspbbnctxbytes = File.ReadAllBytes(Path.Combine(folder, "BasicVS_PBBNCTX.cso"));
            byte[] vspbbncttbytes = File.ReadAllBytes(Path.Combine(folder, "BasicVS_PBBNCTT.cso"));
            byte[] vspbbnctttbytes = File.ReadAllBytes(Path.Combine(folder, "BasicVS_PBBNCTTT.cso"));
            byte[] vspbbncctbytes = File.ReadAllBytes(Path.Combine(folder, "BasicVS_PBBNCCT.cso"));
            byte[] vspbbncctxbytes = File.ReadAllBytes(Path.Combine(folder, "BasicVS_PBBNCCTX.cso"));
            byte[] vspbbnccttxbytes = File.ReadAllBytes(Path.Combine(folder, "BasicVS_PBBNCCTTX.cso"));
            byte[] vspbbncttxbytes = File.ReadAllBytes(Path.Combine(folder, "BasicVS_PBBNCTTX.cso"));

            byte[] vsboxbytes = File.ReadAllBytes(Path.Combine(folder, "BasicVS_Box.cso"));
            byte[] vsspherebytes = File.ReadAllBytes(Path.Combine(folder, "BasicVS_Sphere.cso"));
            byte[] vscapsulebytes = File.ReadAllBytes(Path.Combine(folder, "BasicVS_Capsule.cso"));
            byte[] vscylinderbytes = File.ReadAllBytes(Path.Combine(folder, "BasicVS_Cylinder.cso"));
            byte[] psbytes = File.ReadAllBytes(Path.Combine(folder, "BasicPS.cso"));
            byte[] psdefbytes = File.ReadAllBytes(Path.Combine(folder, "BasicPS_Deferred.cso"));

            basicvspnct = device.CreateVertexShader("BasicVS_PNCT.cso");
            basicvspnctt = device.CreateVertexShader("BasicVS_PNCTT.cso");
            basicvspncttt = device.CreateVertexShader("BasicVS_PNCTTT.cso");
            basicvspncct = device.CreateVertexShader("BasicVS_PNCCT.cso");
            basicvspncctt = device.CreateVertexShader("BasicVS_PNCCTT.cso");
            basicvspnccttt = device.CreateVertexShader("BasicVS_PNCCTTT.cso");
            basicvspnctx = device.CreateVertexShader("BasicVS_PNCTX.cso");
            basicvspncctx = device.CreateVertexShader("BasicVS_PNCCTX.cso");
            basicvspncttx = device.CreateVertexShader("BasicVS_PNCTTX.cso");
            basicvspnccttx = device.CreateVertexShader("BasicVS_PNCCTTX.cso");
            basicvspnctttx = device.CreateVertexShader("BasicVS_PNCTTTX.cso");
            basicvspncctttx = device.CreateVertexShader("BasicVS_PNCCTTTX.cso");
            basicvspbbnct = device.CreateVertexShader("BasicVS_PBBNCT.cso");
            basicvspbbnctx = device.CreateVertexShader("BasicVS_PBBNCTX.cso");
            basicvspbbnctt = device.CreateVertexShader("BasicVS_PBBNCTT.cso");
            basicvspbbncttt = device.CreateVertexShader("BasicVS_PBBNCTTT.cso");
            basicvspbbncct = device.CreateVertexShader("BasicVS_PBBNCCT.cso");
            basicvspbbncctx = device.CreateVertexShader("BasicVS_PBBNCCTX.cso");
            basicvspbbnccttx = device.CreateVertexShader("BasicVS_PBBNCCTTX.cso");
            basicvspbbncttx = device.CreateVertexShader("BasicVS_PBBNCTTX.cso");

            basicvsbox = device.CreateVertexShader("BasicVS_Box.cso");
            basicvssphere = device.CreateVertexShader("BasicVS_Sphere.cso");
            basicvscapsule = device.CreateVertexShader("BasicVS_Capsule.cso");
            basicvscylinder = device.CreateVertexShader("BasicVS_Cylinder.cso");
            basicps = device.CreatePixelShader("BasicPS.cso");
            basicpsdef = device.CreatePixelShader("BasicPS_Deferred.cso");

            VSSceneVars = new GpuVarsBuffer<BasicShaderVSSceneVars>(device);
            VSEntityVars = new GpuVarsBuffer<BasicShaderVSEntityVars>(device);
            VSModelVars = new GpuVarsBuffer<BasicShaderVSModelVars>(device);
            VSGeomVars = new GpuVarsBuffer<BasicShaderVSGeomVars>(device);
            PSSceneVars = new GpuVarsBuffer<BasicShaderPSSceneVars>(device);
            PSGeomVars = new GpuVarsBuffer<BasicShaderPSGeomVars>(device);
            InstGlobalVars = new GpuVarsBuffer<BasicShaderInstGlobals>(device);
            InstLocalVars = new GpuVarsBuffer<BasicShaderInstLocals>(device);
            BoneMatrices = new GpuABuffer<Matrix3_s>(device, 255);
            ClothVertices = new GpuABuffer<Vector4>(device, 254);

            InitInstGlobalVars();


            //supported layouts - requires Position, Normal, Colour, Texcoord
            layouts.Add(VertexType.Default, new InputLayout(device, vspnctbytes, VertexTypeGTAV.GetLayout(VertexType.Default)));
            layouts.Add(VertexType.PNCH2, new InputLayout(device, vspnctbytes, VertexTypeGTAV.GetLayout(VertexType.PNCH2, VertexDeclarationTypes.GTAV3)));//TODO?
            layouts.Add(VertexType.PNCTT, new InputLayout(device, vspncttbytes, VertexTypeGTAV.GetLayout(VertexType.PNCTT)));
            layouts.Add(VertexType.PNCTTT, new InputLayout(device, vspnctttbytes, VertexTypeGTAV.GetLayout(VertexType.PNCTTT)));
            layouts.Add(VertexType.PNCCT, new InputLayout(device, vspncctbytes, VertexTypeGTAV.GetLayout(VertexType.PNCCT)));
            layouts.Add(VertexType.PNCCTT, new InputLayout(device, vspnccttbytes, VertexTypeGTAV.GetLayout(VertexType.PNCCTT)));
            layouts.Add(VertexType.PNCCTTTT, new InputLayout(device, vspncctttbytes, VertexTypeGTAV.GetLayout(VertexType.PNCCTTTT)));//TODO..?



            //normalmap layouts - requires Position, Normal, Colour, Texcoord, Tangent (X)
            layouts.Add(VertexType.DefaultEx, new InputLayout(device, vspnctxbytes, VertexTypeGTAV.GetLayout(VertexType.DefaultEx)));
            layouts.Add(VertexType.PCCH2H4, new InputLayout(device, vspnctxbytes, VertexTypeGTAV.GetLayout(VertexType.PCCH2H4, VertexDeclarationTypes.GTAV2)));
            layouts.Add(VertexType.PNCCTX, new InputLayout(device, vspncctxbytes, VertexTypeGTAV.GetLayout(VertexType.PNCCTX)));
            layouts.Add(VertexType.PNCTTX, new InputLayout(device, vspncttxbytes, VertexTypeGTAV.GetLayout(VertexType.PNCTTX)));
            layouts.Add(VertexType.PNCCTTX, new InputLayout(device, vspnccttxbytes, VertexTypeGTAV.GetLayout(VertexType.PNCCTTX)));
            layouts.Add(VertexType.PNCCTTX_2, new InputLayout(device, vspnccttxbytes, VertexTypeGTAV.GetLayout(VertexType.PNCCTTX_2)));
            layouts.Add(VertexType.PNCTTTX, new InputLayout(device, vspnctttxbytes, VertexTypeGTAV.GetLayout(VertexType.PNCTTTX)));
            layouts.Add(VertexType.PNCTTTX_2, new InputLayout(device, vspnctttxbytes, VertexTypeGTAV.GetLayout(VertexType.PNCTTTX_2)));
            layouts.Add(VertexType.PNCTTTX_3, new InputLayout(device, vspnctttxbytes, VertexTypeGTAV.GetLayout(VertexType.PNCTTTX_3)));
            layouts.Add(VertexType.PNCTTTTX, new InputLayout(device, vspnctttxbytes, VertexTypeGTAV.GetLayout(VertexType.PNCTTTTX)));//TODO
            layouts.Add(VertexType.PNCCTTTX, new InputLayout(device, vspncctttxbytes, VertexTypeGTAV.GetLayout(VertexType.PNCCTTTX)));



            //skinned layouts
            layouts.Add(VertexType.PBBNCT, new InputLayout(device, vspbbnctbytes, VertexTypeGTAV.GetLayout(VertexType.PBBNCT)));
            layouts.Add(VertexType.PBBNCTX, new InputLayout(device, vspbbnctxbytes, VertexTypeGTAV.GetLayout(VertexType.PBBNCTX)));
            layouts.Add(VertexType.PBBNCTT, new InputLayout(device, vspbbncttbytes, VertexTypeGTAV.GetLayout(VertexType.PBBNCTT)));
            layouts.Add(VertexType.PBBNCTTT, new InputLayout(device, vspbbnctttbytes, VertexTypeGTAV.GetLayout(VertexType.PBBNCTTT)));
            layouts.Add(VertexType.PBBNCCT, new InputLayout(device, vspbbncctbytes, VertexTypeGTAV.GetLayout(VertexType.PBBNCCT)));
            layouts.Add(VertexType.PBBNCCTT, new InputLayout(device, vspbbncctbytes, VertexTypeGTAV.GetLayout(VertexType.PBBNCCTT)));//TODO
            layouts.Add(VertexType.PBBNCCTX, new InputLayout(device, vspbbncctxbytes, VertexTypeGTAV.GetLayout(VertexType.PBBNCCTX)));
            layouts.Add(VertexType.PBBNCTTX, new InputLayout(device, vspbbncttxbytes, VertexTypeGTAV.GetLayout(VertexType.PBBNCTTX)));
            layouts.Add(VertexType.PBBNCTTTX, new InputLayout(device, vspbbncttxbytes, VertexTypeGTAV.GetLayout(VertexType.PBBNCTTTX)));//TODO
            layouts.Add(VertexType.PBBNCCTTX, new InputLayout(device, vspbbnccttxbytes, VertexTypeGTAV.GetLayout(VertexType.PBBNCCTTX)));
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
                AddressU = TextureAddressMode.Clamp,
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
            texsamplertntyft = new SamplerState(device, new SamplerStateDescription()
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


            cube = new UnitCube(device, vsboxbytes, false, false, true);
            sphere = new UnitSphere(device, vsspherebytes, 4);
            capsule = new UnitCapsule(device, vscapsulebytes, 4);
            cylinder = new UnitCylinder(device, vscylinderbytes, 8);

            defaultBoneMatrices = new Matrix3_s[255];
            for (int i = 0; i < 255; i++)
            {
                defaultBoneMatrices[i] = new Matrix3_s
                {
                    Row1 = Vector4.UnitX,
                    Row2 = Vector4.UnitY,
                    Row3 = Vector4.UnitZ
                };
            }
        }

        private void InitInstGlobalVars()
        {
            var m0 = Matrix3x3.RotationZ(0.00f * (float)Math.PI);
            var m1 = Matrix3x3.RotationZ(0.25f * (float)Math.PI);
            var m2 = Matrix3x3.RotationZ(0.50f * (float)Math.PI);
            var m3 = Matrix3x3.RotationZ(0.75f * (float)Math.PI);
            var m4 = Matrix3x3.RotationZ(1.00f * (float)Math.PI);
            var m5 = Matrix3x3.RotationZ(1.25f * (float)Math.PI);
            var m6 = Matrix3x3.RotationZ(1.50f * (float)Math.PI);
            var m7 = Matrix3x3.RotationZ(1.75f * (float)Math.PI);

            InstGlobalVars.Vars.M0.Row1 = new Vector4(m0.Row1, 1);
            InstGlobalVars.Vars.M0.Row2 = new Vector4(m0.Row2, 1);
            InstGlobalVars.Vars.M0.Row3 = new Vector4(m0.Row3, 1);
            InstGlobalVars.Vars.M1.Row1 = new Vector4(m1.Row1, 1);
            InstGlobalVars.Vars.M1.Row2 = new Vector4(m1.Row2, 1);
            InstGlobalVars.Vars.M1.Row3 = new Vector4(m1.Row3, 1);
            InstGlobalVars.Vars.M2.Row1 = new Vector4(m2.Row1, 1);
            InstGlobalVars.Vars.M2.Row2 = new Vector4(m2.Row2, 1);
            InstGlobalVars.Vars.M2.Row3 = new Vector4(m2.Row3, 1);
            InstGlobalVars.Vars.M3.Row1 = new Vector4(m3.Row1, 1);
            InstGlobalVars.Vars.M3.Row2 = new Vector4(m3.Row2, 1);
            InstGlobalVars.Vars.M3.Row3 = new Vector4(m3.Row3, 1);
            InstGlobalVars.Vars.M4.Row1 = new Vector4(m4.Row1, 1);
            InstGlobalVars.Vars.M4.Row2 = new Vector4(m4.Row2, 1);
            InstGlobalVars.Vars.M4.Row3 = new Vector4(m4.Row3, 1);
            InstGlobalVars.Vars.M5.Row1 = new Vector4(m5.Row1, 1);
            InstGlobalVars.Vars.M5.Row2 = new Vector4(m5.Row2, 1);
            InstGlobalVars.Vars.M5.Row3 = new Vector4(m5.Row3, 1);
            InstGlobalVars.Vars.M6.Row1 = new Vector4(m6.Row1, 1);
            InstGlobalVars.Vars.M6.Row2 = new Vector4(m6.Row2, 1);
            InstGlobalVars.Vars.M6.Row3 = new Vector4(m6.Row3, 1);
            InstGlobalVars.Vars.M7.Row1 = new Vector4(m7.Row1, 1);
            InstGlobalVars.Vars.M7.Row2 = new Vector4(m7.Row2, 1);
            InstGlobalVars.Vars.M7.Row3 = new Vector4(m7.Row3, 1);

        }


        private void SetVertexShader(DeviceContext context, VertexType type)
        {
            VertexShader vs = basicvspnct;
            switch (type)
            {
                case VertexType.Default:
                case VertexType.PNCH2:
                    vs = basicvspnct;
                    break;
                case VertexType.PNCTT:
                    vs = basicvspnctt;
                    break;
                case VertexType.PNCTTT:
                    vs = basicvspncttt;
                    break;
                case VertexType.PNCCT:
                    vs = basicvspncct;
                    break;
                case VertexType.PNCCTT://not used?
                    vs = basicvspncctt;
                    break;
                case VertexType.PNCCTTTT://not used?
                    vs = basicvspnccttt;
                    break;
                case VertexType.DefaultEx:
                case VertexType.PCCH2H4:
                    vs = basicvspnctx;
                    break;
                case VertexType.PNCCTX:
                    vs = basicvspncctx;
                    break;
                case VertexType.PNCTTX:
                    vs = basicvspncttx;
                    break;
                case VertexType.PNCCTTX://not used?
                case VertexType.PNCCTTX_2://not used?
                    vs = basicvspnccttx;
                    break;

                case VertexType.PNCTTTX:
                case VertexType.PNCTTTX_2:
                case VertexType.PNCTTTX_3:
                case VertexType.PNCTTTTX: //not using last texcoords!
                    vs = basicvspnctttx;
                    break;

                case VertexType.PNCCTTTX://not used?
                    vs = basicvspncctttx;
                    break;

                case VertexType.PBBNCT:
                    vs = basicvspbbnct;
                    break;
                case VertexType.PBBNCTT:
                    vs = basicvspbbnctt;
                    break;
                case VertexType.PBBNCTTT:
                    vs = basicvspbbncttt;
                    break;
                case VertexType.PBBNCCT:
                    vs = basicvspbbncct;
                    break;
                case VertexType.PBBNCCTT://todo
                    vs = basicvspbbncct;
                    break;
                case VertexType.PBBNCTX:
                    vs = basicvspbbnctx;
                    break;
                case VertexType.PBBNCCTX:
                    vs = basicvspbbncctx;
                    break;
                case VertexType.PBBNCTTX:
                    vs = basicvspbbncttx;
                    break;
                case VertexType.PBBNCCTTX:
                    vs = basicvspbbnccttx;
                    break;
                case VertexType.PBBNCTTTX:
                    vs = basicvspbbncttx;//TODO
                    break;

                default:
                    break;

            }

            if (context.VertexShader.Get() != vs)
            {
                context.VertexShader.Set(vs);
            }
        }


        public override void SetShader(DeviceContext context)
        {
            var shader = Deferred ? basicpsdef : basicps;
            if (context.PixelShader.Get() != shader)
            {
                context.PixelShader.Set(Deferred ? basicpsdef : basicps);
            }
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

        public override void SetSceneVars(DeviceContext context, Camera camera, Shadowmap? shadowmap, ShaderGlobalLights lights)
        {
            uint rendermode = 0;
            uint rendermodeind = 1;

            SpecularEnable = lights.SpecularEnabled;

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
            VSSceneVars.Vars.WindVector = WindVector;
            VSSceneVars.Update(context);
            VSSceneVars.SetVSCBuffer(context, 0);

            PSSceneVars.Vars.GlobalLights = lights.Params;
            PSSceneVars.Vars.EnableShadows = (shadowmap != null) ? 1u : 0u;
            PSSceneVars.Vars.RenderMode = rendermode;
            PSSceneVars.Vars.RenderModeIndex = rendermodeind;
            PSSceneVars.Vars.RenderSamplerCoord = (uint)RenderTextureSamplerCoord;
            PSSceneVars.Update(context);
            PSSceneVars.SetPSCBuffer(context, 0);

            shadowmap?.SetFinalRenderResources(context);

            if (!InstGlobalVars.Flag) //on the first frame, update the instance globals
            {
                InstGlobalVars.Update(context);
                InstGlobalVars.Flag = true;
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
            if ((model.Owner.Skeleton?.BoneTransforms != null) && (model.Owner.Skeleton.BoneTransforms.Length > 0))
            {
                SetBoneMatrices(context, model.Owner.Skeleton.BoneTransforms);
                defaultBoneMatricesBound = false;
            }
            else if (!defaultBoneMatricesBound)
            {
                SetBoneMatrices(context, defaultBoneMatrices);
                defaultBoneMatricesBound = true;
            }
            if (model.Owner.Cloth?.Vertices != null)
            {
                SetClothVertices(context, model.Owner.Cloth.Vertices);
            }

            if (!model.UseTransform) return;
            VSModelVars.Vars.Transform = Matrix.Transpose(model.Transform);
            VSModelVars.Update(context);
            VSModelVars.SetVSCBuffer(context, 3);
        }

        public override void SetGeomVars(DeviceContext context, RenderableGeometry geom)
        {
            RenderableTexture texture = null;
            RenderableTexture texture2 = null;
            RenderableTexture tintpal = null;
            RenderableTexture bumptex = null;
            RenderableTexture spectex = null;
            RenderableTexture detltex = null;
            bool isdistmap = false;

            float tntpalind = 0.0f;
            if ((geom.RenderableTextures != null) && (geom.RenderableTextures.Length > 0))
            {
                if (RenderMode == WorldRenderMode.Default)
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
                        if (itex == null) continue;
                        if (itex.Key?.NameHash == 1678728908 /*"blank"*/) continue;
                        switch (ihash)
                        {
                            case ShaderParamNames.DiffuseSampler:
                            case ShaderParamNames.PlateBgSampler:
                                texture = itex;
                                break;
                            case ShaderParamNames.BumpSampler:
                            case ShaderParamNames.PlateBgBumpSampler:
                                bumptex = itex;
                                break;
                            case ShaderParamNames.SpecSampler:
                                spectex = itex;
                                break;
                            case ShaderParamNames.DetailSampler:
                                detltex = itex;
                                break;
                            case ShaderParamNames.TintPaletteSampler:
                            case ShaderParamNames.TextureSamplerDiffPal:
                                tintpal = itex;
                                if (tintpal.Key != null)
                                {
                                    //this is slightly dodgy but VSEntityVars should have the correct value in it...
                                    tntpalind = (VSEntityVars.Vars.TintPaletteIndex + 0.5f) / tintpal.Key.Height;
                                }
                                break;
                            case ShaderParamNames.distanceMapSampler:
                                texture = itex;
                                isdistmap = true;
                                break;
                            case ShaderParamNames.DiffuseSampler2:
                            case ShaderParamNames.DiffuseExtraSampler:
                                texture2 = itex;
                                break;
                            case ShaderParamNames.heightSampler:
                            case ShaderParamNames.EnvironmentSampler:
                            //case MetaName.SnowSampler0:
                            //case MetaName.SnowSampler1:
                            //case MetaName.DiffuseSampler3:
                            //case MetaName.DirtSampler:
                            //case MetaName.DirtBumpSampler:
                                break;
                            case ShaderParamNames.FlowSampler:
                            case ShaderParamNames.FogSampler:
                            case ShaderParamNames.FoamSampler:
                                if (texture == null) texture = itex;
                                break;
                            default:
                                if (texture == null) texture = itex;
                                break;
                        }
                    }
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


            bool usediff = ((texture != null) && (texture.ShaderResourceView != null));
            bool usediff2 = ((texture2 != null) && (texture2.ShaderResourceView != null));
            bool usebump = ((bumptex != null) && (bumptex.ShaderResourceView != null));
            bool usespec = ((spectex != null) && (spectex.ShaderResourceView != null));
            bool usedetl = ((detltex != null) && (detltex.ShaderResourceView != null));
            bool usetint = ((tintpal != null) && (tintpal.ShaderResourceView != null));

            uint tintflag = 0;
            if (usetint) tintflag = 1;

            Vector4 textureAlphaMask = Vector4.Zero;
            uint decalflag = DecalMode ? 1u : 0u;
            uint windflag = geom.EnableWind ? 1u : 0u;
            uint emflag = geom.IsEmissive ? 1u : 0u;
            uint pstintflag = tintflag;
            var shaderName = geom.DrawableGeom.Shader.Name;
            var shaderFile = geom.DrawableGeom.Shader.FileName;
            switch (shaderFile.Hash)
            {
                case 2245870123: //trees_normal_diffspec_tnt.sps
                case 3334613197: //trees_tnt.sps
                case 1229591973://{trees_normal_spec_tnt.sps}
                    if (usetint) tintflag = 2; //use 2nd vertex colour channel for tint...
                    break;
                case 3267631682: //weapon_normal_spec_detail_tnt.sps
                case 14185869:   //weapon_normal_spec_tnt.sps
                    break; //regular tinting?
                case 231364109: //weapon_normal_spec_cutout_palette.sps
                case 3294641629://weapon_normal_spec_detail_palette.sps
                case 731050667: //weapon_normal_spec_palette.sps
                    if (usetint) { tintflag = 0; pstintflag = 2; } //use diffuse sampler alpha for tint lookup!
                    break;
                case 341123999://{decal_normal_only.sps}
                case 2706821972://{mirror_decal.sps}
                case 2457676400://{reflect_decal.sps}
                    decalflag = 3;
                    break;
                case 3880384844://{decal_spec_only.sps}
                case 2842248626://{spec_decal.sps}
                    decalflag = 4;
                    break;
                case 600733812://{decal_amb_only.sps}
                    //if (RenderMode == WorldRenderMode.Default) usediff = false;
                    break;
                case 2655725442://{decal_dirt.sps}
                    textureAlphaMask = geom.DirtDecalMask;
                    decalflag = 2;
                    break;
            }

            if (VSEntityVars.Vars.IsInstanced>0)
            {
                pstintflag = 1;
                switch (shaderFile.Hash)
                {
                    case 916743331: //{grass_batch.sps}
                        windflag = 1;
                        break;
                    case 3833671083://{normal_spec_batch.sps}
                        windflag = 0;
                        break;
                    default:
                        break;
                }
            }


            PSGeomVars.Vars.EnableTexture = (usediff ? 1u : 0u) + (usediff2 ? 2u : 0u);
            PSGeomVars.Vars.EnableTint = pstintflag;
            PSGeomVars.Vars.EnableNormalMap = usebump ? 1u : 0u;
            PSGeomVars.Vars.EnableSpecMap = usespec ? 1u : 0u;
            PSGeomVars.Vars.EnableDetailMap = usedetl ? 1u : 0u;
            PSGeomVars.Vars.IsDecal = decalflag;
            PSGeomVars.Vars.IsEmissive = emflag;
            PSGeomVars.Vars.IsDistMap = isdistmap ? 1u : 0u;
            PSGeomVars.Vars.bumpiness = geom.bumpiness;
            PSGeomVars.Vars.AlphaScale = isdistmap ? 1.0f : AlphaScale;
            PSGeomVars.Vars.HardAlphaBlend = 0.0f; //todo: cutouts flag!
            PSGeomVars.Vars.useTessellation = 0.0f;
            PSGeomVars.Vars.detailSettings = geom.detailSettings;
            PSGeomVars.Vars.specMapIntMask = geom.specMapIntMask;
            PSGeomVars.Vars.specularIntensityMult = SpecularEnable ? geom.specularIntensityMult : 0.0f;
            PSGeomVars.Vars.specularFalloffMult = geom.specularFalloffMult;
            PSGeomVars.Vars.specularFresnel = geom.specularFresnel;
            PSGeomVars.Vars.wetnessMultiplier = geom.wetnessMultiplier;
            PSGeomVars.Vars.SpecOnly = geom.SpecOnly ? 1u : 0u;
            PSGeomVars.Vars.TextureAlphaMask = textureAlphaMask;
            PSGeomVars.Update(context);
            PSGeomVars.SetPSCBuffer(context, 2);

            VSGeomVars.Vars.EnableTint = tintflag;
            VSGeomVars.Vars.TintYVal = tntpalind;
            VSGeomVars.Vars.IsDecal = DecalMode ? 1u : 0u;
            VSGeomVars.Vars.EnableWind = windflag;
            VSGeomVars.Vars.WindOverrideParams = geom.WindOverrideParams;
            VSGeomVars.Vars.globalAnimUV0 = geom.globalAnimUV0;
            VSGeomVars.Vars.globalAnimUV1 = geom.globalAnimUV1;
            VSGeomVars.Update(context);
            VSGeomVars.SetVSCBuffer(context, 4);

            context.VertexShader.SetSampler(0, geom.IsFragment ? texsamplertntyft : texsamplertnt);
            context.PixelShader.SetSampler(0, AnisotropicFilter ? texsampleranis : texsampler);
            if (usediff)
            {
                texture.SetPSResource(context, 0);
            }
            if (usebump)
            {
                bumptex.SetPSResource(context, 2);
            }
            if (usespec)
            {
                spectex.SetPSResource(context, 3);
            }
            if (usedetl)
            {
                detltex.SetPSResource(context, 4);
            }
            if (usediff2)
            {
                texture2.SetPSResource(context, 5);
            }
            if (usetint)
            {
                tintpal.SetVSResource(context, 0);
            }
            if (pstintflag == 2)
            {
                tintpal.SetPSResource(context, 6);
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

        public void SetClothVertices(DeviceContext context, Vector4[] vertices)
        {
            ClothVertices.Update(context, vertices);
            ClothVertices.SetVSCBuffer(context, 8);
        }


        public void SetInstanceVars(DeviceContext context, RenderableInstanceBatch batch)
        {
            var gb = batch.Key;

            // sanity check
            if (batch.GrassInstanceBuffer == null)
                return;

            VSEntityVars.Vars.CamRel = new Vector4(batch.CamRel, 0.0f);
            VSEntityVars.Vars.Orientation = Quaternion.Identity;
            VSEntityVars.Vars.Scale = Vector3.One;
            VSEntityVars.Vars.HasSkeleton = 0;
            VSEntityVars.Vars.HasTransforms = 0;
            VSEntityVars.Vars.TintPaletteIndex = 0;
            VSEntityVars.Vars.IsInstanced = 1;
            VSEntityVars.Update(context);
            VSEntityVars.SetVSCBuffer(context, 2);

            InstGlobalVars.SetVSCBuffer(context, 5);

            InstLocalVars.Vars.vecBatchAabbMin = batch.AABBMin;
            InstLocalVars.Vars.vecBatchAabbDelta = batch.AABBMax - batch.AABBMin;
            Vector3.Subtract(ref batch.Position, ref batch.CamRel, out var result);
            InstLocalVars.Vars.vecPlayerPos = new Vector4(result, 1.0f);
            InstLocalVars.Vars._vecCollParams = new Vector2(2.0f, -3.0f);//range, offset
            InstLocalVars.Vars.fadeAlphaDistUmTimer = new Vector4(0.0f);
            InstLocalVars.Vars.uMovementParams = new Vector4(0.0f);
            var camRel = -batch.CamRel;
            camRel.Normalize();
            InstLocalVars.Vars._fakedGrassNormal = new Vector4(camRel, 0.0f);
            InstLocalVars.Vars.gScaleRange = gb.Batch.ScaleRange;
            InstLocalVars.Vars.gWindBendingGlobals = new Vector4(WindVector.X, WindVector.Y, 1.0f, 1.0f);
            InstLocalVars.Vars.gWindBendScaleVar = new Vector2(WindVector.Z, WindVector.W);
            InstLocalVars.Vars.gAlphaTest = 0.0f;
            InstLocalVars.Vars.gAlphaToCoverageScale = 1.0f;
            InstLocalVars.Vars.gLodFadeInstRange = new Vector3(gb.Batch.LodInstFadeRange, gb.Batch.LodFadeStartDist, gb.Batch.lodDist);
            InstLocalVars.Vars.gUseComputeShaderOutputBuffer = 0;
            InstLocalVars.Update(context);
            InstLocalVars.SetVSCBuffer(context, 6);


            context.VertexShader.SetShaderResource(2, batch.GrassInstanceBuffer.SRV);
        }


        public void RenderBoundGeom(DeviceContext context, RenderableBoundGeometryInst inst)
        {


            VSEntityVars.Vars.CamRel = new Vector4(inst.Inst.CamRel, 0.0f);
            VSEntityVars.Vars.Orientation = inst.Inst.Orientation;
            VSEntityVars.Vars.Scale = inst.Inst.Scale;
            VSEntityVars.Vars.HasSkeleton = 0;
            VSEntityVars.Vars.HasTransforms = 0; //todo! bounds transforms..?
            VSEntityVars.Vars.TintPaletteIndex = 0;
            VSEntityVars.Vars.IsInstanced = 0;
            VSEntityVars.Update(context);
            VSEntityVars.SetVSCBuffer(context, 2);

            PSGeomVars.Vars.EnableTexture = 0;
            PSGeomVars.Vars.EnableTint = 0;
            PSGeomVars.Vars.EnableNormalMap = 0;
            PSGeomVars.Vars.EnableSpecMap = 0;
            PSGeomVars.Vars.EnableDetailMap = 0;
            PSGeomVars.Vars.IsDecal = 0;
            PSGeomVars.Vars.IsEmissive = 0;
            PSGeomVars.Vars.IsDistMap = 0;
            PSGeomVars.Vars.bumpiness = 0;
            PSGeomVars.Vars.AlphaScale = 1;
            PSGeomVars.Vars.HardAlphaBlend = 0;
            PSGeomVars.Vars.useTessellation = 0;
            PSGeomVars.Vars.detailSettings = Vector4.Zero;
            PSGeomVars.Vars.specMapIntMask = Vector3.Zero;
            PSGeomVars.Vars.specularIntensityMult = 1.0f;
            PSGeomVars.Vars.specularFalloffMult = 1.0f;
            PSGeomVars.Vars.specularFresnel = 1.0f;
            PSGeomVars.Vars.wetnessMultiplier = 0.0f;
            PSGeomVars.Vars.SpecOnly = 0;
            PSGeomVars.Vars.TextureAlphaMask = Vector4.Zero;
            PSGeomVars.Update(context);
            PSGeomVars.SetPSCBuffer(context, 2);

            VSGeomVars.Vars.EnableTint = 0;
            VSGeomVars.Vars.TintYVal = 0.0f;
            VSGeomVars.Vars.IsDecal = 0;
            VSGeomVars.Vars.EnableWind = 0;
            VSGeomVars.Vars.WindOverrideParams = Vector4.Zero;
            VSGeomVars.Vars.globalAnimUV0 = new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
            VSGeomVars.Vars.globalAnimUV1 = new Vector4(0.0f, 1.0f, 0.0f, 0.0f);
            VSGeomVars.Update(context);
            VSGeomVars.SetVSCBuffer(context, 4);


            if (inst.Geom.VertexBuffer != null) //render the triangles
            {
                SetVertexShader(context, VertexType.Default);
                SetInputLayout(context, VertexType.Default);
                inst.Geom.RenderTriangles(context);
            }

            //render the boxes
            if (inst.Geom.BoxBuffer != null)
            {
                context.VertexShader.Set(basicvsbox);
                context.VertexShader.SetShaderResource(1, inst.Geom.BoxBuffer.SRV);
                cube.DrawInstanced(context, inst.Geom.BoxBuffer.StructCount);
            }

            //render the spheres
            if (inst.Geom.SphereBuffer != null)
            {
                context.VertexShader.Set(basicvssphere);
                context.VertexShader.SetShaderResource(1, inst.Geom.SphereBuffer.SRV);
                sphere.DrawInstanced(context, inst.Geom.SphereBuffer.StructCount);
            }

            //render the capsules
            if (inst.Geom.CapsuleBuffer != null)
            {
                context.VertexShader.Set(basicvscapsule);
                context.VertexShader.SetShaderResource(1, inst.Geom.CapsuleBuffer.SRV);
                capsule.DrawInstanced(context, inst.Geom.CapsuleBuffer.StructCount);
            }

            //render the cylinders
            if (inst.Geom.CylinderBuffer != null)
            {
                context.VertexShader.Set(basicvscylinder);
                context.VertexShader.SetShaderResource(1, inst.Geom.CylinderBuffer.SRV);
                cylinder.DrawInstanced(context, inst.Geom.CylinderBuffer.StructCount);
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
            context.VertexShader.SetConstantBuffer(5, null);
            context.VertexShader.SetConstantBuffer(6, null);
            context.VertexShader.SetSampler(0, null);
            context.PixelShader.SetSampler(0, null);
            context.PixelShader.SetShaderResource(0, null);
            context.PixelShader.SetShaderResource(2, null);
            context.PixelShader.SetShaderResource(3, null);
            context.PixelShader.SetShaderResource(4, null);
            context.PixelShader.SetShaderResource(5, null);
            context.VertexShader.SetShaderResource(0, null);
            context.VertexShader.SetShaderResource(1, null);
            context.VertexShader.SetShaderResource(2, null);
            context.VertexShader.Set(null);
            context.PixelShader.Set(null);
        }


        public void Dispose()
        {
            if (disposed) return;

            cube.Dispose();
            sphere.Dispose();
            capsule.Dispose();
            cylinder.Dispose();

            texsampler.Dispose();
            texsampleranis.Dispose();
            texsamplertnt.Dispose();
            texsamplertntyft.Dispose();

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
            InstGlobalVars.Dispose();
            InstLocalVars.Dispose();
            BoneMatrices.Dispose();
            ClothVertices.Dispose();

            basicps.Dispose();
            basicpsdef.Dispose();
            basicvspnct.Dispose();
            basicvspnctt.Dispose();
            basicvspncttt.Dispose();
            basicvspncct.Dispose();
            basicvspncctt.Dispose();
            basicvspnccttt.Dispose();
            basicvspnctx.Dispose();
            basicvspncctx.Dispose();
            basicvspncttx.Dispose();
            basicvspnccttx.Dispose();
            basicvspnctttx.Dispose();
            basicvspncctttx.Dispose();
            basicvspbbnct.Dispose();
            basicvspbbnctx.Dispose();
            basicvspbbnctt.Dispose();
            basicvspbbncttt.Dispose();
            basicvspbbncct.Dispose();
            basicvspbbncctx.Dispose();
            basicvspbbnccttx.Dispose();
            basicvspbbncttx.Dispose();
            basicvsbox.Dispose();
            basicvssphere.Dispose();
            basicvscapsule.Dispose();
            basicvscylinder.Dispose();

            disposed = true;
        }
    }
}
