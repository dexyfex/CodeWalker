#include "Skydome.hlsli"

Texture2D<float4> Starmap : register(t0);
SamplerState TextureSS : register(s0);

cbuffer PSSceneVars : register(b1)
{
    float4 LightDirection;
    uint EnableHDR;
    uint Pad0;
    uint Pad1;
    uint Pad2;
}

struct VS_OUTPUT
{
    float4 Position : SV_POSITION; //         0   xyzw        0      POS   float   xyzw
    float4 o1 : TEXCOORD0; //                 0   xyzw        1     NONE   float   xyzw
    float4 o2 : TEXCOORD1; //                 1   xyzw        2     NONE   float   xyzw
    float4 o3 : TEXCOORD2; //                 2   xyzw        3     NONE   float   xyzw
    float4 o4 : TEXCOORD3; //                 3   xyzw        4     NONE   float   xyzw
    float4 o5 : TEXCOORD4; //                 4   xyzw        5     NONE   float   xyzw
    float2 o6 : TEXCOORD5; //                 5   xy          6     NONE   float   xy  
    float4 o7 : TEXCOORD6; //                 6   xyzw        7     NONE   float   xyzw
};

float4 main(VS_OUTPUT input) : SV_TARGET
{
    //return float4(1,0,0,1);
    float4 sf = Starmap.Sample(TextureSS, input.o6);
    sf.rgb = saturate(sf.rgb*2.0 - 0.1);

    float3 skybase = input.o3.rgb;

    if (EnableHDR == 0)
    {
        sf.rgb *= max(starfieldIntensity, 1.25);
        if (hdrIntensity != 0.0)
        {
            sf.rgb *= saturate(1.0f / (hdrIntensity*25.0f));//fake hiding stars when no HDR
        }
    }
    else
    {
        sf.rgb *= starfieldIntensity;
    }

    //float4 zen = zenithColor * hdrIntensity;
    //sf.rgb += zen.rgb;


    //sf.rgb += input.o3.rgb * hdrIntensity;


    float4 o2 = input.o2;

    float4 r0, r1, r2, r3, r4;
    float poslen = length(o2); //4d length... 
    r2.xyz = o2.xyz / poslen; //normalized
    r4.x = dot(r2.xyz, -sunDirection.xyz);  //dp3 r4.x, r2.xyzx, -sunDirection.xyzx
    ///return float4(r4.xxx, 1);
    //r4.y = dot(r2.xyz, -moonDirection.xyz);  //dp3 r4.y, r2.xyzx, moonDirection.xyzx;//.yzwy
    r0.z = r4.x*-sunConstants.x +sunConstants.y; //mad r0.z, -sunConstants.x, r4.x, sunConstants.y //mie phase, scatter
    r0.z = log(abs(r0.z));  //log r0.z, |r0.z|
    r0.z = r0.z * 1.5;      //mul r0.z, r0.z, l(1.500000)
    r0.z = exp(r0.z);       //exp r0.z, r0.z
    r0.w = r4.x*r4.x + 1.0; //mad r0.w, r4.x, r4.x, l(1.000000)
    r4.x = saturate(-r4.x); //mov_sat r4.x, -r4.x
    //r2.xy = 1.0 - r4.xy;    //add r2.xy, -r4.xyxx, l(1.000000, 1.000000, 0.000000, 0.000000)
    r0.z = r0.w / r0.z;     //div r0.z, r0.w, r0.z
    r0.w = r0.z*sunConstants.z; //mul r0.w, r0.z, sunConstants.z //mie range?
    r0.z = saturate(1.0 - r0.w); //mad_sat r0.z, -r0.z, sunConstants.z, l(1.000000)
    r0.w = saturate(r0.w);          //mov_sat r0.w, r0.w
    r4.xyz = r0.w*sunColorHdr.xyz;      //mul r4.xyz, r0.wwww, sunColorHdr.xyzx
    r4.xyz = r4.xyz*sunConstants.w;         //mul r4.xyz, r4.xyzx, sunConstants.wwww //scatter intensity?    
    r4.xyz = skybase*r0.z + r4.xyz;   //mad r4.xyz, v3.xyzx, r0.zzzz, r4.xyzx

    sf.rgb += r4.xyz;

    if (EnableHDR == 0)
    {
        sf = saturate(sf);
    }
    else
    {
        sf.a = saturate(sf.a);
    }

    return sf;

    //float f = frac(tc.x);
    //return (f < 0.5) ? float4(1, 0, 1, 1) : sf;
}




