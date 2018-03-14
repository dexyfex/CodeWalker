#include "Shadowmap.hlsli"

Texture2D<float4> Colourmap : register(t0);
Texture2D<float4> Bumpmap : register(t2);
Texture2D<float4> Foammap : register(t3);
Texture2D<float4> WaterBumpSampler : register(t4);// graphics.ytd, waterbump and waterbump2
Texture2D<float4> WaterBumpSampler2 : register(t5);
Texture2D<float4> WaterFog : register(t6);
SamplerState TextureSS : register(s0);


cbuffer PSSceneVars : register(b0)
{
    ShaderGlobalLightParams GlobalLights;
    uint EnableShadows;
    uint RenderMode;//0=default, 1=normals, 2=tangents, 3=colours, 4=texcoords, 5=diffuse, 6=normalmap, 7=spec, 8=direct
    uint RenderModeIndex;
    uint RenderSamplerCoord;
    uint EnableWaterbumps;//if the waterbump textures are ready..
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
    float4 Position  : SV_POSITION;
    float3 Normal    : NORMAL;
    float2 Texcoord0 : TEXCOORD0;
    float4 Flow      : TEXCOORD1;
    float4 Shadows   : TEXCOORD3;
    float4 LightShadow : TEXCOORD4;
    float4 Colour0   : COLOR0;
    float4 Tangent   : TEXCOORD5;
    float4 Bitangent : TEXCOORD6;
    float3 CamRelPos : TEXCOORD7;
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

    r0.xy = input.Flow.zw * RippleSpeed;    //mul r0.xy, v5.zwzz, RippleSpeed
    r1 = -r0.xyxy * gFlowParams.xxyy + worldpos.xyxy; //mad r1.xyzw, -r0.xyxy, gFlowParams.xxyy, v2.xyxy
    r0.x = min(sqrt(dot(r0.xy, r0.xy)), 1.0);       //dp2 r0.x, r0.xyxx, r0.xyxx    //sqrt r0.x, r0.x    //min r0.x, r0.x, l(1.000000)
    r0.yz = r1.xy * RippleScale;        //mul r0.yz, r1.xxyx, RippleScale
    r1.xy = r1.zw * RippleScale + 0.5;  //mad r1.xy, r1.zwzz, RippleScale, l(0.500000, 0.500000, 0.000000, 0.000000)
    r1.xy = r1.xy * 2.3;                //mul r1.xy, r1.xyxx, l(2.300000, 2.300000, 0.000000, 0.000000)
    r0.yz = r0.yz * 2.3;                //mul r0.yz, r0.yyzy, l(0.000000, 2.300000, 2.300000, 0.000000)
    r2 = WaterBumpSampler2.Sample(TextureSS, r0.yz);    //sample r2.xyzw, r0.yzyy, WaterBumpSampler2.xyzw, s14
    r3 = WaterBumpSampler.Sample(TextureSS, r0.yz);     //sample r3.xyzw, r0.yzyy, WaterBumpSampler.xyzw, s10
    r4 = WaterBumpSampler2.Sample(TextureSS, r1.xy);    //sample r4.xyzw, r1.xyxx, WaterBumpSampler2.xyzw, s14
    r1 = WaterBumpSampler.Sample(TextureSS, r1.xy);     //sample r1.xyzw, r1.xyxx, WaterBumpSampler.xyzw, s10
    r3.zw = r1.xy;  //mov r3.zw, r1.xxxy
    r2.zw = r4.xy;  //mov r2.zw, r4.xxxy
    r1 = r2 + r3;   //add r1.xyzw, r2.xyzw, r3.xyzw
    r2 = r3 + 0.5;  //add r2.xyzw, r3.xyzw, l(0.500000, 0.500000, 0.500000, 0.500000)
    r1 = r1 - r2;   //add r1.xyzw, r1.xyzw, -r2.xyzw
    r0 = r1 * r0.x + r2;    //mad r0.xyzw, r0.xxxx, r1.xyzw, r2.xyzw
    r0 = r0 * 2 - 2;        //mad r0.xyzw, r0.xyzw, l(2.000000, 2.000000, 2.000000, 2.000000), l(-2.000000, -2.000000, -2.000000, -2.000000)
    r0 = r0 * gFlowParams.zzww; //mul r0.xyzw, r0.xyzw, gFlowParams.zzww
    r0.xy = r0.xy + r0.zw;      //add r0.xy, r0.zwzz, r0.xyxx
    r0.zw = r0.xy * RippleBumpiness;//mul r0.zw, r0.xxxy, RippleBumpiness
    //r0.x = sqrt(dot(r0.xy, r0.xy)); //dp2 r0.x, r0.xyxx, r0.xyxx    //sqrt r0.x, r0.x
    //r0.x = r0.x * 0.27 + 0.44;      //mad r0.x, r0.x, l(0.270000), l(0.440000)
    r1.xy = r0.zw * v2w + norm.xy;  //mad r1.xy, r0.zwzz, v2.wwww, v4.xyxx
    r1.z = norm.z;                  //mov r1.z, v4.z
    //return normalize(r1.xyz);
    r0.y = dot(r1.xyz, r1.xyz);     //dp3 r0.y, r1.xyzx, r1.xyzx
    r0.y = 1.0 / sqrt(r0.y);        //rsq r0.y, r0.y
    r2.xyz = -r1.xyz * r0.y + float3(0, 0, 1);  //mad r2.xyz, -r1.xyzx, r0.yyyy, l(0.000000, 0.000000, 1.000000, 0.000000)
    r0.yzw = r1.xyz * r0.y;         //mul r0.yzw, r0.yyyy, r1.xxyz
    //r1.xyz = r2.xyz * 0.833333 + r0.yzw; //mad r1.xyz, r2.xyzx, l(0.833333, 0.833333, 0.833333, 0.000000), r0.yzwy
    //r2.xyz = worldpos - //add r2.xyz, v2.xyzx, -gViewInverse[3].xyzx

    return r0.yzw;
    //return r0.wzy;
    ////return float3(r0.w, r0.z, r0.y);

    ////return normalize(input.Normal);
}



