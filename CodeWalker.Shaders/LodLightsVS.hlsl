#include "Common.hlsli"

struct LODLight
{
    float3 Position;
    uint Colour;
    float3 Direction;
    uint TimeAndStateFlags;
    float4 TangentX;
    float4 TangentY;
    float Falloff;
    float FalloffExponent;
    float InnerAngle; //for cone
    float OuterAngleOrCapExt; //outer angle for cone, cap extent for capsule
    float Distance;
};

struct VS_Output
{
    float4 Pos : SV_POSITION;
    float4 Screen : TEXCOORD0;
    uint IID : SV_INSTANCEID;
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

StructuredBuffer<LODLight> LODLights : register(t0);


VS_Output main(float4 ipos : POSITION, uint iid : SV_InstanceID)
{
    LODLight lodlight = LODLights[iid];
    float extent = lodlight.Falloff;
    float3 opos = 0;
    if (LightType == 1)//point (sphere)
    {
        opos = ipos.xyz * extent;
    }
    else if (LightType == 2)//spot (cone)
    {
        float arads = lodlight.OuterAngleOrCapExt;
        float3 tpos = (ipos.xyz * sin(arads)) + float3(0, 0, ipos.w * cos(arads));
        float3 cpos = ((ipos.w>0) ? normalize(tpos) : tpos) * extent;
        opos = (cpos.x * lodlight.TangentX.xyz) + (cpos.y * lodlight.TangentY.xyz) + (cpos.z * lodlight.Direction.xyz);
    }
    else if (LightType == 4)//capsule
    {
        float3 cpos = ipos.xyz * extent;
        cpos.y += (ipos.w * 2 - 1) * lodlight.OuterAngleOrCapExt;
        opos = (cpos.x * lodlight.TangentX.xyz) + (cpos.y * lodlight.Direction.xyz) + (cpos.z * lodlight.TangentY.xyz);
    }

    opos += (lodlight.Position - CameraPos.xyz);
    float4 spos = mul(float4(opos, 1), ViewProj);
    VS_Output output;
    output.Pos = spos;
    output.Screen = spos;
    output.IID = iid;
    return output;
}
