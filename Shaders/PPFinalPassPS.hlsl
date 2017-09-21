
struct VS_Output
{
    float4 Pos : SV_POSITION;              
    float2 Tex : TEXCOORD0;
};


Texture2D<float4> tex : register( t0 );
StructuredBuffer<float> lum : register( t1 );
Texture2D<float4> bloom : register( t2 );

SamplerState PointSampler : register (s0);
SamplerState LinearSampler : register (s1);


static const float  MIDDLE_GRAY = 0.72f;
static const float  LUM_WHITE = 1.5f;

cbuffer cbPS : register( b0 )
{
    float4    g_param;   
};


float4 main(VS_Output input) : SV_TARGET
{
    float4 vColor = tex.Sample(PointSampler, input.Tex);
    float fLum = min(max(lum[0]*g_param.x, 0.2), 10); //limit amplification...
    float3 vBloom = bloom.Sample(LinearSampler, input.Tex).rgb;

    // Tone mapping
    vColor.rgb *= MIDDLE_GRAY / (fLum + 0.001f);
    vColor.rgb *= (1.0f + vColor.rgb/LUM_WHITE);
    vColor.rgb /= (1.0f + vColor.rgb);

    vColor.rgb += 0.6f * vBloom;
    vColor.a = 1.0f;

    return vColor;
}