float4 main(VS_OUTPUT input) : SV_TARGET
{
    float4 c = float4(0.1, 0.18, 0.25, 0.8);
    //if (RenderMode == 0) c = float4(1, 1, 1, 1);

    //c.a *= input.Colour0.a;

    float3 camrel = input.CamRelPos;
    float3 worldpos = camrel + CameraPos.xyz;
    if ((EnableFoamMap == 0) && (EnableFogtex == 1))
    {
        float2 fogtc = saturate((worldpos.xy - WaterFogParams.xy) * WaterFogParams.zw);
        fogtc.y = 1.0 - fogtc.y;
        c = WaterFog.Sample(TextureSS, fogtc);
        c.a = 0.9;
    }


    float3 norm = EnableFoamMap ? normalize(input.Normal) : RippleNormal(input, worldpos);//  normalize(input.Normal);

    if (RenderMode == 1) //normals
    {
        c.rgb = norm*0.5+0.5;
    }
    else if (RenderMode == 2) //tangents
    {
        c.rgb = normalize(input.Tangent.rgb)*0.5+0.5;
    }
    else if (RenderMode == 3) //colours
    {
        c.rgb = input.Colour0.rgb;
    }
    else if (RenderMode == 4) //texcoords
    {
        c.rgb = float3(input.Texcoord0, 0);
    }
    else if ((RenderMode == 8) || (EnableTexture == 1)) //single texture or diffuse enabled
    {
        c.rgb = Colourmap.Sample(TextureSS, input.Texcoord0).rgb;
    }
    else if (EnableFoamMap)
    {
        c = Foammap.Sample(TextureSS, input.Texcoord0);
    }


    float3 spec = 0;

    if (RenderMode == 0)
    {

        float4 nv = Bumpmap.Sample(TextureSS, input.Texcoord0);  //sample r1.xyzw, v2.xyxx, t3.xyzw, s3  (BumpSampler)


        float2 nmv = nv.xy;
        float4 r0 = 0, r1, r2, r3;

        float bumpiness = 0.5;

        if (EnableBumpMap)
        {
            norm = NormalMap(nmv, bumpiness, input.Normal.xyz, input.Tangent.xyz, input.Bitangent.xyz);
        }


        float3 tc = c.rgb;
        c.rgb = tc;// *r0.z; //diffuse factors...

        float3 incident = normalize(input.CamRelPos);
        float3 refl = normalize(reflect(incident, norm));
        float specb = saturate(dot(refl, GlobalLights.LightDir));
        float specp = max(exp(specb * 10) - 1, 0);
        spec += GlobalLights.LightDirColour.rgb * 0.00006 * specp * SpecularIntensity;

        if (ShaderMode == 1) //river foam
        {
            c.a *= input.Colour0.g;
        }
        else if (ShaderMode == 2) //terrain foam
        {
            c.a *= c.r;
            c.a *= input.Colour0.r;
        }
        else
        {
            ///c.a = 1;
        }
    }



    float4 fc = c;

    c.rgb = FullLighting(c.rgb, spec, norm, 0, GlobalLights, EnableShadows, input.Shadows.x, input.LightShadow);
    c.a = saturate(c.a);
    return c;
}
























