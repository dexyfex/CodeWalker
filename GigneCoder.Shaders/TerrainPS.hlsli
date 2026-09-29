#include "Shadowmap.hlsli"

Texture2D<float4> Colourmap0 : register(t0);
Texture2D<float4> Colourmap1 : register(t2);
Texture2D<float4> Colourmap2 : register(t3);
Texture2D<float4> Colourmap3 : register(t4);
Texture2D<float4> Colourmap4 : register(t5);
Texture2D<float4> Colourmask : register(t6);
Texture2D<float4> Normalmap0 : register(t7);
Texture2D<float4> Normalmap1 : register(t8);
Texture2D<float4> Normalmap2 : register(t9);
Texture2D<float4> Normalmap3 : register(t10);
Texture2D<float4> Normalmap4 : register(t11);
SamplerState TextureSS : register(s0);


cbuffer PSSceneVars : register(b0)
{
    ShaderGlobalLightParams GlobalLights;
    uint EnableShadows;
    uint RenderMode; //0=default, 1=normals, 2=tangents, 3=colours, 4=texcoords, 5=diffuse, 6=normalmap, 7=spec, 8=direct
    uint RenderModeIndex; //colour or texcoord index
    uint RenderSamplerCoord;
}
cbuffer PSEntityVars : register(b2)
{
    uint EnableTexture0;
    uint EnableTexture1;
    uint EnableTexture2;
    uint EnableTexture3;
    uint EnableTexture4;
    uint EnableTextureMask;
    uint EnableNormalMap;
    uint ShaderName;
    uint EnableTint;
    uint EnableVertexColour;
    float bumpiness;
    uint Pad102;
}


struct VS_OUTPUT
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float4 Colour0 : COLOR0;
    float4 Colour1 : COLOR1;
    float4 Tint : COLOR2;
    float2 Texcoord0 : TEXCOORD0;
    float2 Texcoord1 : TEXCOORD1;
    float2 Texcoord2 : TEXCOORD2;
    float4 Shadows : TEXCOORD4;
    float4 LightShadow : TEXCOORD5;
    float4 Tangent : TEXCOORD6;
    float4 Bitangent : TEXCOORD7;
    float3 CamRelPos : TEXCOORD8;
};

struct PS_OUTPUT
{
    float4 Diffuse : SV_Target0;
    float4 Normal : SV_Target1;
    float4 Specular : SV_Target2;
    float4 Irradiance : SV_Target3;
};