/*

sky normal all PS:

ps_4_0
dcl_constantbuffer CB2[14], immediateIndexed
dcl_constantbuffer CB12[28], immediateIndexed
dcl_sampler s3, mode_default
dcl_sampler s4, mode_default
dcl_sampler s5, mode_default
dcl_sampler s6, mode_default
dcl_resource_texture2d (float,float,float,float) t3  perlinSampler
dcl_resource_texture2d (float,float,float,float) t4  highDetailSampler
dcl_resource_texture2d (float,float,float,float) t5  starFieldSampler
dcl_resource_texture2d (float,float,float,float) t6  ditherSampler
dcl_input_ps linear v1.xyzw
dcl_input_ps linear v2.xyzw
dcl_input_ps linear v3.xyz
dcl_input_ps linear v4.xyzw
dcl_input_ps linear v5.xyzw
dcl_input_ps linear v6.xy
dcl_input_ps linear v7.xyzw
dcl_output o0.xyzw
dcl_temps 6
sample r0.xyzw, v4.xyxx, highDetailSampler.xyzw, s4
add r0.x, r0.x, l(-0.500000)
mul r0.x, r0.x, cloudDetailConstants.x
sample r1.xyzw, v5.zwzz, highDetailSampler.xyzw, s4
add r0.z, r1.x, l(-0.500000)
mul r0.y, r0.z, smallCloudConstants.y
sample r1.xyzw, v4.zwzz, highDetailSampler.xyzw, s4
add r1.x, r1.x, l(-0.500000)
mul r0.z, r1.x, cloudConstants2.z
sample r2.xyzw, v1.zwzz, perlinSampler.xyzw, s3
mad r0.z, r0.z, cloudDetailConstants.z, r2.x
mul r0.z, r0.z, cloudConstants1.x
sample r3.xyzw, v1.xyxx, perlinSampler.xyzw, s3
mul r0.z, r0.z, r3.x
mad_sat r0.w, -r0.z, l(2.000000), l(1.000000)
mul r0.xy, r0.wwww, r0.xyxx
mul r0.xy, r3.zzzz, r0.xyxx
mov r4.x, cloudDetailConstants.z
mov r4.y, smallCloudConstants.y
add r1.y, -r3.z, l(1.000000)
mad r0.xy, r1.xyxx, r4.xyxx, r0.xyxx
add r0.w, r0.x, r2.y
mad r1.xyz, r0.wwww, cloudShadowMinusBaseColourTimesShadowStrength.xyzx, cloudBaseMinusMidColour.xyzx
mad r1.xyz, r0.zzzz, r1.xyzx, cloudMidColour.xyzx
mov r3.y, l(0)
add r0.xy, r0.xyxx, r3.xyxx
mul r2.x, r0.x, cloudConstants1.y
mul r2.y, r0.y, smallCloudConstants.z
add_sat r0.x, -r0.x, l(1.000000)
mov r3.y, -smallCloudConstants.w
mov r3.xz, -cloudConstants1.zzwz
add_sat r0.yz, r2.xxyx, r3.xxyx
mul r0.yz, r0.yyzy, r0.yyzy
dp4 r0.w, v2.xyzw, v2.xyzw
rsq r0.w, r0.w
mul r2.xyz, r0.wwww, v2.xyzx
mad_sat r0.w, r2.z, l(5.000000), r3.z
mul r3.xy, r0.wwww, r0.yzyy
mad r0.y, -r0.y, r0.w, l(1.000000)
mul r0.y, r0.y, r3.y
mad r0.y, r0.y, l(0.300000), r3.x

dp3 r4.x, r2.xyzx, -sunDirection.xyzx
dp3 r4.y, r2.xyzx, moonDirection.xyzx;//.yzwy
mad r0.z, -sunConstants.x, r4.x, sunConstants.y
log r0.z, |r0.z|
mul r0.z, r0.z, l(1.500000)
exp r0.z, r0.z
mad r0.w, r4.x, r4.x, l(1.000000)
mov_sat r4.x, -r4.x
add r2.xy, -r4.xyxx, l(1.000000, 1.000000, 0.000000, 0.000000)
div r0.z, r0.w, r0.z
mul r0.w, r0.z, sunConstants.z
mad_sat r0.z, -r0.z, sunConstants.z, l(1.000000)
mov_sat r0.w, r0.w
mul r4.xyz, r0.wwww, sunColorHdr.xyzx
mul r4.xyz, r4.xyzx, sunConstants.wwww
mad r4.xyz, v3.xyzx, r0.zzzz, r4.xyzx

add r5.xyz, -r4.xyzx, smallCloudColorHdr.xyzx
mad r4.xyz, r3.yyyy, r5.xyzx, r4.xyzx
mad r1.xyz, r1.xyzx, cloudConstants2.wwww, -r4.xyzx
mad r1.xyz, r3.xxxx, r1.xyzx, r4.xyzx
add r0.z, r3.y, r3.x
add_sat o0.w, -r0.z, v2.w
div r0.zw, r2.xxxy, effectsConstants.xxxz
ge r1.w, r2.y, l(0.000545)
and r1.w, r1.w, l(0x3f800000)
mul_sat r0.zw, r0.zzzw, l(0.000000, 0.000000, 100.000000, 100.000000)
add r0.zw, -r0.zzzw, l(0.000000, 0.000000, 1.000000, 1.000000)
mul r0.zw, r0.yyyy, r0.zzzw
add r0.y, -r0.y, l(1.000000)
mul r0.xz, r0.xxxx, r0.zzwz
mul r0.xz, r0.xxzx, r0.xxzx
mul r0.xz, r0.xxzx, r0.xxzx
mad r0.w, lunarCycle.y, l(0.500000), l(0.500000)
mul r0.w, r0.w, effectsConstants.w
mul r0.z, r0.w, r0.z
mul r0.x, r0.x, effectsConstants.y
mul r2.xyz, r0.zzzz, moonColor.xyzx
mad r0.xzw, sunColor.yyzw, r0.xxxx, r2.xxyz
add r0.xzw, r0.xxzw, r1.xxyz

sample r2.xyzw, v6.xyxx, starFieldSampler.xyzw, s5
mad_sat r1.xyz, r2.xyzx, l(2.000000, 2.000000, 2.000000, 0.000000), l(-0.100000, -0.100000, -0.100000, 0.000000)
mul r1.xyz, r1.xyzx, starfieldIntensity.xxxx
mul r1.xyz, r1.wwww, r1.xyzx
mad r0.xyz, r1.xyzx, r0.yyyy, r0.xzwx
add r1.xyz, -r0.xyzx, v7.xyzx
mad r0.xyz, v7.wwww, r1.xyzx, r0.xyzx

sample r1.xyzw, v5.xyxx, ditherSampler.xyzw, s6
add r0.w, r1.x, l(-0.500000)
mad r0.xyz, r0.wwww, l(0.000100, 0.000100, 0.000100, 0.000000), r0.xyzx
mul o0.xyz, r0.xyzx, globalScalars3.zzzz

ret 
// Approximately 92 instruction slots used





//
// Generated by Microsoft (R) HLSL Shader Compiler 9.29.952.3111
//
//
// Buffer Definitions: 
//
// cbuffer misc_globals
// {
//
//   float4 globalFade;                 // Offset:    0 Size:    16 [unused]
//   float globalHeightScale;           // Offset:   16 Size:     4 [unused]
//   float4 g_Rage_Tessellation_CameraPosition;// Offset:   32 Size:    16 [unused]
//   float4 g_Rage_Tessellation_CameraZAxis;// Offset:   48 Size:    16 [unused]
//   float4 g_Rage_Tessellation_ScreenSpaceErrorParams;// Offset:   64 Size:    16 [unused]
//   float4 g_Rage_Tessellation_LinearScale;// Offset:   80 Size:    16 [unused]
//   float4 g_Rage_Tessellation_Frustum[4];// Offset:   96 Size:    64 [unused]
//   float4 g_Rage_Tessellation_Epsilons;// Offset:  160 Size:    16 [unused]
//   float4 globalScalars;              // Offset:  176 Size:    16 [unused]
//   float4 globalScalars2;             // Offset:  192 Size:    16 [unused]
//   float4 globalScalars3;             // Offset:  208 Size:    16
//   float4 globalScreenSize;           // Offset:  224 Size:    16 [unused]
//   uint4 gTargetAAParams;             // Offset:  240 Size:    16 [unused]
//   float4 colorize;                   // Offset:  256 Size:    16 [unused]
//   float4 gGlobalParticleShadowBias;  // Offset:  272 Size:    16 [unused]
//   float gGlobalParticleDofAlphaScale;// Offset:  288 Size:     4 [unused]
//   float gGlobalFogIntensity;         // Offset:  292 Size:     4 [unused]
//   float4 gPlayerLFootPos;            // Offset:  304 Size:    16 [unused]
//   float4 gPlayerRFootPos;            // Offset:  320 Size:    16 [unused]
//
// }
//
// cbuffer sky_system_locals
// {
//
//   float3 azimuthEastColor;           // Offset:    0 Size:    12 [unused]
//   float3 azimuthWestColor;           // Offset:   16 Size:    12 [unused]
//   float3 azimuthTransitionColor;     // Offset:   32 Size:    12 [unused]
//   float azimuthTransitionPosition;   // Offset:   44 Size:     4 [unused]
//   float3 zenithColor;                // Offset:   48 Size:    12 [unused]
//   float3 zenithTransitionColor;      // Offset:   64 Size:    12 [unused]
//   float4 zenithConstants;            // Offset:   80 Size:    16 [unused]
//   float4 skyPlaneColor;              // Offset:   96 Size:    16 [unused]
//   float4 skyPlaneParams;             // Offset:  112 Size:    16 [unused]
//   float hdrIntensity;                // Offset:  128 Size:     4 [unused]
//   float3 sunColor;                   // Offset:  132 Size:    12
//   float3 sunColorHdr;                // Offset:  144 Size:    12
//   float3 sunDiscColorHdr;            // Offset:  160 Size:    12 [unused]
//   float4 sunConstants;               // Offset:  176 Size:    16
//   float3 sunDirection;               // Offset:  192 Size:    12
//   float3 sunPosition;                // Offset:  208 Size:    12 [unused]
//   float3 cloudBaseMinusMidColour;    // Offset:  224 Size:    12
//   float3 cloudMidColour;             // Offset:  240 Size:    12
//   float3 cloudShadowMinusBaseColourTimesShadowStrength;// Offset:  256 Size:    12
//   float4 cloudDetailConstants;       // Offset:  272 Size:    16
//   float4 cloudConstants1;            // Offset:  288 Size:    16
//   float4 cloudConstants2;            // Offset:  304 Size:    16
//   float4 smallCloudConstants;        // Offset:  320 Size:    16
//   float3 smallCloudColorHdr;         // Offset:  336 Size:    12
//   float4 effectsConstants;           // Offset:  352 Size:    16
//   float horizonLevel;                // Offset:  368 Size:     4 [unused]
//   float3 speedConstants;             // Offset:  372 Size:    12 [unused]
//   float starfieldIntensity;          // Offset:  384 Size:     4
//   float3 moonDirection;              // Offset:  388 Size:    12
//   float3 moonPosition;               // Offset:  400 Size:    12 [unused]
//   float moonIntensity;               // Offset:  412 Size:     4 [unused]
//   float3 lunarCycle;                 // Offset:  416 Size:    12
//   float3 moonColor;                  // Offset:  432 Size:    12
//   float noiseFrequency;              // Offset:  444 Size:     4 [unused]
//   float noiseScale;                  // Offset:  448 Size:     4 [unused]
//   float noiseThreshold;              // Offset:  452 Size:     4 [unused]
//   float noiseSoftness;               // Offset:  456 Size:     4 [unused]
//   float noiseDensityOffset;          // Offset:  460 Size:     4 [unused]
//   float2 noisePhase;                 // Offset:  464 Size:     8 [unused]
//
// }
//
//
// Resource Bindings:
//
// Name                                 Type  Format         Dim      HLSL Bind  Count
// ------------------------------ ---------- ------- ----------- -------------- ------
// perlinSampler                     sampler      NA          NA             s3      1 
// highDetailSampler                 sampler      NA          NA             s4      1 
// starFieldSampler                  sampler      NA          NA             s5      1 
// ditherSampler                     sampler      NA          NA             s6      1 
// perlinSampler                     texture  float4          2d             t3      1 
// highDetailSampler                 texture  float4          2d             t4      1 
// starFieldSampler                  texture  float4          2d             t5      1 
// ditherSampler                     texture  float4          2d             t6      1 
// misc_globals                      cbuffer      NA          NA            cb2      1 
// sky_system_locals                 cbuffer      NA          NA           cb12      1 
//
//
//
// Input signature:
//
// Name                 Index   Mask Register SysValue  Format   Used
// -------------------- ----- ------ -------- -------- ------- ------
// SV_Position              0   xyzw        0      POS   float       
// TEXCOORD                 0   xyzw        1     NONE   float   xyzw
// TEXCOORD                 1   xyzw        2     NONE   float   xyzw
// TEXCOORD                 2   xyzw        3     NONE   float   xyz 
// TEXCOORD                 3   xyzw        4     NONE   float   xyzw
// TEXCOORD                 4   xyzw        5     NONE   float   xyzw
// TEXCOORD                 5   xy          6     NONE   float   xy  
// TEXCOORD                 6   xyzw        7     NONE   float   xyzw
//
//
// Output signature:
//
// Name                 Index   Mask Register SysValue  Format   Used
// -------------------- ----- ------ -------- -------- ------- ------
// SV_Target                0   xyzw        0   TARGET   float   xyzw
//



*/