/*
water_terrainfoam.fxc_PSFoam

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
//   float4 globalScalars;              // Offset:  176 Size:    16
//   float4 globalScalars2;             // Offset:  192 Size:    16
//   float4 globalScalars3;             // Offset:  208 Size:    16 [unused]
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
// cbuffer more_stuff
// {
//
//   float4 gEntitySelectColor[2];      // Offset:    0 Size:    32 [unused]
//   float4 gAmbientOcclusionEffect;    // Offset:   32 Size:    16 [unused]
//   float4 gDynamicBakesAndWetness;    // Offset:   48 Size:    16
//   float4 gAlphaRefVec0;              // Offset:   64 Size:    16 [unused]
//   float4 gAlphaRefVec1;              // Offset:   80 Size:    16 [unused]
//   float gAlphaTestRef;               // Offset:   96 Size:     4 [unused]
//   bool gTreesUseDiscard;             // Offset:  100 Size:     4 [unused]
//   float gReflectionMipCount;         // Offset:  104 Size:     4 [unused]
//   bool gUseTransparencyAA;           // Offset:  108 Size:     4 [unused]
//   bool gUseFogRay;                   // Offset:  112 Size:     4 [unused]
//
// }
//
// cbuffer water_terrainfoam_locals
// {
//
//   float WaveOffset;                  // Offset:    0 Size:     4
//   float WaterHeight;                 // Offset:    4 Size:     4
//   float WaveMovement;                // Offset:    8 Size:     4
//   float HeightOpacity;               // Offset:   12 Size:     4
//
// }
//
//
// Resource Bindings:
//
// Name                                 Type  Format         Dim      HLSL Bind  Count
// ------------------------------ ---------- ------- ----------- -------------- ------
// FoamSampler                       sampler      NA          NA             s3      1 
// WetSampler                        sampler      NA          NA             s9      1 
// WaterBumpSampler                  sampler      NA          NA            s10      1 
// FoamSampler                       texture  float4          2d             t3      1 
// WetSampler                        texture  float4          2d             t9      1 
// WaterBumpSampler                  texture  float4          2d            t10      1 
// misc_globals                      cbuffer      NA          NA            cb2      1 
// more_stuff                        cbuffer      NA          NA            cb5      1 
// water_terrainfoam_locals          cbuffer      NA          NA           cb10      1 
//
//
//
// Input signature:
//
// Name                 Index   Mask Register SysValue  Format   Used
// -------------------- ----- ------ -------- -------- ------- ------
// SV_Position              0   xyzw        0      POS   float       
// TEXCOORD                 0   xyzw        1     NONE   float     zw
// TEXCOORD                 1   xyz         2     NONE   float   xyz 
// TEXCOORD                 2   xyz         3     NONE   float       
// TEXCOORD                 3   xyz         4     NONE   float       
// TEXCOORD                 4   xyzw        5     NONE   float   xyzw
//
//
// Output signature:
//
// Name                 Index   Mask Register SysValue  Format   Used
// -------------------- ----- ------ -------- -------- ------- ------
// SV_Target                0   xyzw        0   TARGET   float   xyzw
// SV_Target                1   xyzw        1   TARGET   float   xyzw
// SV_Target                2   xyzw        2   TARGET   float   xyzw
// SV_Target                3   xyzw        3   TARGET   float   xyzw
//
ps_4_0
dcl_constantbuffer CB2[13], immediateIndexed
dcl_constantbuffer CB5[4], immediateIndexed
dcl_constantbuffer CB10[1], immediateIndexed
dcl_sampler s3, mode_default
dcl_sampler s9, mode_default
dcl_sampler s10, mode_default
dcl_resource_texture2d (float,float,float,float) t3
dcl_resource_texture2d (float,float,float,float) t9
dcl_resource_texture2d (float,float,float,float) t10
dcl_input_ps linear v1.zw
dcl_input_ps linear v2.xyz
dcl_input_ps linear v5.xyzw
dcl_output o0.xyzw
dcl_output o1.xyzw
dcl_output o2.xyzw
dcl_output o3.xyzw
dcl_temps 3
mov r0.x, l(0)
sample r1.xyzw, v5.zwzz, t9.xyzw, s9
dp2 r0.y, r1.xxxx, cb10[0].zzzz
add r0.xy, -r0.xyxx, v5.xyxx
add r0.xy, r0.xyxx, cb10[0].xxxx
add r0.zw, r0.xxxy, v5.xxxy
sample r2.xyzw, r0.xyxx, t3.xyzw, s3
sample r0.xyzw, r0.zwzz, t10.xyzw, s10
mul r0.x, r0.x, r2.y
add r0.y, v1.z, -cb10[0].y
max r0.y, r0.y, l(0.000000)
mul r0.y, r0.y, cb10[0].w
mul r0.x, r0.y, r0.x
mul r0.x, r0.x, v1.w
mul_sat r0.x, r1.x, r0.x
mul r0.x, r0.x, cb2[11].x
mov o0.w, r0.x
add r0.y, v2.z, l(-0.350000)
mul_sat r0.y, r0.y, l(1.538462)
mul r0.y, r0.y, cb5[3].z
add r0.z, -cb2[12].z, l(1.000000)
mul r0.y, r0.z, r0.y
mul r0.y, r0.y, cb2[11].z
mad o0.xyz, r0.yyyy, l(-0.500000, -0.500000, -0.500000, 0.000000), l(1.000000, 1.000000, 1.000000, 0.000000)
mul r0.yz, r0.yyyy, l(0.000000, 0.500000, 0.488281, 0.000000)
sqrt o2.xy, r0.yzyy
mov o1.w, r0.x
mov o2.w, r0.x
mad o1.xyz, v2.xyzx, l(0.500000, 0.500000, 0.500000, 0.000000), l(0.500000, 0.500000, 0.500000, 0.000000)
mov o2.z, l(0.980000)
mov o3.xyzw, l(0,0,0,0)
ret 
// Approximately 32 instruction slots used



*/









