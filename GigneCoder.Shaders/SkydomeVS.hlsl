#include "Common.hlsli"
#include "Quaternion.hlsli"
#include "Skydome.hlsli"

cbuffer VSSceneVars : register(b1)
{
    float4x4 ViewProj;
    float4x4 ViewInv;
}
cbuffer VSEntityVars : register(b2)
{
    float4 CamRel;
    float4 Orientation;
}
cbuffer VSModelVars : register(b3)
{
    float4x4 Transform;
}


struct VS_INPUT
{
    float4 Position : POSITION;
    float2 Texcoord0 : TEXCOORD0;
    float2 Texcoord1 : TEXCOORD1;
};
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

VS_OUTPUT main(VS_INPUT input)
{
    VS_OUTPUT output;
    float3 ipos = input.Position.xyz * 10.0;
    float3 bpos = mulvq(ipos, Orientation);
    float3 opos = CamRel.xyz + bpos;
    float4 pos = float4(opos, 1);
    float4 cpos = mul(pos, ViewProj);
    cpos.z = DepthFunc(cpos.zw);
    output.Position = cpos;

    float2 v1 = input.Texcoord0;
    float2 v2 = input.Texcoord1;

    float4 r0, r1, r2, r3, r4, r5;

    r0.xy = v1 * 2.0 - 1.0;
    r0.x = dot(r0.xy, r0.xy);
    r0.x = 1 - r0.x;
    r0.y = cloudConstants2.y * 0.1;
    r1.xyz = float3(-ViewInv[3].xy, -horizonLevel); //mov r1.xy, -gViewInverse[3].xyxx
                                                    //mov r1.z, -horizonLevel
    r2.xyz = opos;// float3(opos.xy, -opos.z);               //(mad r2.xyz, v0.wwww, gWorld[3].xyzx, r2.xyzx)
    r1.xyz = r1.xyz + r2.xyz;               //add r1.xyz, r1.xyzx, r2.xyzx
    r0.z = 1.0 / length(r1.xyz);    // sqrt(r0.z);    r0.z = dot(r1.xyz, r1.xyz);             //dp3 r0.z, r1.xyzx, r1.xyzx            //rsq r0.z, r0.z
    r1.yw = -r1.xy*r0.z + sunDirection.xy;  //mad r1.yw, -r1.xxxy, r0.zzzz, sunDirection.xxxy
    r0.zw = r1.xz*r0.z;                     //mul r0.zw, r0.zzzz, r1.xxxz
    r1.xy = r1.yw*r0.y;                     //mul r1.xy, r0.yyyy, r1.ywyy
    output.o1.zw = -r1.xy*r0.x + v1.xy;     //mad o1.zw, -r1.xxxy, r0.xxxx, v1.xxxy
    output.o1.xy = v1.xy;                   //mov o1.xy, v1.xyxx
    
    //r0.x = dot(r2.xyz, r2.xyz);         //dp3 r0.x, r2.xyzx, r2.xyzx
    r0.x = 1.0 / length(r2.xyz);    // sqrt(r0.x);            //rsq r0.x, r0.x
    r0.x = saturate(r0.x * -r2.z + 0.05); //************ invert+FUDGE      //mul_sat r0.x, r0.x, r2.z
    r0.x = r0.x * 5.0;                  //mul r0.x, r0.x, l(5.000000)
    output.o2.w = min(r0.x, 1);         //min o2.w, r0.x, l(1.000000)
    output.o2.xyz = r2.xyz;             //mov o2.xyz, r2.xyzx
    

    r1.xyz = r2.xyz - ViewInv[3].xyz;           //add r1.xyz, r2.xyzx, -gViewInverse[3].xyzx
    r0.x = abs(r0.w) - zenithConstants.x;       //add r0.x, |r0.w|, -zenithConstants.x
    r0.y = 1.0 - zenithConstants.x;             //add r0.y, -zenithConstants.x, l(1.000000)
    r0.x = saturate(r0.x / r0.y);               //div_sat r0.x, r0.x, r0.y
    r0.x = saturate(r0.x / zenithConstants.w);  //div_sat r0.x, r0.x, zenithConstants.w
    r0.y = r0.z*-0.5 + 0.5;                     //mad r0.y, r0.z, l(-0.500000), l(0.500000)
    r0.y = sqrt(r0.y);                          //sqrt r0.y, r0.y
    r0.z = r0.y - azimuthTransitionPosition;    //add r0.z, r0.y, -azimuthTransitionPosition
    r1.w = 1.0 - azimuthTransitionPosition;     //add r1.w, -azimuthTransitionPosition, l(1.000000)
    r0.z = r0.z / r1.w;                         //div r0.z, r0.z, r1.w
    r2.xyz = azimuthWestColor.xyz - azimuthTransitionColor.xyz; //add r2.xyz, azimuthWestColor.xyzx, -azimuthTransitionColor.xyzx
    r2.xyz = r2.xyz*r0.z + azimuthTransitionColor.xyz;          //mad r2.xyz, r0.zzzz, r2.xyzx, azimuthTransitionColor.xyzx
    r0.z = r0.y / azimuthTransitionPosition;                    //div r0.z, r0.y, azimuthTransitionPosition
    r3.xyz = azimuthTransitionColor.xyz - azimuthEastColor.xyz; //add r3.xyz, -azimuthEastColor.xyzx, azimuthTransitionColor.xyzx
    r3.xyz = r3.xyz*r0.z + azimuthEastColor.xyz;    //mad r3.xyz, r0.zzzz, r3.xyzx, azimuthEastColor.xyzx
    r0.z = r0.y < azimuthTransitionPosition;        //lt r0.z, r0.y, azimuthTransitionPosition
    r2.xyz = r0.z ? r3.xyz : r2.xyz;                //movc r2.xyz, r0.zzzz, r3.xyzx, r2.xyzx
    r3.xyz = zenithTransitionColor.xyz - r2.xyz;    //add r3.xyz, -r2.xyzx, zenithTransitionColor.xyzx
    r0.z = zenithConstants.z - zenithConstants.y;   //add r0.z, -zenithConstants.y, zenithConstants.z
    r0.y = r0.y*r0.z + zenithConstants.y;           //mad r0.y, r0.y, r0.z, zenithConstants.y
    r4.xyz = r3.xyz*r0.y + r2.xyz;                  //mad r4.xyz, r0.yyyy, r3.xyzx, r2.xyzx
    r3.xyz = r3.xyz*r0.y;                           //mul r3.xyz, r3.xyzx, r0.yyyy
    r5.xyz = zenithColor.xyz - r4.xyz;              //add r5.xyz, -r4.xyzx, zenithColor.xyzx
    r0.xyz = r5.xyz*r0.x + r4.xyz;                  //mad r0.xyz, r0.xxxx, r5.xyzx, r4.xyzx
    r1.w = abs(r0.w) / zenithConstants.x;           //div r1.w, |r0.w|, zenithConstants.x
    r0.w = abs(r0.w) < zenithConstants.x;           //lt r0.w, |r0.w|, zenithConstants.x
    r2.xyz = r3.xyz*r1.w + r2.xyz;                  //mad r2.xyz, r1.wwww, r3.xyzx, r2.xyzx
    r0.xyz = r0.w ? r2.xyz : r0.xyz;                //movc r0.xyz, r0.wwww, r2.xyzx, r0.xyzx
    output.o3.xyz = r0.xyz*hdrIntensity;            //mul o3.xyz, r0.xyzx, hdrIntensity.xxxx
    output.o3.w = 0;                                //mov o3.w, l(0)

    r0.xy = v1.xy - 0.5;                            //add r0.xy, v1.xyxx, l(-0.500000, -0.500000, 0.000000, 0.000000)
    r2.xyzw = r0.xyyx + speedConstants.zzyy;        //add r2.xyzw, r0.xyyx, speedConstants.zzyy
    r0.xy = r0.xy + speedConstants.x;               //add r0.xy, r0.xyxx, speedConstants.xxxx
    output.o5.zw = r0.xy * smallCloudConstants.x;   //mul o5.zw, r0.xxxy, smallCloudConstants.xxxx

    output.o4.xyzw = r2.xyzw * cloudDetailConstants.yyww;   //mul o4.xyzw, r2.xyzw, cloudDetailConstants.yyww

    output.o5.xy = v2.xy * 64.0;    //mul o5.xy, v2.xyxx, l(64.000000, 64.000000, 0.000000, 0.000000)

    output.o6.xy = v2.xy * 12.0;    //mul o6.xy, v2.xyxx, l(12.000000, 12.000000, 0.000000, 0.000000)

    r0.x = dot(r1.xyz, r1.xyz);     //dp3 r0.x, r1.xyzx, r1.xyzx
    r0.y = sqrt(r0.x);              //sqrt r0.y, r0.x
    r0.x = 1.0 / sqrt(r0.x);        //rsq r0.x, r0.x
    r0.xzw = r1.xyz*r0.x;           //mul r0.xzw, r0.xxxx, r1.xxyz
    //r1.x = r0.y - globalFogParams[0].x; //add r1.x, r0.y, -globalFogParams[0].x
    //max r1.x, r1.x, l(0.000000)
    //div r0.y, r1.x, r0.y
    //mul r0.y, r0.y, r1.z
    //mul r1.y, r0.y, globalFogParams[2].z
    //lt r0.y, l(0.010000), |r0.y|
    //mul r1.z, r1.y, l(-1.442695)
    //exp r1.z, r1.z
    //add r1.z, -r1.z, l(1.000000)
    //div r1.y, r1.z, r1.y
    //movc r0.y, r0.y, r1.y, l(1.000000)
    //mul r1.y, r1.x, globalFogParams[1].w
    //mul r1.x, r1.x, -globalFogParams[1].z
    //mul r1.x, r1.x, l(1.442695)
    //exp r1.x, r1.x
    //add r1.x, -r1.x, l(1.000000)
    //mul r0.y, r0.y, r1.y
    //min r0.y, r0.y, l(1.000000)
    //mul r0.y, r0.y, l(1.442695)
    //exp r0.y, r0.y
    //min r0.y, r0.y, l(1.000000)
    //add r0.y, -r0.y, l(1.000000)
    //mul_sat o7.w, r0.y, globalFogParams[2].y

    //dp3_sat r0.y, r0.xzwx, globalFogParams[3].xyzx
    //dp3_sat r0.x, r0.xzwx, globalFogParams[4].xyzx
    //log r0.x, r0.x
    //mul r0.x, r0.x, globalFogParams[4].w
    //exp r0.x, r0.x
    //log r0.y, r0.y
    //mul r0.y, r0.y, globalFogParams[3].w
    //exp r0.y, r0.y
    //add r1.yzw, -globalFogColorE.xxyz, globalFogColorMoon.xxyz
    //mad r0.xzw, r0.xxxx, r1.yyzw, globalFogColorE.xxyz
    //add r1.yzw, -r0.xxzw, globalFogColor.xxyz
    //mad r0.xyz, r0.yyyy, r1.yzwy, r0.xzwx
    //add r0.xyz, r0.xyzx, -globalFogColorN.xyzx
    //mad o7.xyz, r1.xxxx, r0.xyzx, globalFogColorN.xyzx



    output.o7 = float4(input.Texcoord1, 0, 0);
    return output;
}




