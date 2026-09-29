#include "Common.hlsli"
#include "Quaternion.hlsli"


cbuffer VSSceneVars : register(b0)
{
    float4x4 ViewProj;
}
cbuffer VSEntityVars : register(b1)
{
    float4 CamRel;
    float4 Orientation;
    uint HasSkeleton;
    uint HasTransforms;
    uint Pad0;
    uint Pad1;
    float3 Scale;
    uint Pad2;
}
cbuffer VSModelVars : register(b2)
{
    float4x4 Transform;
}
cbuffer TreesLodShaderVSGeometryVars : register(b3)
{
    float4 AlphaTest;
    float4 AlphaScale;
    float4 UseTreeNormals;
    float4 treeLod2Normal;
    float4 treeLod2Params;
}


struct VS_INPUT
{
    float4 Position : POSITION;
    float3 Normal   : NORMAL;
    float2 Texcoord0 : TEXCOORD0;
    float2 Texcoord1 : TEXCOORD1;
    float2 Texcoord2 : TEXCOORD2;
    float2 Texcoord3 : TEXCOORD3;
    float4 Colour0   : COLOR0;
    float4 Colour1   : COLOR1;
};

struct VS_OUTPUT
{
    float4 Position : SV_POSITION;
    float3 Normal   : NORMAL;
    float2 Texcoord : TEXCOORD0;
    float4 Colour   : COLOR0;
};



VS_OUTPUT main(VS_INPUT input)
{
    VS_OUTPUT output;

    //first find the base point of the billboard.. full transformation may not be necessary!
    float3 ipos = input.Position.xyz;
    float3 tpos = (HasTransforms == 1) ? mul(float4(ipos, 1), Transform).xyz : ipos;
    float3 spos = tpos;// *Scale;
    float3 bpos = mulvq(spos, Orientation);
    float3 opos = CamRel.xyz + bpos;

    float3 dir = normalize(opos);
    float3 bbside = normalize(cross(dir, treeLod2Normal.xyz));
    float2 bbvpos = treeLod2Params.xy*(0.5 - input.Texcoord0)*input.Texcoord2;
    opos += bbside*bbvpos.x;
    opos += treeLod2Normal.xyz*bbvpos.y;



    //float3 ipos = input.Position.xyz + float3(treeLod2Params.xy*(input.Texcoord0-0.5)*input.Texcoord2,0);
    //float3 tpos = (HasTransforms == 1) ? mul(float4(ipos, 1), Transform).xyz : ipos;
    //float3 spos = tpos;// *Scale;
    //float3 bpos = mulvq(spos, Orientation);
    //float3 opos = CamRel.xyz + bpos;

    float4 pos = float4(opos, 1);
    float4 cpos = mul(pos, ViewProj);
    cpos.z = DepthFunc(cpos.zw);

    //float3 inorm = input.Normal;
    //float3 tnorm = (HasTransforms == 1) ? mul(inorm, (float3x3)Transform) : inorm;
    //float3 bnorm = normalize(mulvq(tnorm, Orientation));
    float3 bnorm = normalize(-pos.xyz); //normal pointing towards the camera...

    output.Position = cpos;
    output.Normal = bnorm;
    output.Texcoord = input.Texcoord1;
    output.Colour = input.Colour0;// float4(input.Texcoord2, 0, 1);// input.Colour1;
    return output;
}