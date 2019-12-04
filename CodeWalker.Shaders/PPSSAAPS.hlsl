
struct VS_Output
{
    float4 Pos : SV_POSITION;
    float2 Tex : TEXCOORD0;
};


Texture2D<float4> SceneColour : register(t0);
SamplerState PointSampler : register(s0);


cbuffer cbPS : register(b0)
{
    uint SampleCount;
    float SampleMult;
    float TexelSizeX;
    float TexelSizeY;
};

float4 main(VS_Output input) : SV_TARGET
{
    float4 vColor = 0;
    
    float2 ts = float2(TexelSizeX, TexelSizeY);
    float2 tc = input.Tex * (1.0 - (ts * (float) (SampleCount - 1)));
    
    for (uint x = 0; x < SampleCount; x++)
    {
        for (uint y = 0; y < SampleCount; y++)
        {
            float2 tcxy = tc + float2(x, y) * ts;
            vColor += SceneColour.Sample(PointSampler, tcxy);
        }
    }
    
    vColor *= SampleMult;
    
    return float4(vColor.rgb, 1);
}

