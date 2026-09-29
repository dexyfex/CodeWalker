#include "Common.hlsli"
#include "Quaternion.hlsli"


cbuffer VSSceneVars : register(b0)
{
    float4x4 ViewProj;
    float4 WindVector;
}
cbuffer VSEntityVars : register(b1)
{
    float4 CamRel;
    float4 Orientation;
    uint HasSkeleton;
    uint HasTransforms;
    uint TintPaletteIndex;
    uint Pad1;
    float3 Scale;
    uint Pad2;
}
cbuffer VSModelVars : register(b2)
{
    float4x4 Transform;
}
cbuffer GeomVars : register(b3)
{
    uint EnableTexture;
    uint EnableTint;
    uint IsDecal;
    uint EnableWind;
    float4 WindOverrideParams;
}

struct VS_INPUT
{
    float4 Position : POSITION;
    float3 Normal   : NORMAL;
    float2 Texcoord : TEXCOORD0;
    float4 Colour   : COLOR0;
};

struct VS_OUTPUT
{
    float4 Position : SV_POSITION;
    //float3 Normal   : NORMAL;
    float2 Texcoord : TEXCOORD0;
    //float4 Colour   : COLOR0;
    //float4 Tint     : COLOR1;
};

//Texture2D<float4> TintPalette : register(t0);
//SamplerState TextureSS : register(s0);


VS_OUTPUT main(VS_INPUT input)
{
    VS_OUTPUT output;
    float3 ipos = input.Position.xyz;
    float3 tpos = (HasTransforms == 1) ? mul(float4(ipos, 1), Transform).xyz : ipos;
    float3 spos = tpos * Scale;
    float3 bpos = mulvq(spos, Orientation);
    if (EnableWind)
    {
        bpos = GeomWindMotion(bpos, input.Colour.xyz, WindVector, WindOverrideParams);
    }
    float3 opos = CamRel.xyz + bpos;
    float4 pos = float4(opos, 1);
    float4 cpos = mul(pos, ViewProj);
    //if (IsDecal == 1)
    //{
    //    //cpos.z -= 0.003; //todo: correct decal z-bias
    //}
    //cpos.z = saturate(cpos.z); //might need work

    //cpos.z = DepthFunc(cpos.zw);

    //float3 inorm = input.Normal;
    //float3 tnorm = (HasTransforms == 1) ? mul(inorm, (float3x3)Transform) : inorm;
    //float3 bnorm = normalize(mulvq(tnorm, Orientation));

    //float4 tnt = 0;
    //if (EnableTint == 1)
    //{
    //    tnt = TintPalette.SampleLevel(TextureSS, float2(input.Colour.b, TintYVal), 0);
    //}

    output.Position = cpos;
    //output.Normal = bnorm;
    output.Texcoord = input.Texcoord;
    //output.Colour = input.Colour;
    //output.Tint = tnt;
    return output;
}