
cbuffer PSSunVars : register(b0)
{
    float4 Colour;
}


float4 main() : SV_TARGET
{
	return Colour;
}