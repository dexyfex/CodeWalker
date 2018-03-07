#include "Shadowmap.hlsli"

Texture2D<float4> Colourmap : register(t0);
Texture2D<float4> Bumpmap : register(t2);
Texture2D<float4> Specmap : register(t3);
Texture2D<float4> Detailmap : register(t4);
SamplerState TextureSS : register(s0);


cbuffer PSSceneVars : register(b0)
{
    ShaderGlobalLightParams GlobalLights;
    uint EnableShadows;
    uint RenderMode;//0=default, 1=normals, 2=tangents, 3=colours, 4=texcoords, 5=diffuse, 6=normalmap, 7=spec, 8=direct
    uint RenderModeIndex;
    uint RenderSamplerCoord;
}
cbuffer PSGeomVars : register(b2)
{
    uint EnableTexture;
    uint EnableTint;
    uint EnableNormalMap;
    uint EnableSpecMap;
    uint EnableDetailMap;
    uint IsDecal;
    uint IsEmissive;
    uint IsDistMap;
    float bumpiness;
    float AlphaScale;
    float HardAlphaBlend;
    float useTessellation;
    float4 detailSettings;
    float3 specMapIntMask;
    float specularIntensityMult;
    float specularFalloffMult;
    float specularFresnel;
    float wetnessMultiplier;
    uint SpecOnly;
	float4 TextureAlphaMask;
}


struct VS_OUTPUT
{
    float4 Position  : SV_POSITION;
    float3 Normal    : NORMAL;
    float2 Texcoord0 : TEXCOORD0;
    float2 Texcoord1 : TEXCOORD1;
    float2 Texcoord2 : TEXCOORD2;
    float4 Shadows   : TEXCOORD3;
    float4 LightShadow : TEXCOORD4;
    float4 Colour0   : COLOR0;
    float4 Colour1   : COLOR1;
    float4 Tint      : COLOR2;
    float4 Tangent   : TEXCOORD5;
    float4 Bitangent : TEXCOORD6;
    float3 CamRelPos : TEXCOORD7;
};



