
cbuffer PSMoonVars : register(b0)
{
    float4 Colour;
}

Texture2D<float4> MoonSampler : register(t0);
SamplerState TextureSS : register(s0);

struct VS_OUTPUT
{
    float4 Position : SV_POSITION;
    float2 Texcoord : TEXCOORD0;
};


float4 main(VS_OUTPUT input) : SV_TARGET
{
    float2 texc = input.Texcoord;
    float4 m = MoonSampler.Sample(TextureSS, texc);

    return float4(Colour.rgb*m.rgb, m.a);
}

