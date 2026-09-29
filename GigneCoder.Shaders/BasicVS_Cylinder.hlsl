#include "BasicVS.hlsli"

struct VS_INPUT
{
    float4 Position  : POSITION;
    float3 Normal    : NORMAL;
};

struct RenderableCylinder
{
    float3 Point1;
    float Radius;
    float4 Orientation;
    float Length;
    uint Colour;
    float Pad0;
    float Pad1;
};

StructuredBuffer<RenderableCylinder> Cylinders : register(t1);


VS_OUTPUT main(VS_INPUT input, uint iid : SV_InstanceID)
{
    VS_OUTPUT output;

    RenderableCylinder cyl = Cylinders[iid];

    float3 mpos = input.Position.xyz*cyl.Radius;
    mpos.y += input.Position.w*cyl.Length;

    float3 ipos = mulvq(mpos, cyl.Orientation);
    float3 inorm = mulvq(input.Normal, cyl.Orientation);// input.Normal;

    float3 spos = (ipos + cyl.Point1) * Scale;
    float3 rpos = mulvq(spos, Orientation);
    float3 opos = CamRel.xyz + rpos;// bpos ModelTransform(input.Position.xyz);
    float4 cpos = ScreenTransform(opos);
    float3 bnorm = normalize(mulvq(inorm, Orientation));// NormalTransform(input.Normal);
    float3 btang = 0.5;// NormalTransform(float3(1, 0, 0)); //no tangent to use on this vertex type...

    float4 c = Unpack4x8UNF(cyl.Colour).abgr;

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

