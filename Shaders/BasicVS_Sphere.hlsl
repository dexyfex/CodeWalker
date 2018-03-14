#include "BasicVS.hlsli"

struct VS_INPUT
{
    float4 Position  : POSITION;
    //float3 Normal    : NORMAL;
};

struct RenderableSphere
{
    float3 Center;
    float Radius;
    float3 Pad0;
    uint Colour;
};

StructuredBuffer<RenderableSphere> Spheres : register(t1);


VS_OUTPUT main(VS_INPUT input, uint iid : SV_InstanceID)
{
    VS_OUTPUT output;

    RenderableSphere sph = Spheres[iid];

    float3 ipos = (input.Position.xyz) * sph.Radius;// *0.5;
    float3 inorm = input.Position.xyz;// input.Normal; //unit sphere pos == normal

    float3 spos = (ipos + sph.Center) * Scale;
    float3 rpos = mulvq(spos, Orientation);
    float3 opos = CamRel.xyz + rpos;// bpos ModelTransform(input.Position.xyz);
    float4 cpos = ScreenTransform(opos);
    float3 bnorm = normalize(mulvq(inorm, Orientation));// NormalTransform(input.Normal);
    float3 btang = 0.5;// NormalTransform(float3(1, 0, 0)); //no tangent to use on this vertex type...

    float4 c = Unpack4x8UNF(sph.Colour).abgr;

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
    output.Colour0 = c;// float4(0.5, 0.5, 0.5, 1); //input.Colour0;//float4(abs(input.Position.xyz), 1);// 
    output.Colour1 = float4(0.5,0.5,0.5,1); //input.Colour1
    output.Tint = 0;
    output.Tangent = float4(btang, 1);
    output.Bitangent = float4(cross(btang, bnorm), 0);
    return output;
}

