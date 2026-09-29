
struct VS_OUTPUT
{
    float4 Position  : SV_POSITION;
    float4 Colour : COLOR0;
    float CullValue : TEXCOORD0;
};


float4 main(VS_OUTPUT input) : SV_TARGET
{
    if (input.CullValue < -0.18) discard;
	return input.Colour;
}
