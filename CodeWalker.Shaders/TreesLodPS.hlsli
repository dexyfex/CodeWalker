#include "Common.hlsli"

Texture2D<float4> Colourmap : register(t0);
SamplerState TextureSS : register(s0);


cbuffer PSSceneVars : register(b0)
{
    ShaderGlobalLightParams GlobalLights;
}
cbuffer PSEntityVars : register(b1)
{
    uint EnableTexture;
    uint Pad1;
    uint Pad2;
    uint Pad3;
}


struct VS_OUTPUT
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float2 Texcoord : TEXCOORD0;
    float4 Colour : COLOR0;
};

struct PS_OUTPUT
{
    float4 Diffuse : SV_Target0;
    float4 Normal : SV_Target1;
    float4 Specular : SV_Target2;
    float4 Irradiance : SV_Target3;
};

