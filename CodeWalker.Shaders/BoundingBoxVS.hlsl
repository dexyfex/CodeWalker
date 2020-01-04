#include "Common.hlsli"
#include "Quaternion.hlsli"

cbuffer VSSceneVars : register(b0)
{
    float4x4 ViewProj;
}
cbuffer VSBoxVars : register(b1)
{
    float4 Orientation;
    float4 BBMin;
    float4 BBRng; //max-min
    float3 CamRel;
    float Pad1;
    float3 Scale;
    float Pad2;
}

float4 main(float4 pos: POSITION) : SV_POSITION
{
    float3 bpos = (BBMin.xyz + pos.xyz*BBRng.xyz) * Scale;
    float3 opos = mulvq(bpos, Orientation);
    float3 f = CamRel + opos;
    float4 cpos = mul(float4(f,1), ViewProj);
    cpos.z = DepthFunc(cpos.zw);
    return cpos;
}
