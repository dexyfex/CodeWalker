


cbuffer VSSceneVars : register(b0)
{
    float4x4 ViewProj;
    float4 CameraPos;
    float4 LightColour;
}

StructuredBuffer<float4> Nodes : register(t0);


struct VS_INPUT
{
    float4 Position  : POSITION;
    float4 Normal    : NORMAL;
};

struct VS_OUTPUT
{
    float4 Position  : SV_POSITION;
    float4 Colour    : COLOR0;
    float3 Normal    : NORMAL;
};



VS_OUTPUT main(VS_INPUT input, uint iid : SV_InstanceID)
{
    VS_OUTPUT output;

    float4 n = Nodes[iid];
    float3 npos = n.xyz;
    float3 ipos = input.Position.xyz * 0.25f;
    float3 opos = ipos + npos - CameraPos.xyz;
    float4 cpos = mul(float4(opos, 1), ViewProj);

    output.Position = cpos;
    output.Colour = ((float4)1) * LightColour.a; //apply intensity
    output.Normal = input.Normal.xyz;

    return output;
}