float4 main(VS_OUTPUT input) : SV_TARGET
{
    float4 c = float4(0.5, 0.5, 0.5, 1);
    if (RenderMode == 0) c = float4(1, 1, 1, 1);
    if (EnableTexture == 1)
    {
        float2 texc = input.Texcoord0;
        if (RenderMode >= 5)
        {
            if (RenderSamplerCoord == 2) texc = input.Texcoord1;
            else if (RenderSamplerCoord == 3) texc = input.Texcoord2;
        }

        c = Colourmap.Sample(TextureSS, texc);

        if (IsDistMap) c = float4(c.rgb*2, (c.r+c.g+c.b) - 1);
        if ((IsDecal == 0) && (c.a <= 0.33)) discard;
        if ((IsDecal == 1) && (c.a <= 0.0)) discard;
        if(IsDecal==0) c.a = 1;
		if (IsDecal == 2)
		{
			float4 mask = TextureAlphaMask * c;
			c.a = saturate(mask.r + mask.g + mask.b + mask.a);
			c.rgb = 0;
		}
        c.a = saturate(c.a*AlphaScale);
    }
    if (EnableTint > 0)
    {
        c.rgb *= input.Tint.rgb;
    }
    if (IsDecal == 1)
    {
        c.a *= input.Colour0.a;
    }

    float3 norm = normalize(input.Normal);

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
        if (RenderModeIndex == 2) c.rgb = input.Colour1.rgb;
    }
    else if (RenderMode == 4) //texcoords
    {
        c.rgb = float3(input.Texcoord0, 0);
        if (RenderModeIndex == 2) c.rgb = float3(input.Texcoord1, 0);
        if (RenderModeIndex == 3) c.rgb = float3(input.Texcoord2, 0);
    }


    float3 spec = 0;

    if (RenderMode == 0)
    {

        float4 nv = Bumpmap.Sample(TextureSS, input.Texcoord0);  //sample r1.xyzw, v2.xyxx, t3.xyzw, s3  (BumpSampler)
        float4 sv = Specmap.Sample(TextureSS, input.Texcoord0);  //sample r2.xyzw, v2.xyxx, t4.xyzw, s4  (SpecSampler)


        float2 nmv = nv.xy;
        float4 r0 = 0, r1, r2, r3;

        if (EnableNormalMap)
        {
            if (EnableDetailMap)
            {
                //detail normalmapp
                r0.xy = input.Texcoord0 * detailSettings.zw;    //mul r0.xy, v2.xyxx, detailSettings.zwzz
                r0.zw = r0.xy * 3.17;       //mul r0.zw, r0.xxxy, l(0.000000, 0.000000, 3.170000, 3.170000)
                r0.xy = Detailmap.Sample(TextureSS, r0.xy).xy - 0.5;    //sample r1.xyzw, r0.xyxx, t2.xyzw, s2  (DetailSampler)    //mad r0.xy, r1.xyxx, l(2.000000, 2.000000, 0.000000, 0.000000), l(-1.000000, -1.000000, 0.000000, 0.000000)
                r0.zw = Detailmap.Sample(TextureSS, r0.zw).xy - 0.5;    //sample r1.xyzw, r0.zwzz, t2.xyzw, s2  (DetailSampler)    //mad r0.zw, r1.xxxy, l(0.000000, 0.000000, 2.000000, 2.000000), l(0.000000, 0.000000, -1.000000, -1.000000) //r0.zw = r0.zw*0.5;          //mul r0.zw, r0.zzzw, l(0.000000, 0.000000, 0.500000, 0.500000)
                r0.xy = r0.xy + r0.zw;  //mad r0.xy, r0.xyxx, l(0.500000, 0.500000, 0.000000, 0.000000), r0.zwzz
                r0.yz = r0.xy * detailSettings.y; //mul r0.yz, r0.xxyx, detailSettings.yyyy
                //r0.x = -r0.x*detailSettings.x;  //mul r0.x, -r0.x, detailSettings.x
                nmv = r0.yz*sv.w + nv.xy; //mad r0.yz, r0.yyzy, r2.wwww, r1.xxyx //add detail to normal, using specmap(!)
            }

            norm = NormalMap(nmv, bumpiness, input.Normal.xyz, input.Tangent.xyz, input.Bitangent.xyz);


        }
        


        if (EnableSpecMap == 0)
        {
            sv = float4(0.1,0.1,0.1,0.1);
        }

        float r1y = norm.z - 0.35;    ////r1.y = r0.w*r1.x - 0.35;    //mad r1.y, r0.w, r1.x, l(-0.350000)

        float3 globalScalars = float3(0.5, 0.5, 0.5);
        float globalScalars2z = 1;// 0.65; //wet darkness?
        float wetness = 0;// 10.0;

        r0.x = 0;// .5;

        r0.z = 1 - globalScalars2z;     //add r0.z, -globalScalars2.z, l(1.000000)
        r0.y = saturate(r1y*1.538462);  //mul_sat r0.y, r1.y, l(1.538462)
        r0.y = r0.y * wetness;          //mul r0.y, r0.y, gDynamicBakesAndWetness.z
        r0.y = r0.y * r0.z;             //mul r0.y, r0.z, r0.y
        r1.yz = input.Colour0.xy * globalScalars.zy; //mul r1.yz, v1.xxyx, globalScalars.zzyz     //vertex.colour0
        r0.y = r0.y * r1.y;             //mul r0.y, r0.y, r1.y
        r0.x = r0.x * sv.w + 1.0;       //mad r0.x, r0.x, r2.w, l(1.000000)
        sv.xy = sv.xy*sv.xy;            //mul r2.xy, r2.xyxx, r2.xyxx
        r0.z = sv.w * specularFalloffMult;  //mul r0.z, r2.w, specularFalloffMult
        r3.y = r0.z * 0.001953125;          //mul r3.y, r0.z, l(0.001953)  (1/512)
        r0.z = dot(sv.xyz, specMapIntMask); //dp3 r0.z, r2.xyzx, specMapIntMask.xyzx
        r0.z = r0.z*specularIntensityMult;  //mul r0.z, r0.z, specularIntensityMult
        r3.x = r0.x * r0.z;                 //mul r3.x, r0.x, r0.z
        r0.z = saturate(r0.z*r0.x + 0.4);   //mad_sat r0.z, r0.z, r0.x, l(0.400000)
        //sv.xy = r0.z*float2(0.5, 0.488281); //mul r2.xy, r0.zzzz, l(0.500000, 0.488281, 0.000000, 0.000000)
        r0.z = 1 - r3.x*0.5;    //mad r0.z, -r3.x, l(0.500000), l(1.000000)
        r0.z = r0.z * r0.y;     //mul r0.z, r0.z, r0.y
        r0.y = r0.y * wetnessMultiplier;    //mul r0.y, r0.y, wetnessMultiplier
        r0.z = 1 - r0.z*0.5;    //mad r0.z, r0.z, l(-0.500000), l(1.000000)

        float3 tc = c.rgb * r0.x;
        c.rgb = tc * r0.z; //diffuse factors...

        float3 incident = normalize(input.CamRelPos);
        float3 refl = normalize(reflect(incident, norm));
        float specb = saturate(dot(refl, GlobalLights.LightDir));
        float specp = max(exp(specb * 10) - 1, 0);
        spec += GlobalLights.LightDirColour.rgb * 0.00006 * specp * r0.z * sv.x * specularIntensityMult;// ((specularIntensityMult != 0) ? 1 : 0);

        if (SpecOnly == 1)
        {
            c.a *= (EnableSpecMap == 0) ? nv.a : saturate(specp);
        }

    }


    float4 fc = c;

    c.rgb = FullLighting(c.rgb, spec, norm, input.Colour0, GlobalLights, EnableShadows, input.Shadows.x, input.LightShadow);


    if (IsEmissive==1)
    {
        c.rgb += fc.rgb;
    }
    else
    {
    }

    //c.rgb = max(c.rgb, 0);
    c.a = saturate(c.a);
    return c;
}





