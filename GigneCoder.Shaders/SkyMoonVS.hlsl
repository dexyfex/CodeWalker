#include "Common.hlsli"

cbuffer VSMoonVars : register(b0)
{
    float4x4 ViewProj;
    float4x4 ViewInv;
    float4 CamRel;
    float2 Size;
    float2 Offset;
}


struct VS_INPUT
{
    float4 Position : POSITION;
};
struct VS_OUTPUT
{
    float4 Position : SV_POSITION;
    float2 Texcoord : TEXCOORD0;
};

VS_OUTPUT main(VS_INPUT input)
{
    VS_OUTPUT output;
    float3 ipos = input.Position.xyz * float3(Size, 1);
    float3 bpos = mul(ipos, (float3x3)ViewInv);
    float3 opos = CamRel.xyz + bpos;
    float4 pos = float4(opos, 1);
    float4 cpos = mul(pos, ViewProj);
    cpos.z = DepthFunc(cpos.zw);
    output.Position = cpos;
    output.Texcoord = input.Position.xy*0.5+0.5;
    return output;
}

