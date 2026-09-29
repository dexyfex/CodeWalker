#include "Shadowmap.hlsli"

Texture2D<float4> Colourmap : register(t0);
Texture2D<float4> Bumpmap : register(t2);
Texture2D<float4> Foammap : register(t3);
Texture2D<float4> WaterBumpSampler : register(t4); // graphics.ytd, waterbump and waterbump2
Texture2D<float4> WaterBumpSampler2 : register(t5);
Texture2D<float4> WaterFog : register(t6);
SamplerState TextureSS : register(s0);


cbuffer PSSceneVars : register(b0)
{
    ShaderGlobalLightParams GlobalLights;
    uint EnableShadows;
    uint RenderMode; //0=default, 1=normals, 2=tangents, 3=colours, 4=texcoords, 5=diffuse, 6=normalmap, 7=spec, 8=direct
    uint RenderModeIndex;
    uint RenderSamplerCoord;
    uint EnableWaterbumps; //if the waterbump textures are ready..
    uint EnableFogtex; //if the fog texture is ready
    uint ScnPad1;
    uint ScnPad2;
    float4 gFlowParams;
    float4 CameraPos;
    float4 WaterFogParams; //xy = base location, zw = inverse size
}
cbuffer PSGeomVars : register(b2)
{
    uint EnableTexture;
    uint EnableBumpMap;
    uint EnableFoamMap;
    uint ShaderMode;
    float SpecularIntensity;
    float SpecularFalloff;
    float GeoPad1;
    float GeoPad2;
    float WaveOffset; //for terrainfoam
    float WaterHeight; //for terrainfoam
    float WaveMovement; //for terrainfoam
    float HeightOpacity; //for terrainfoam
    float RippleSpeed;
    float RippleScale;
    float RippleBumpiness;
    float GeoPad3;
}


struct VS_OUTPUT
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float2 Texcoord0 : TEXCOORD0;
    float4 Flow : TEXCOORD1;
    float4 Shadows : TEXCOORD3;
    float4 LightShadow : TEXCOORD4;
    float4 Colour0 : COLOR0;
    float4 Tangent : TEXCOORD5;
    float4 Bitangent : TEXCOORD6;
    float3 CamRelPos : TEXCOORD7;
};

struct PS_OUTPUT
{
    float4 Diffuse : SV_Target0;
    float4 Normal : SV_Target1;
    float4 Specular : SV_Target2;
    float4 Irradiance : SV_Target3;
};





