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
    float3 Normal   : NORMAL;
    float2 Texcoord : TEXCOORD0;
    float4 Colour   : COLOR0;
};



float4 main(VS_OUTPUT input) : SV_TARGET
{
    //return float4(1,0,0,1);//red

    float4 c = 0;// float4(input.Colour.rgb, 1);
    //return c;

    if (EnableTexture == 1)
    {
        //c = Colourmap.SampleLevel(TextureSS, input.Texcoord, 0);
        c = Colourmap.Sample(TextureSS, input.Texcoord);
        if (c.a <= 0.25) discard;
        c.a = 1;
           // c = float4(input.Colour.rgb, 1);
    }

    float3 norm = input.Normal;
    float lf = saturate(dot(normalize(norm), GlobalLights.LightDir.xyz));

    c.rgb = GlobalLighting(c.rgb, norm, input.Colour, lf, GlobalLights);
    c.a = saturate(c.a);

    return c;
}