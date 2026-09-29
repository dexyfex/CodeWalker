#include "BasicVS.hlsli"

struct VS_INPUT
{
    float4 Position  : POSITION;
    //float3 Normal    : NORMAL;
};

struct RenderableCapsule
{
    float3 Point1;
    float Radius;
    float4 Orientation;
    float Length;
    uint Colour;
    float Pad0;
    float Pad1;
};

StructuredBuffer<RenderableCapsule> Capsules : register(t1);


VS_OUTPUT main(VS_INPUT input, uint iid : SV_InstanceID)
{
    VS_OUTPUT output;

    RenderableCapsule cap = Capsules[iid];

    float3 mpos = input.Position.xyz*cap.Radius;
    mpos.y += input.Position.w*cap.Length;

    float3 ipos = mulvq(mpos, cap.Orientation);
    float3 inorm = mulvq(input.Position.xyz, cap.Orientation);// input.Normal;

    float3 spos = (ipos + cap.Point1) * Scale;
    float3 rpos = mulvq(spos, Orientation);
    float3 opos = CamRel.xyz + rpos;// bpos ModelTransform(input.Position.xyz);
    float4 cpos = ScreenTransform(opos);
    float3 bnorm = normalize(mulvq(inorm, Orientation));// NormalTransform(input.Normal);
    float3 btang = 0.5;// NormalTransform(float3(1, 0, 0)); //no tangent to use on this vertex type...

    float4 c = Unpack4x8UNF(cap.Colour).abgr;

    float4 lightspacepos;
    float shadowdepth = ShadowmapSceneDepth(opos, lightspacepos);
    output.LightShadow = lightspacepos;
    output.Shadows = float4(shadowdepth, 0,0,0);

    output.Position = cpos;
    output.CamRelPos = opos;
    output.Normal = bnorm;
    output.Texcoord0 = 0.5;// input.Texcoord0;
    output.Texcoord1 = 0.5;// input.Texcoord;
    output.Texcoord2 = 0.5;// input.Texcoord;
    output.Colour0 = c;// float4(0.5, 0.5, 0.5, 1); //input.Colour0;
    output.Colour1 = float4(0.5,0.5,0.5,1); //input.Colour1
    output.Tint = 0;
    output.Tangent = float4(btang, 1);
    output.Bitangent = float4(cross(btang, bnorm), 0);
    return output;
}