/*
water_riverfoam.fxc_PSFoam


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
//
// Resource Bindings:
//
// Name                                 Type  Format         Dim      HLSL Bind  Count
// ------------------------------ ---------- ------- ----------- -------------- ------
// FoamSampler                       sampler      NA          NA             s2      1 
// LightingSampler                   sampler      NA          NA            s15      1 
// FoamSampler                       texture  float4          2d             t2      1 
// LightingSampler                   texture  float4          2d            t15      1 
// misc_globals                      cbuffer      NA          NA            cb2      1 
//
//
//
// Input signature:
//
// Name                 Index   Mask Register SysValue  Format   Used
// -------------------- ----- ------ -------- -------- ------- ------
// SV_Position              0   xyzw        0      POS   float       
// TEXCOORD                 0   xyzw        1     NONE   float      w
// TEXCOORD                 1   xyzw        2     NONE   float   xy w
// TEXCOORD                 2   xy          3     NONE   float   xy  
// TEXCOORD                 3   xyz         4     NONE   float   xyz 
// TEXCOORD                 4   xyz         5     NONE   float   xyz 
//
//
// Output signature:
//
// Name                 Index   Mask Register SysValue  Format   Used
// -------------------- ----- ------ -------- -------- ------- ------
// SV_Target                0   xyzw        0   TARGET   float   xyzw
//
ps_4_0
dcl_constantbuffer CB2[14], immediateIndexed
dcl_sampler s2, mode_default
dcl_sampler s15, mode_default
dcl_resource_texture2d (float,float,float,float) t2
dcl_resource_texture2d (float,float,float,float) t15
dcl_input_ps linear v1.w
dcl_input_ps linear v2.xyw
dcl_input_ps linear v3.xy
dcl_input_ps linear v4.xyz
dcl_input_ps linear v5.xyz
dcl_output o0.xyzw
dcl_temps 1
div r0.xy, v2.xyxx, v2.wwww
sample r0.xyzw, r0.xyxx, t15.xyzw, s15
mad r0.xyz, v5.xyzx, r0.wwww, v4.xyzx
mul o0.xyz, r0.xyzx, cb2[13].zzzz
sample r0.xyzw, v3.xyxx, t2.xyzw, s2
mul r0.x, r0.w, v1.w
mul o0.w, r0.x, r0.x
ret 
// Approximately 8 instruction slots used



*/







