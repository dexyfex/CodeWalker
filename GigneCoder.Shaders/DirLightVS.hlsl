#include "Common.hlsli"


struct VS_Output
{
    float4 Pos : SV_POSITION;
    float4 Screen : TEXCOORD0;
};

cbuffer VSLightVars : register(b0)
{
    float4x4 ViewProj;
    float4 CameraPos;
    uint LightType; //0=directional, 1=Point, 2=Spot, 4=Capsule
    uint IsLOD; //useful or not?
    uint Pad0;
    uint Pad1;
}


VS_Output main(float4 ipos : POSITION)
{
    float3 opos = ipos.xyz;
    float4 spos = mul(float4(opos, 1), ViewProj);
    VS_Output output;
    output.Pos = spos;
    output.Screen = spos;
    return output;
}

