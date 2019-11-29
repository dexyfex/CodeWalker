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
cbuffer BoneMatrices : register(b7) //rage_bonemtx
{
    row_major float3x4 gBoneMtx[255]; // Offset:    0 Size: 12240
}
cbuffer ClothVertices : register(b8) //pedcloth
{
    float4 clothVertices[254]; // Offset:   64 Size:  4060
}

struct VS_INPUT
{
    float4 Position : POSITION;
    float4 BlendWeights : BLENDWEIGHTS;
    float4 BlendIndices : BLENDINDICES;
    float3 Normal : NORMAL;
    float2 Texcoord : TEXCOORD0;
    float4 Colour : COLOR0;
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



void BoneTransform(float4 weights, float4 indices, float3 ipos, out float3 opos)
{
    uint4 binds = (uint4) (indices * 255.001953);
    if (binds.z > 254) //this is the signal to use clothVertices!
    {
        float4 cv0 = clothVertices[binds.w];
        float4 cv1 = clothVertices[binds.x];
        float4 cv2 = clothVertices[binds.y];
        float3 r0 = cv0.zxy - cv1.zxy;
        float3 r8 = cv2.yzx - cv1.yzx;
        float3 r4 = normalize((r0.zxy * r8.yzx) - (r0.xyz * r8.xyz));
        float3 r5 = (cv2.xyz * weights.x) + (cv1.xyz * weights.y) + (cv0.xyz * weights.z);
        float r0w = (weights.w - 0.5) * 0.1;
        r5 = (r0w * -r4) + r5;
        opos = r5;
    }
    else
    {
        float3x4 b0 = gBoneMtx[binds.x];
        float3x4 b1 = gBoneMtx[binds.y];
        float3x4 b2 = gBoneMtx[binds.z];
        float3x4 b3 = gBoneMtx[binds.w];
        float4 t0 = b0[0] * weights.x + b1[0] * weights.y + b2[0] * weights.z + b3[0] * weights.w;
        float4 t1 = b0[1] * weights.x + b1[1] * weights.y + b2[1] * weights.z + b3[1] * weights.w;
        float4 t2 = b0[2] * weights.x + b1[2] * weights.y + b2[2] * weights.z + b3[2] * weights.w;
        float3x4 m = float3x4(t0, t1, t2);
        float4 p = float4(ipos, 1);
        opos = float3(dot(m[0], p), dot(m[1], p), dot(m[2], p));
    }
}





VS_OUTPUT main(VS_INPUT input)
{
    VS_OUTPUT output;
    float3 ipos;
    BoneTransform(input.BlendWeights, input.BlendIndices, input.Position.xyz, ipos);
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