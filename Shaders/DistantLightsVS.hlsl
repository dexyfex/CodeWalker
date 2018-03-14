#include "Common.hlsli"


struct VS_INPUT
{
    float4 Position : POSITION;
    float2 Texcoord : TEXCOORD0;
};
struct VS_OUTPUT
{
    float4 Position : SV_POSITION;
    float2 Texcoord : TEXCOORD0;
    float4 Colour   : COLOR0;
};

struct DistLODLight
{
    float3 Position;
    uint Colour;
};
StructuredBuffer<DistLODLight> LightInstances : register(t0);

cbuffer VSSceneVars : register(b0)
{
    float4x4 ViewProj;
    float4x4 ViewInv;
    float3 CamPos;
    float Pad0;
};


VS_OUTPUT main(VS_INPUT input, uint iid : SV_InstanceID)
{
    VS_OUTPUT output;

    DistLODLight light = LightInstances[iid];
    float4 ipos = float4(light.Position - CamPos, 1.0);
    float4 vpos = float4(input.Position.xy, 0.0, 0.0);// *20.0f;

    float4 rgbi = Unpack4x8UNF(light.Colour).gbar;

    float dist = length(ipos.xyz);
    float size = rgbi.a * min(dist, 50);

    float3 offs = vpos.xyz * min(size*0.1f, 3.0f);
    float3 tpos = mul(offs, (float3x3)ViewInv);
    ipos.xyz += tpos;

    float4 opos = mul(ipos, ViewProj);
    //opos.xy += offs * opos.w;

    output.Position = opos;// +vpos;
    output.Texcoord = input.Texcoord;
    output.Colour = float4(rgbi.rgb, 0.25);

	return output;
}