//normal_spec_detail.fxc_PS_DeferredTextured

//mul r0.xy, v2.xyxx, detailSettings.zwzz
//mul r0.zw, r0.xxxy, l(0.000000, 0.000000, 3.170000, 3.170000)
//sample r1.xyzw, r0.xyxx, t2.xyzw, s2  (DetailSampler)
//mad r0.xy, r1.xyxx, l(2.000000, 2.000000, 0.000000, 0.000000), l(-1.000000, -1.000000, 0.000000, 0.000000)
//sample r1.xyzw, r0.zwzz, t2.xyzw, s2  (DetailSampler)
//mad r0.zw, r1.xxxy, l(0.000000, 0.000000, 2.000000, 2.000000), l(0.000000, 0.000000, -1.000000, -1.000000)
//mul r0.zw, r0.zzzw, l(0.000000, 0.000000, 0.500000, 0.500000)
//mad r0.xy, r0.xyxx, l(0.500000, 0.500000, 0.000000, 0.000000), r0.zwzz
//mul r0.yz, r0.xxyx, detailSettings.yyyy
//mul r0.x, -r0.x, detailSettings.x
//sample r1.xyzw, v2.xyxx, t3.xyzw, s3  (BumpSampler)
//sample r2.xyzw, v2.xyxx, t4.xyzw, s4  (SpecSampler)
//mad r0.yz, r0.yyzy, r2.wwww, r1.xxyx
//mad r0.yz, r0.yyzy, l(0.000000, 2.000000, 2.000000, 0.000000), l(0.000000, -1.000000, -1.000000, 0.000000)
//max r0.w, bumpiness, l(0.001000)
//mul r1.xy, r0.wwww, r0.yzyy
//dp2 r0.y, r0.yzyy, r0.yzyy
//add r0.y, -r0.y, l(1.000000)
//sqrt r0.y, |r0.y|
//mul r1.yzw, r1.yyyy, v5.xxyz
//mad r1.xyz, r1.xxxx, v4.xyzx, r1.yzwy
//mad r0.yzw, r0.yyyy, v3.xxyz, r1.xxyz
//dp3 r1.x, r0.yzwy, r0.yzwy
//rsq r1.x, r1.x
//mad r1.y, r0.w, r1.x, l(-0.350000)
//mul r0.yzw, r0.yyzw, r1.xxxx
//mad o1.xyz, r0.yzwy, l(0.500000, 0.500000, 0.500000, 0.000000), l(0.500000, 0.500000, 0.500000, 0.000000)

//add r0.z, -globalScalars2.z, l(1.000000)
//mul_sat r0.y, r1.y, l(1.538462)
//mul r0.y, r0.y, gDynamicBakesAndWetness.z
//mul r0.y, r0.z, r0.y
//mul r1.yz, v1.xxyx, globalScalars.zzyz
//mul r0.y, r0.y, r1.y
//mad r0.x, r0.x, r2.w, l(1.000000)
//mul r2.xy, r2.xyxx, r2.xyxx
//mul r0.z, r2.w, specularFalloffMult
//mul r3.y, r0.z, l(0.001953)
//dp3 r0.z, r2.xyzx, specMapIntMask.xyzx
//mul r0.z, r0.z, specularIntensityMult
//mul r3.x, r0.x, r0.z
//mad_sat r0.z, r0.z, r0.x, l(0.400000)
//mul r2.xy, r0.zzzz, l(0.500000, 0.488281, 0.000000, 0.000000)
//mad r0.z, -r3.x, l(0.500000), l(1.000000)
//mul r0.z, r0.z, r0.y
//mul r0.y, r0.y, wetnessMultiplier
//mad r0.z, r0.z, l(-0.500000), l(1.000000)

