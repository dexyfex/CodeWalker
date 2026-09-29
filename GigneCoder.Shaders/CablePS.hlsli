#include "Shadowmap.hlsli"

Texture2D<float4> Colourmap : register(t0);
SamplerState TextureSS : register(s0);


cbuffer PSSceneVars : register(b0)
{
    ShaderGlobalLightParams GlobalLights;
    uint EnableShadows;
    uint RenderMode; //0=default, 1=normals, 2=tangents, 3=colours, 4=texcoords, 5=diffuse, 6=normalmap, 7=spec, 8=direct
    uint RenderModeIndex;
    uint RenderSamplerCoord;
}
cbuffer PSGeomVars : register(b2)
{
    uint EnableTexture;
    uint EnableTint;
    uint Pad100;
    uint Pad101;
}


struct VS_OUTPUT
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float2 Texcoord0 : TEXCOORD0;
    float2 Texcoord1 : TEXCOORD1;
    float2 Texcoord2 : TEXCOORD2;
    float4 Shadows : TEXCOORD3;
    float4 LightShadow : TEXCOORD4;
    float4 Colour0 : COLOR0;
    float4 Colour1 : COLOR1;
    float4 Tint : COLOR2;
    float4 Tangent : TANGENT;
};

struct PS_OUTPUT
{
    float4 Diffuse : SV_Target0;
    float4 Normal : SV_Target1;
    float4 Specular : SV_Target2;
    float4 Irradiance : SV_Target3;
};