/*
water_river.fxc_PS

//
// Input signature:
//
// Name                 Index   Mask Register SysValue  Format   Used
// -------------------- ----- ------ -------- -------- ------- ------
// SV_Position              0   xyzw        0      POS   float       
// TEXCOORD                 0   xyzw        1     NONE   float   xyzw
// TEXCOORD                 1   xyzw        2     NONE   float   xyzw
// TEXCOORD                 2   xyzw        3     NONE   float     zw
// TEXCOORD                 3   xyzw        4     NONE   float   xyzw
// TEXCOORD                 4   xyzw        5     NONE   float     zw
//

mul r0.xy, v5.zwzz, RippleSpeed
mad r1.xyzw, -r0.xyxy, gFlowParams.xxyy, v2.xyxy
dp2 r0.x, r0.xyxx, r0.xyxx
sqrt r0.x, r0.x
min r0.x, r0.x, l(1.000000)
mul r0.yz, r1.xxyx, RippleScale
mad r1.xy, r1.zwzz, RippleScale, l(0.500000, 0.500000, 0.000000, 0.000000)
mul r1.xy, r1.xyxx, l(2.300000, 2.300000, 0.000000, 0.000000)
mul r0.yz, r0.yyzy, l(0.000000, 2.300000, 2.300000, 0.000000)
sample r2.xyzw, r0.yzyy, WaterBumpSampler2.xyzw, s14
sample r3.xyzw, r0.yzyy, WaterBumpSampler.xyzw, s10
sample r4.xyzw, r1.xyxx, WaterBumpSampler2.xyzw, s14
sample r1.xyzw, r1.xyxx, WaterBumpSampler.xyzw, s10
mov r3.zw, r1.xxxy
mov r2.zw, r4.xxxy
add r1.xyzw, r2.xyzw, r3.xyzw
add r2.xyzw, r3.xyzw, l(0.500000, 0.500000, 0.500000, 0.500000)
add r1.xyzw, r1.xyzw, -r2.xyzw
mad r0.xyzw, r0.xxxx, r1.xyzw, r2.xyzw
mad r0.xyzw, r0.xyzw, l(2.000000, 2.000000, 2.000000, 2.000000), l(-2.000000, -2.000000, -2.000000, -2.000000)
mul r0.xyzw, r0.xyzw, gFlowParams.zzww
add r0.xy, r0.zwzz, r0.xyxx
mul r0.zw, r0.xxxy, RippleBumpiness
dp2 r0.x, r0.xyxx, r0.xyxx
sqrt r0.x, r0.x
mad r0.x, r0.x, l(0.270000), l(0.440000)
mad r1.xy, r0.zwzz, v2.wwww, v4.xyxx
mov r1.z, v4.z
dp3 r0.y, r1.xyzx, r1.xyzx
rsq r0.y, r0.y
mad r2.xyz, -r1.xyzx, r0.yyyy, l(0.000000, 0.000000, 1.000000, 0.000000)
mul r0.yzw, r0.yyyy, r1.xxyz
mad r1.xyz, r2.xyzx, l(0.833333, 0.833333, 0.833333, 0.000000), r0.yzwy
add r2.xyz, v2.xyzx, -gViewInverse[3].xyzx
dp3 r1.w, r2.xyzx, r2.xyzx
rsq r1.w, r1.w
mul r3.xyz, r1.wwww, r2.xyzx
dp3 r2.w, r3.xyzx, r1.xyzx
add r2.w, r2.w, r2.w
mad r1.xyz, r1.xyzx, -r2.wwww, r3.xyzx
dp3 r2.w, -r3.xyzx, r0.yzwy
mad r1.z, -r2.z, r1.w, -r1.z
mad r1.z, -r2.z, r1.w, |r1.z|
mul r3.xyz, r1.yyyy, gReflectionWorldViewProj[1].xwyx
mad r3.xyz, r1.xxxx, gReflectionWorldViewProj[0].xwyx, r3.xyzx
mad r1.xyz, r1.zzzz, gReflectionWorldViewProj[2].xwyx, r3.xyzx
mul r3.xyz, r1.xyzx, l(0.500000, 0.500000, 0.500000, 0.000000)
mad r4.y, r1.y, l(0.500000), -r3.z
add r4.x, r3.y, r3.x
div r1.xy, r4.xyxx, r1.yyyy
sample r3.xyzw, r1.xyxx, PlanarReflectionSampler.xyzw, s7
sample r4.xyzw, v1.zwzz, WetSampler.xyzw, s9
mul r1.x, r4.x, l(0.650000)
add r1.y, -v4.w, l(512.000000)
mul_sat r1.y, r1.y, l(0.001953)
mul r1.x, r1.y, r1.x
sample r4.xyzw, v3.zwzz, StaticFoamSampler.xyzw, s4
mul r1.y, r4.y, l(0.350000)
mad r4.x, r1.y, r0.x, r1.x
mov r4.yw, l(0,0.500000,0,0.500000)
sample r5.xyzw, r4.xyxx, BlendSampler.xyzw, s6
mov r1.xy, -r0.yzyy
mov r1.z, l(0)
mad r1.xyz, r2.xyzx, r1.wwww, r1.xyzx
mad r2.xyz, r2.xyzx, r1.wwww, gDirectionalLight.xyzx
div r6.xy, v1.xyxx, v4.wwww
sample r7.xyzw, r6.xyxx, LightingSampler.xyzw, s15
mad r1.xyz, r1.xyzx, r7.yyyy, v2.xyzx
mov r6.z, r7.y
mul r5.xzw, r1.yyyy, gRefractionWorldViewProj[1].xxwy
mad r1.xyw, r1.xxxx, gRefractionWorldViewProj[0].xwxy, r5.xzxw
mad r1.xyz, r1.zzzz, gRefractionWorldViewProj[2].xwyx, r1.xywx
add r1.xyz, r1.xyzx, gRefractionWorldViewProj[3].xwyx
mul r1.xzw, r1.xxyz, l(0.500000, 0.000000, 0.500000, 0.500000)
mad r4.y, r1.y, l(0.500000), -r1.w
add r4.x, r1.z, r1.x
div r1.xy, r4.xyxx, r1.yyyy
sample r7.xyzw, r1.xyxx, LightingSampler.xyzw, s15
ne r0.x, l(0.000000, 0.000000, 0.000000, 0.000000), r7.z
mov r1.z, r7.y
movc r1.xyz, r0.xxxx, r6.xyzx, r1.xyzx
mul r0.x, r1.z, r5.y
sample r5.xyzw, r1.xyxx, RefractionSampler.xyzw, s12
dp3 r1.x, r0.yzwy, -gDirectionalLight.xyzx
mad_sat r1.x, r1.x, l(0.700000), l(0.300000)
mul r1.xyw, r1.xxxx, gWaterDirectionalColor.xyxz
mad r1.xyw, r1.xyxw, r7.wwww, gWaterAmbientColor.xyxz
mad r1.xyw, r1.xyxw, r0.xxxx, r5.xyxz
add r3.xyz, -r1.xywx, r3.xyzx
add r0.x, -r2.w, l(1.000000)
mad r4.z, r0.x, l(0.300000), r2.w
sample r4.xyzw, r4.zwzz, BlendSampler.xyzw, s6
mad r3.xyz, r4.xxxx, r3.xyzx, r1.xywx
dp3 r0.x, r2.xyzx, r2.xyzx
rsq r0.x, r0.x
mul r2.xyz, r0.xxxx, r2.xyzx
dp3_sat r0.x, -r2.xyzx, r0.yzwy
log r0.x, r0.x
mul r0.x, r0.x, SpecularFalloff
exp r0.x, r0.x
mul r0.x, r0.x, SpecularIntensity
mul r0.x, r7.w, r0.x
mad r0.xyz, gWaterDirectionalColor.xyzx, r0.xxxx, r3.xyzx
add r0.xyz, -r1.xywx, r0.xyzx
mad r0.xyz, r1.zzzz, r0.xyzx, r1.xywx
mul o0.xyz, r0.xyzx, globalScalars3.zzzz
mov o0.w, l(0)
ret 
// Approximately 108 instruction slots used


//
// Generated by Microsoft (R) HLSL Shader Compiler 9.29.952.3111
//
//
// Buffer Definitions: 
//
// cbuffer rage_matrices
// {
//
//   row_major float4x4 gWorld;         // Offset:    0 Size:    64 [unused]
//   row_major float4x4 gWorldView;     // Offset:   64 Size:    64 [unused]
//   row_major float4x4 gWorldViewProj; // Offset:  128 Size:    64 [unused]
//   row_major float4x4 gViewInverse;   // Offset:  192 Size:    64
//
// }
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
// cbuffer lighting_globals
// {
//
//   float4 gDirectionalLight;          // Offset:    0 Size:    16
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
//   float4 globalFogParams[5];         // Offset:  800 Size:    80 [unused]
//   float4 globalFogColor;             // Offset:  880 Size:    16 [unused]
//   float4 globalFogColorE;            // Offset:  896 Size:    16 [unused]
//   float4 globalFogColorN;            // Offset:  912 Size:    16 [unused]
//   float4 globalFogColorMoon;         // Offset:  928 Size:    16 [unused]
//   float4 gReflectionTweaks;          // Offset:  944 Size:    16 [unused]
//
// }
//
// cbuffer water_globals
// {
//
//   float2 gWorldBaseVS;               // Offset:    0 Size:     8 [unused]
//   float4 gFlowParams;                // Offset:   16 Size:    16
//   float4 gFlowParams2;               // Offset:   32 Size:    16 [unused]
//   float4 gWaterAmbientColor;         // Offset:   48 Size:    16
//   float4 gWaterDirectionalColor;     // Offset:   64 Size:    16
//   float4 gScaledTime;                // Offset:   80 Size:    16 [unused]
//   float4 gOceanParams0;              // Offset:   96 Size:    16 [unused]
//   float4 gOceanParams1;              // Offset:  112 Size:    16 [unused]
//   row_major float4x4 gReflectionWorldViewProj;// Offset:  128 Size:    64
//   float4 gFogLight_Debugging;        // Offset:  192 Size:    16 [unused]
//   row_major float4x4 gRefractionWorldViewProj;// Offset:  208 Size:    64
//
// }
//
// cbuffer water_common_locals
// {
//
//   float RippleBumpiness;             // Offset:    0 Size:     4
//   float RippleSpeed;                 // Offset:    4 Size:     4
//   float RippleScale;                 // Offset:    8 Size:     4
//   float SpecularIntensity;           // Offset:   12 Size:     4
//   float SpecularFalloff;             // Offset:   16 Size:     4
//   float ParallaxIntensity;           // Offset:   20 Size:     4 [unused]
//
// }
//
//
// Resource Bindings:
//
// Name                                 Type  Format         Dim      HLSL Bind  Count
// ------------------------------ ---------- ------- ----------- -------------- ------
// StaticFoamSampler                 sampler      NA          NA             s4      1 
// BlendSampler                      sampler      NA          NA             s6      1 
// PlanarReflectionSampler           sampler      NA          NA             s7      1 
// WetSampler                        sampler      NA          NA             s9      1 
// WaterBumpSampler                  sampler      NA          NA            s10      1 
// RefractionSampler                 sampler      NA          NA            s12      1 
// WaterBumpSampler2                 sampler      NA          NA            s14      1 
// LightingSampler                   sampler      NA          NA            s15      1 
// StaticFoamSampler                 texture  float4          2d             t4      1 
// BlendSampler                      texture  float4          2d             t6      1 
// PlanarReflectionSampler           texture  float4          2d             t7      1 
// WetSampler                        texture  float4          2d             t9      1 
// WaterBumpSampler                  texture  float4          2d            t10      1 
// RefractionSampler                 texture  float4          2d            t12      1 
// WaterBumpSampler2                 texture  float4          2d            t14      1 
// LightingSampler                   texture  float4          2d            t15      1 
// rage_matrices                     cbuffer      NA          NA            cb1      1 
// misc_globals                      cbuffer      NA          NA            cb2      1 
// lighting_globals                  cbuffer      NA          NA            cb3      1 
// water_globals                     cbuffer      NA          NA            cb4      1 
// water_common_locals               cbuffer      NA          NA           cb10      1 
//
//
//
// Output signature:
//
// Name                 Index   Mask Register SysValue  Format   Used
// -------------------- ----- ------ -------- -------- ------- ------
// SV_Target                0   xyzw        0   TARGET   float   xyzw
//
ps_4_0
dcl_constantbuffer CB1[16], immediateIndexed
dcl_constantbuffer CB2[14], immediateIndexed
dcl_constantbuffer CB3[1], immediateIndexed
dcl_constantbuffer CB4[17], immediateIndexed
dcl_constantbuffer CB10[2], immediateIndexed
dcl_sampler s4, mode_default
dcl_sampler s6, mode_default
dcl_sampler s7, mode_default
dcl_sampler s9, mode_default
dcl_sampler s10, mode_default
dcl_sampler s12, mode_default
dcl_sampler s14, mode_default
dcl_sampler s15, mode_default
dcl_resource_texture2d (float,float,float,float) t4
dcl_resource_texture2d (float,float,float,float) t6
dcl_resource_texture2d (float,float,float,float) t7
dcl_resource_texture2d (float,float,float,float) t9
dcl_resource_texture2d (float,float,float,float) t10
dcl_resource_texture2d (float,float,float,float) t12
dcl_resource_texture2d (float,float,float,float) t14
dcl_resource_texture2d (float,float,float,float) t15
dcl_input_ps linear v1.xyzw
dcl_input_ps linear v2.xyzw
dcl_input_ps linear v3.zw
dcl_input_ps linear v4.xyzw
dcl_input_ps linear v5.zw
dcl_output o0.xyzw
dcl_temps 8


*/