/*

sky normal all VS:

v1 = Texcoord0
v2 = Texcoord1


mul r0.xyzw, v0.yyyy, gWorldViewProj[1].xyzw
mad r0.xyzw, v0.xxxx, gWorldViewProj[0].xyzw, r0.xyzw
mad r0.xyzw, v0.zzzz, gWorldViewProj[2].xyzw, r0.xyzw
mad o0.xyzw, v0.wwww, gWorldViewProj[3].xyzw, r0.xyzw

mad r0.xy, v1.xyxx, l(2.000000, 2.000000, 0.000000, 0.000000), l(-1.000000, -1.000000, 0.000000, 0.000000)
dp2 r0.x, r0.xyxx, r0.xyxx
add r0.x, -r0.x, l(1.000000)
mul r0.y, cloudConstants2.y, l(0.100000)
mov r1.xy, -gViewInverse[3].xyxx
mov r1.z, -horizonLevel
mul r2.xyz, v0.yyyy, gWorld[1].xyzx
mad r2.xyz, v0.xxxx, gWorld[0].xyzx, r2.xyzx
mad r2.xyz, v0.zzzz, gWorld[2].xyzx, r2.xyzx
mad r2.xyz, v0.wwww, gWorld[3].xyzx, r2.xyzx
add r1.xyz, r1.xyzx, r2.xyzx
dp3 r0.z, r1.xyzx, r1.xyzx
rsq r0.z, r0.z
mad r1.yw, -r1.xxxy, r0.zzzz, sunDirection.xxxy
mul r0.zw, r0.zzzz, r1.xxxz
mul r1.xy, r0.yyyy, r1.ywyy
mad o1.zw, -r1.xxxy, r0.xxxx, v1.xxxy
mov o1.xy, v1.xyxx


dp3 r0.x, r2.xyzx, r2.xyzx
rsq r0.x, r0.x
mul_sat r0.x, r0.x, r2.z
mul r0.x, r0.x, l(5.000000)
min o2.w, r0.x, l(1.000000)
mov o2.xyz, r2.xyzx

add r1.xyz, r2.xyzx, -gViewInverse[3].xyzx
add r0.x, |r0.w|, -zenithConstants.x
add r0.y, -zenithConstants.x, l(1.000000)
div_sat r0.x, r0.x, r0.y
div_sat r0.x, r0.x, zenithConstants.w
mad r0.y, r0.z, l(-0.500000), l(0.500000)
sqrt r0.y, r0.y
add r0.z, r0.y, -azimuthTransitionPosition
add r1.w, -azimuthTransitionPosition, l(1.000000)
div r0.z, r0.z, r1.w
add r2.xyz, azimuthWestColor.xyzx, -azimuthTransitionColor.xyzx
mad r2.xyz, r0.zzzz, r2.xyzx, azimuthTransitionColor.xyzx
div r0.z, r0.y, azimuthTransitionPosition
add r3.xyz, -azimuthEastColor.xyzx, azimuthTransitionColor.xyzx
mad r3.xyz, r0.zzzz, r3.xyzx, azimuthEastColor.xyzx
lt r0.z, r0.y, azimuthTransitionPosition
movc r2.xyz, r0.zzzz, r3.xyzx, r2.xyzx
add r3.xyz, -r2.xyzx, zenithTransitionColor.xyzx
add r0.z, -zenithConstants.y, zenithConstants.z
mad r0.y, r0.y, r0.z, zenithConstants.y
mad r4.xyz, r0.yyyy, r3.xyzx, r2.xyzx
mul r3.xyz, r3.xyzx, r0.yyyy
add r5.xyz, -r4.xyzx, zenithColor.xyzx
mad r0.xyz, r0.xxxx, r5.xyzx, r4.xyzx
div r1.w, |r0.w|, zenithConstants.x
lt r0.w, |r0.w|, zenithConstants.x
mad r2.xyz, r1.wwww, r3.xyzx, r2.xyzx
movc r0.xyz, r0.wwww, r2.xyzx, r0.xyzx
mul o3.xyz, r0.xyzx, hdrIntensity.xxxx
mov o3.w, l(0)

add r0.xy, v1.xyxx, l(-0.500000, -0.500000, 0.000000, 0.000000)
add r2.xyzw, r0.xyyx, speedConstants.zzyy
add r0.xy, r0.xyxx, speedConstants.xxxx
mul o5.zw, r0.xxxy, smallCloudConstants.xxxx

mul o4.xyzw, r2.xyzw, cloudDetailConstants.yyww

mul o5.xy, v2.xyxx, l(64.000000, 64.000000, 0.000000, 0.000000)

mul o6.xy, v2.xyxx, l(12.000000, 12.000000, 0.000000, 0.000000)

dp3 r0.x, r1.xyzx, r1.xyzx
sqrt r0.y, r0.x
rsq r0.x, r0.x
mul r0.xzw, r0.xxxx, r1.xxyz
add r1.x, r0.y, -globalFogParams[0].x
max r1.x, r1.x, l(0.000000)
div r0.y, r1.x, r0.y
mul r0.y, r0.y, r1.z
mul r1.y, r0.y, globalFogParams[2].z
lt r0.y, l(0.010000), |r0.y|
mul r1.z, r1.y, l(-1.442695)
exp r1.z, r1.z
add r1.z, -r1.z, l(1.000000)
div r1.y, r1.z, r1.y
movc r0.y, r0.y, r1.y, l(1.000000)
mul r1.y, r1.x, globalFogParams[1].w
mul r1.x, r1.x, -globalFogParams[1].z
mul r1.x, r1.x, l(1.442695)
exp r1.x, r1.x
add r1.x, -r1.x, l(1.000000)
mul r0.y, r0.y, r1.y
min r0.y, r0.y, l(1.000000)
mul r0.y, r0.y, l(1.442695)
exp r0.y, r0.y
min r0.y, r0.y, l(1.000000)
add r0.y, -r0.y, l(1.000000)
mul_sat o7.w, r0.y, globalFogParams[2].y

dp3_sat r0.y, r0.xzwx, globalFogParams[3].xyzx
dp3_sat r0.x, r0.xzwx, globalFogParams[4].xyzx
log r0.x, r0.x
mul r0.x, r0.x, globalFogParams[4].w
exp r0.x, r0.x
log r0.y, r0.y
mul r0.y, r0.y, globalFogParams[3].w
exp r0.y, r0.y
add r1.yzw, -globalFogColorE.xxyz, globalFogColorMoon.xxyz
mad r0.xzw, r0.xxxx, r1.yyzw, globalFogColorE.xxyz
add r1.yzw, -r0.xxzw, globalFogColor.xxyz
mad r0.xyz, r0.yyyy, r1.yzwy, r0.xzwx
add r0.xyz, r0.xyzx, -globalFogColorN.xyzx
mad o7.xyz, r1.xxxx, r0.xyzx, globalFogColorN.xyzx

ret 
// Approximately 107 instruction slots used










//
// Generated by Microsoft (R) HLSL Shader Compiler 9.29.952.3111
//
//
// Buffer Definitions: 
//
// cbuffer rage_matrices
// {
//
//   row_major float4x4 gWorld;         // Offset:    0 Size:    64
//   row_major float4x4 gWorldView;     // Offset:   64 Size:    64 [unused]
//   row_major float4x4 gWorldViewProj; // Offset:  128 Size:    64
//   row_major float4x4 gViewInverse;   // Offset:  192 Size:    64
//
// }
//
// cbuffer lighting_globals
// {
//
//   float4 gDirectionalLight;          // Offset:    0 Size:    16 [unused]
//   float4 gDirectionalColour;         // Offset:   16 Size:    16 [unused]
//   int gNumForwardLights;             // Offset:   32 Size:     4 [unused]
//   float4 gLightPositionAndInvDistSqr[8];// Offset:   48 Size:   128 [unused]
//   float4 gLightDirectionAndFalloffExponent[8];// Offset:  176 Size:   128 [unused]
//   float4 gLightColourAndCapsuleExtent[8];// Offset:  304 Size:   128 [unused]
//   float gLightConeScale[8];          // Offset:  432 Size:   116 [unused]
//   float gLightConeOffset[8];         // Offset:  560 Size:   116 [unused]
//   float4 gLightNaturalAmbient0;      // Offset:  688 Size:    16 [unused]
//   float4 gLightNaturalAmbient1;      // Offset:  704 Size:    16 [unused]
//   float4 gLightArtificialIntAmbient0;// Offset:  720 Size:    16 [unused]
//   float4 gLightArtificialIntAmbient1;// Offset:  736 Size:    16 [unused]
//   float4 gLightArtificialExtAmbient0;// Offset:  752 Size:    16 [unused]
//   float4 gLightArtificialExtAmbient1;// Offset:  768 Size:    16 [unused]
//   float4 gDirectionalAmbientColour;  // Offset:  784 Size:    16 [unused]
//   float4 globalFogParams[5];         // Offset:  800 Size:    80
//   float4 globalFogColor;             // Offset:  880 Size:    16
//   float4 globalFogColorE;            // Offset:  896 Size:    16
//   float4 globalFogColorN;            // Offset:  912 Size:    16
//   float4 globalFogColorMoon;         // Offset:  928 Size:    16
//   float4 gReflectionTweaks;          // Offset:  944 Size:    16 [unused]
//
// }
//
// cbuffer sky_system_locals
// {
//
//   float3 azimuthEastColor;           // Offset:    0 Size:    12
//   float3 azimuthWestColor;           // Offset:   16 Size:    12
//   float3 azimuthTransitionColor;     // Offset:   32 Size:    12
//   float azimuthTransitionPosition;   // Offset:   44 Size:     4
//   float3 zenithColor;                // Offset:   48 Size:    12
//   float3 zenithTransitionColor;      // Offset:   64 Size:    12
//   float4 zenithConstants;            // Offset:   80 Size:    16
//   float4 skyPlaneColor;              // Offset:   96 Size:    16 [unused]
//   float4 skyPlaneParams;             // Offset:  112 Size:    16 [unused]
//   float hdrIntensity;                // Offset:  128 Size:     4
//   float3 sunColor;                   // Offset:  132 Size:    12 [unused]
//   float3 sunColorHdr;                // Offset:  144 Size:    12 [unused]
//   float3 sunDiscColorHdr;            // Offset:  160 Size:    12 [unused]
//   float4 sunConstants;               // Offset:  176 Size:    16 [unused]
//   float3 sunDirection;               // Offset:  192 Size:    12
//   float3 sunPosition;                // Offset:  208 Size:    12 [unused]
//   float3 cloudBaseMinusMidColour;    // Offset:  224 Size:    12 [unused]
//   float3 cloudMidColour;             // Offset:  240 Size:    12 [unused]
//   float3 cloudShadowMinusBaseColourTimesShadowStrength;// Offset:  256 Size:    12 [unused]
//   float4 cloudDetailConstants;       // Offset:  272 Size:    16
//   float4 cloudConstants1;            // Offset:  288 Size:    16 [unused]
//   float4 cloudConstants2;            // Offset:  304 Size:    16
//   float4 smallCloudConstants;        // Offset:  320 Size:    16
//   float3 smallCloudColorHdr;         // Offset:  336 Size:    12 [unused]
//   float4 effectsConstants;           // Offset:  352 Size:    16 [unused]
//   float horizonLevel;                // Offset:  368 Size:     4
//   float3 speedConstants;             // Offset:  372 Size:    12
//   float starfieldIntensity;          // Offset:  384 Size:     4 [unused]
//   float3 moonDirection;              // Offset:  388 Size:    12 [unused]
//   float3 moonPosition;               // Offset:  400 Size:    12 [unused]
//   float moonIntensity;               // Offset:  412 Size:     4 [unused]
//   float3 lunarCycle;                 // Offset:  416 Size:    12 [unused]
//   float3 moonColor;                  // Offset:  432 Size:    12 [unused]
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
// rage_matrices                     cbuffer      NA          NA            cb1      1 
// lighting_globals                  cbuffer      NA          NA            cb3      1 
// sky_system_locals                 cbuffer      NA          NA           cb12      1 
//
//
//
// Input signature:
//
// Name                 Index   Mask Register SysValue  Format   Used
// -------------------- ----- ------ -------- -------- ------- ------
// POSITION                 0   xyzw        0     NONE   float   xyzw
// TEXCOORD                 0   xy          1     NONE   float   xy  
// TEXCOORD                 1   xy          2     NONE   float   xy  
// SV_InstanceID            0   x           3   INSTID    uint       
//
//
// Output signature:
//
// Name                 Index   Mask Register SysValue  Format   Used
// -------------------- ----- ------ -------- -------- ------- ------
// SV_Position              0   xyzw        0      POS   float   xyzw
// TEXCOORD                 0   xyzw        1     NONE   float   xyzw
// TEXCOORD                 1   xyzw        2     NONE   float   xyzw
// TEXCOORD                 2   xyzw        3     NONE   float   xyzw
// TEXCOORD                 3   xyzw        4     NONE   float   xyzw
// TEXCOORD                 4   xyzw        5     NONE   float   xyzw
// TEXCOORD                 5   xy          6     NONE   float   xy  
// TEXCOORD                 6   xyzw        7     NONE   float   xyzw
//
vs_4_0
dcl_constantbuffer CB1[16], immediateIndexed
dcl_constantbuffer CB3[59], immediateIndexed
dcl_constantbuffer CB12[24], immediateIndexed
dcl_input v0.xyzw
dcl_input v1.xy
dcl_input v2.xy
dcl_output_siv o0.xyzw, position
dcl_output o1.xyzw
dcl_output o2.xyzw
dcl_output o3.xyzw
dcl_output o4.xyzw
dcl_output o5.xyzw
dcl_output o6.xy
dcl_output o7.xyzw
dcl_temps 6

*/