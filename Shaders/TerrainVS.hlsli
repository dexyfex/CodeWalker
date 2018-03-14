#include "Shadowmap.hlsli"
#include "Quaternion.hlsli"


cbuffer VSSceneVars : register(b0)
{
    float4x4 ViewProj;
}
cbuffer VSEntityVars : register(b2)
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
cbuffer VSModelVars : register(b3)
{
    float4x4 Transform;
}
cbuffer VSGeomVars : register(b4)
{
    uint EnableTint;
    float TintYVal;
    uint Pad4;
    uint Pad5;
}



struct VS_OUTPUT
{
    float4 Position : SV_POSITION;
    float3 Normal   : NORMAL;
    float4 Colour0   : COLOR0;
    float4 Colour1   : COLOR1;
    float4 Tint      : COLOR2;
    float2 Texcoord0 : TEXCOORD0;
    float2 Texcoord1 : TEXCOORD1;
    float2 Texcoord2 : TEXCOORD2;
    float4 Shadows   : TEXCOORD4;
    float4 LightShadow : TEXCOORD5;
    float4 Tangent     : TEXCOORD6;
    float4 Bitangent   : TEXCOORD7;
    float3 CamRelPos   : TEXCOORD8;
};

Texture2D<float4> TintPalette : register(t0);
SamplerState TextureSS : register(s0);




float3 ModelTransform(float3 ipos)
{
    float3 tpos = (HasTransforms == 1) ? mul(float4(ipos, 1), Transform).xyz : ipos;
    float3 spos = tpos * Scale;
    float3 bpos = mulvq(spos, Orientation);
    return CamRel.xyz + bpos;
}
float4 ScreenTransform(float3 opos)
{
    float4 pos = float4(opos, 1);
    float4 cpos = mul(pos, ViewProj);
    cpos.z = DepthFunc(cpos.zw);
    return cpos;
}
float3 NormalTransform(float3 inorm)
{
    float3 tnorm = (HasTransforms == 1) ? mul(inorm, (float3x3)Transform) : inorm;
    float3 bnorm = normalize(mulvq(tnorm, Orientation));
    return bnorm;
}

float4 ColourTint(float tx)
{
    float4 tnt = 1;
    if (EnableTint == 1)
    {
        tnt = TintPalette.SampleLevel(TextureSS, float2(tx, TintYVal), 0);
    }
    return tnt;
}

