#include "Common.hlsli"

cbuffer VSSceneVars : register(b0)
{
    float4x4 ViewProj;
    float4x4 ViewInv;
    float SegmentCount;
    float VertexCount;
    float Pad1;
    float Pad2;
}
cbuffer VSSphereVars : register(b1)
{
    float3 Center;
    float Radius;
}


float4 main(uint id : SV_VertexID) : SV_POSITION
{
    static const float twopi = 6.283185307179586476925286766559;
    uint seg = id;
    float t = twopi*((float)seg)/SegmentCount;
    float ct = cos(t);
    float st = sin(t);
    float r = Radius;
    float3 o = float3(ct*r, st*r, 0);
    float3 f = Center.xyz + mul(o, (float3x3)ViewInv);
    float4 cpos = mul(float4(f,1), ViewProj);
    cpos.z = DepthFunc(cpos.zw);
    return cpos;
}