float3 RippleNormal(VS_OUTPUT input, float3 worldpos)
{
    ////
    //// Input signature:
    ////
    //// Name                 Index   Mask Register SysValue  Format   Used
    //// -------------------- ----- ------ -------- -------- ------- ------
    //// SV_Position              0   xyzw        0      POS   float       
    //// TEXCOORD                 0   xyzw        1     NONE   float   xyzw
    //// TEXCOORD                 1   xyzw        2     NONE   float   xyzw
    //// TEXCOORD                 2   xyzw        3     NONE   float     zw
    //// TEXCOORD                 3   xyzw        4     NONE   float   xyzw  //NORMAL +half
    //// TEXCOORD                 4   xyzw        5     NONE   float     zw  //FLOW
    ////
    //

    float3 norm = input.Normal.xyz;
    float v2w = input.Colour0.r; //vertex red channel

    float4 r0, r1, r2, r3, r4;

    r0.xy = input.Flow.zw * RippleSpeed; //mul r0.xy, v5.zwzz, RippleSpeed
    r1 = -r0.xyxy * gFlowParams.xxyy + worldpos.xyxy; //mad r1.xyzw, -r0.xyxy, gFlowParams.xxyy, v2.xyxy
    r0.x = min(sqrt(dot(r0.xy, r0.xy)), 1.0); //dp2 r0.x, r0.xyxx, r0.xyxx    //sqrt r0.x, r0.x    //min r0.x, r0.x, l(1.000000)
    r0.yz = r1.xy * RippleScale; //mul r0.yz, r1.xxyx, RippleScale
    r1.xy = r1.zw * RippleScale + 0.5; //mad r1.xy, r1.zwzz, RippleScale, l(0.500000, 0.500000, 0.000000, 0.000000)
    r1.xy = r1.xy * 2.3; //mul r1.xy, r1.xyxx, l(2.300000, 2.300000, 0.000000, 0.000000)
    r0.yz = r0.yz * 2.3; //mul r0.yz, r0.yyzy, l(0.000000, 2.300000, 2.300000, 0.000000)
    r2 = WaterBumpSampler2.Sample(TextureSS, r0.yz); //sample r2.xyzw, r0.yzyy, WaterBumpSampler2.xyzw, s14
    r3 = WaterBumpSampler.Sample(TextureSS, r0.yz); //sample r3.xyzw, r0.yzyy, WaterBumpSampler.xyzw, s10
    r4 = WaterBumpSampler2.Sample(TextureSS, r1.xy); //sample r4.xyzw, r1.xyxx, WaterBumpSampler2.xyzw, s14
    r1 = WaterBumpSampler.Sample(TextureSS, r1.xy); //sample r1.xyzw, r1.xyxx, WaterBumpSampler.xyzw, s10
    r3.zw = r1.xy; //mov r3.zw, r1.xxxy
    r2.zw = r4.xy; //mov r2.zw, r4.xxxy
    r1 = r2 + r3; //add r1.xyzw, r2.xyzw, r3.xyzw
    r2 = r3 + 0.5; //add r2.xyzw, r3.xyzw, l(0.500000, 0.500000, 0.500000, 0.500000)
    r1 = r1 - r2; //add r1.xyzw, r1.xyzw, -r2.xyzw
    r0 = r1 * r0.x + r2; //mad r0.xyzw, r0.xxxx, r1.xyzw, r2.xyzw
    r0 = r0 * 2 - 2; //mad r0.xyzw, r0.xyzw, l(2.000000, 2.000000, 2.000000, 2.000000), l(-2.000000, -2.000000, -2.000000, -2.000000)
    r0 = r0 * gFlowParams.zzww; //mul r0.xyzw, r0.xyzw, gFlowParams.zzww
    r0.xy = r0.xy + r0.zw; //add r0.xy, r0.zwzz, r0.xyxx
    r0.zw = r0.xy * RippleBumpiness; //mul r0.zw, r0.xxxy, RippleBumpiness
    //r0.x = sqrt(dot(r0.xy, r0.xy)); //dp2 r0.x, r0.xyxx, r0.xyxx    //sqrt r0.x, r0.x
    //r0.x = r0.x * 0.27 + 0.44;      //mad r0.x, r0.x, l(0.270000), l(0.440000)
    r1.xy = r0.zw * v2w + norm.xy; //mad r1.xy, r0.zwzz, v2.wwww, v4.xyxx
    r1.z = norm.z; //mov r1.z, v4.z
    //return normalize(r1.xyz);
    r0.y = dot(r1.xyz, r1.xyz); //dp3 r0.y, r1.xyzx, r1.xyzx
    r0.y = 1.0 / sqrt(r0.y); //rsq r0.y, r0.y
    r2.xyz = -r1.xyz * r0.y + float3(0, 0, 1); //mad r2.xyz, -r1.xyzx, r0.yyyy, l(0.000000, 0.000000, 1.000000, 0.000000)
    r0.yzw = r1.xyz * r0.y; //mul r0.yzw, r0.yyyy, r1.xxyz
    //r1.xyz = r2.xyz * 0.833333 + r0.yzw; //mad r1.xyz, r2.xyzx, l(0.833333, 0.833333, 0.833333, 0.000000), r0.yzwy
    //r2.xyz = worldpos - //add r2.xyz, v2.xyzx, -gViewInverse[3].xyzx

    return r0.yzw;
    //return r0.wzy;
    ////return float3(r0.w, r0.z, r0.y);

    ////return normalize(input.Normal);
}