//sample r4.xyzw, v2.xyxx, t0.xyzw, s0  //DiffuseSampler
//mul r4.xyzw, r0.xxxx, r4.xyzw
//mul o0.xyz, r0.zzzz, r4.xyzx
//mul r0.x, r4.w, v1.w
//mul o0.w, r0.x, globalScalars.x

//add r0.x, wetnessMultiplier, l(-0.200000)
//mul_sat r0.x, r0.x, l(10.000000)
//mad_sat r0.z, v3.z, l(128.000000), l(-127.000000)
//mul o1.w, r0.x, r0.z
//mov r2.z, l(0.970000)
//mov r3.z, specularFresnel
//add r0.xzw, r2.xxyz, -r3.xxyz
//max r0.xzw, r0.xxzw, l(0.000000, 0.000000, 0.000000, 0.000000)
//mad r0.xyz, r0.xzwx, r0.yyyy, r3.xyzx
//sqrt o2.xy, r0.xyxx
//mov o2.z, r0.z
//mov o2.w, l(1.000000)

//mad r1.x, v1.x, globalScalars.z, gLightArtificialIntAmbient0.w
//mul r0.xy, r1.xzxx, l(0.500000, 0.500000, 0.000000, 0.000000)
//sqrt o3.xy, r0.xyxx
//mov o3.zw, l(0,0,0,1.001884)



















