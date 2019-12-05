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

cbuffer VSLightInstVars : register(b1)
{
    float3 InstPosition;//camera relative
    float InstIntensity;
    float3 InstColour;
    float InstFalloff;
    float3 InstDirection;
    float InstFalloffExponent;
    float3 InstTangentX;
    float InstConeInnerAngle;
    float3 InstTangentY;
    float InstConeOuterAngle;
    float3 InstCapsuleExtent;
    uint InstType;
    float3 InstCullingPlaneNormal;
    float InstCullingPlaneOffset;
}


VS_Output main(float4 ipos : POSITION, uint iid : SV_InstanceID)
{
    float3 opos = 0;
    
    float extent = InstFalloff;
    if (InstType == 1)//point (sphere)
    {
        opos = ipos.xyz * extent;
    }
    else if (InstType == 2)//spot (cone)
    {
        float arads = InstConeOuterAngle;
        float3 cpos = ipos.xyz * (tan(arads) * extent);
        cpos.y += ipos.w * extent;
        opos = (cpos.x * InstTangentX) + (cpos.y * InstDirection) + (cpos.z * InstTangentY);
    }
    else if (InstType == 4)//capsule
    {
        float3 cpos = ipos.xyz * extent;
        cpos += InstCapsuleExtent * (ipos.w * 2 - 1) * 0.1;
        opos = (cpos.x * InstTangentX.xyz) + (cpos.y * InstDirection.xyz) + (cpos.z * InstTangentY.xyz);
    }
    
    float4 spos = mul(float4(opos + InstPosition, 1), ViewProj);
    VS_Output output;
    output.Pos = spos;
    output.Screen = spos;
    return output;
}
