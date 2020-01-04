
cbuffer VSSceneVars : register(b0)
{
    float4x4 ViewProj;
    float4 CameraPos;
    float4 LightColour;
}

struct VS_INPUT
{
    float4 Position  : POSITION;
    float4 Colour    : COLOR0;
};

struct VS_OUTPUT
{
    float4 Position  : SV_POSITION;
    float4 Colour    : COLOR0;
};


VS_OUTPUT main(VS_INPUT input)
{
    VS_OUTPUT output;

	float3 pos = input.Position.xyz;
	float4 col = input.Colour;

	float3 opos = pos - CameraPos.xyz;
	float4 cpos = mul(float4(opos, 1), ViewProj);
    cpos.z *= 1.01; //bias paths depth slightly to bring it in front of normal geometry...
    output.Position = cpos;
	output.Colour.rgb = col.rgb * LightColour.a; //apply intensity
	output.Colour.a = col.a;

	return output;
}

