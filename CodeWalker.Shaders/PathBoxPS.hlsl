
struct VS_OUTPUT
{
    float4 Position  : SV_POSITION;
    float4 Colour    : COLOR0;
    float3 Normal    : NORMAL;
};


float4 main(VS_OUTPUT input) : SV_TARGET
{
    float4 c = input.Colour;

    return float4(c.rgb, 1.0f);
}


