#include "Shadowmap.hlsli"

Texture2D<float4> Colourmap : register(t0);
Texture2D<float4> Bumpmap : register(t2);
Texture2D<float4> Specmap : register(t3);
Texture2D<float4> Detailmap : register(t4);
Texture2D<float4> Colourmap2 : register(t5);
Texture2D<float4> TintPalette : register(t6);
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
    uint EnableTexture;//1+=diffuse1, 2+=diffuse2
    uint EnableTint;//1=default, 2=weapons (use diffuse.a for tint lookup)
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

struct PS_OUTPUT
{
    float4 Diffuse : SV_Target0;
    float4 Normal : SV_Target1;
    float4 Specular : SV_Target2;
    float4 Irradiance : SV_Target3;
};