/*
//normal_spec.fxc_PS_DeferredTextured
//

max r0.x, bumpiness, l(0.001000)            //cb12[1].w
sample r1.xyzw, v2.xyxx, BumpSampler.xyzw, s2
mad r0.yz, r1.xxyx, l(0.000000, 2.000000, 2.000000, 0.000000), l(0.000000, -1.000000, -1.000000, 0.000000)
mul r0.xw, r0.xxxx, r0.yyyz
dp2 r0.y, r0.yzyy, r0.yzyy
add r0.y, -r0.y, l(1.000000)
sqrt r0.y, |r0.y|
mul r1.xyz, r0.wwww, v5.xyzx
mad r0.xzw, r0.xxxx, v4.xxyz, r1.xxyz
mad r0.xyz, r0.yyyy, v3.xyzx, r0.xzwx
dp3 r0.w, r0.xyzx, r0.xyzx
rsq r0.w, r0.w
mad r1.x, r0.z, r0.w, l(-0.350000)
mul r0.xyz, r0.wwww, r0.xyzx
mad o1.xyz, r0.xyzx, l(0.500000, 0.500000, 0.500000, 0.000000), l(0.500000, 0.500000, 0.500000, 0.000000)

mul_sat r0.x, r1.x, l(1.538462)
mul r0.x, r0.x, gDynamicBakesAndWetness.z   //cb5[3].z
add r0.y, -globalScalars2.z, l(1.000000)    //cb2[12].z
mul r0.x, r0.y, r0.x
mul r1.yz, v1.xxyx, globalScalars.zzyz      //vertex.colour0, cb2[11].zzyz
mul r0.x, r0.x, r1.y

sample r2.xyzw, v2.xyxx, SpecSampler.xyzw, s3
mul r2.xy, r2.xyxx, r2.xyxx
mul r0.y, r2.w, specularFalloffMult         //cb12[0].y
mul r3.y, r0.y, l(0.001953)
dp3 r0.y, r2.xyzx, specMapIntMask.xyzx      //cb12[1].xyzx
mul r3.x, r0.y, specularIntensityMult       //cb12[0].z
mad_sat r0.y, r0.y, specularIntensityMult, l(0.400000)   //cb12[0].z
mul r2.xy, r0.yyyy, l(0.500000, 0.488281, 0.000000, 0.000000)
mad r0.y, -r3.x, l(0.500000), l(1.000000)
mul r0.y, r0.y, r0.x
mul r0.x, r0.x, wetnessMultiplier;          //cb12[2].x
mad r0.y, r0.y, l(-0.500000), l(1.000000)
sample r4.xyzw, v2.xyxx, DiffuseSampler.xyzw, s0
mul o0.xyz, r0.yyyy, r4.xyzx
mul r0.y, r4.w, v1.w                        //vertex.colour0
mul o0.w, r0.y, globalScalars.x             //cb2[11].x

add r0.y, wetnessMultiplier, l(-0.200000)   //cb12[2].x
mul_sat r0.y, r0.y, l(10.000000)
mad_sat r0.z, v3.z, l(128.000000), l(-127.000000)
mul o1.w, r0.y, r0.z

mov r2.z, l(0.970000)
mov r3.z, specularFresnel                   //cb12[0].x
add r0.yzw, r2.xxyz, -r3.xxyz
max r0.yzw, r0.yyzw, l(0.000000, 0.000000, 0.000000, 0.000000)
mad r0.xyz, r0.yzwy, r0.xxxx, r3.xyzx
sqrt o2.xy, r0.xyxx
mov o2.z, r0.z
mov o2.w, l(1.000000)

mad r1.x, v1.x, globalScalars.z, gLightArtificialIntAmbient0.w    //vertex.colour0, cb2[11].z, cb3[45].w
mul r0.xy, r1.xzxx, l(0.500000, 0.500000, 0.000000, 0.000000)
sqrt o3.xy, r0.xyxx
mov o3.zw, l(0,0,0,1.001884)

ret 
// Approximately 54 instruction slots used

//
// Generated by Microsoft (R) HLSL Shader Compiler 9.29.952.3111
//
//
// Resource Bindings:
//
// Name                                 Type  Format         Dim      HLSL Bind  Count
// ------------------------------ ---------- ------- ----------- -------------- ------
// DiffuseSampler                    sampler      NA          NA             s0      1 
// BumpSampler                       sampler      NA          NA             s2      1 
// SpecSampler                       sampler      NA          NA             s3      1 
// DiffuseSampler                    texture  float4          2d             t0      1 
// BumpSampler                       texture  float4          2d             t2      1 
// SpecSampler                       texture  float4          2d             t3      1 
// misc_globals                      cbuffer      NA          NA            cb2      1 
// lighting_globals                  cbuffer      NA          NA            cb3      1 
// more_stuff                        cbuffer      NA          NA            cb5      1 
// megashader_locals                 cbuffer      NA          NA           cb12      1 
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
//   float4 gLightArtificialIntAmbient0;// Offset:  720 Size:    16
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
// cbuffer megashader_locals
// {
//
//   float specularFresnel;             // Offset:    0 Size:     4
//   float specularFalloffMult;         // Offset:    4 Size:     4
//   float specularIntensityMult;       // Offset:    8 Size:     4
//   float3 specMapIntMask;             // Offset:   16 Size:    12
//   float bumpiness;                   // Offset:   28 Size:     4
//   float wetnessMultiplier;           // Offset:   32 Size:     4
//   float useTessellation;             // Offset:   36 Size:     4 [unused]
//   float HardAlphaBlend;              // Offset:   40 Size:     4 [unused]
//
// }
//
//
//
// Input signature:
//
// Name                 Index   Mask Register SysValue  Format   Used
// -------------------- ----- ------ -------- -------- ------- ------
// SV_Position              0   xyzw        0      POS   float       
// COLOR                    0   xyzw        1     NONE   float   xy w
// TEXCOORD                 0   xy          2     NONE   float   xy  
// TEXCOORD                 1   xyz         3     NONE   float   xyz 
// TEXCOORD                 4   xyz         4     NONE   float   xyz 
// TEXCOORD                 5   xyz         5     NONE   float   xyz 
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
dcl_constantbuffer CB3[46], immediateIndexed
dcl_constantbuffer CB5[4], immediateIndexed
dcl_constantbuffer CB12[3], immediateIndexed
dcl_sampler s0, mode_default
dcl_sampler s2, mode_default
dcl_sampler s3, mode_default
dcl_resource_texture2d (float,float,float,float) t0
dcl_resource_texture2d (float,float,float,float) t2
dcl_resource_texture2d (float,float,float,float) t3
dcl_input_ps linear v1.xyw
dcl_input_ps linear v2.xy
dcl_input_ps linear v3.xyz
dcl_input_ps linear v4.xyz
dcl_input_ps linear v5.xyz
dcl_output o0.xyzw
dcl_output o1.xyzw
dcl_output o2.xyzw
dcl_output o3.xyzw
dcl_temps 5

*/

