#include "BasicVS.hlsli"

struct VS_INPUT
{
    float4 Position : POSITION;
    float4 BlendWeights : BLENDWEIGHTS;
    float4 BlendIndices : BLENDINDICES;
    float3 Normal : NORMAL;
    float4 Colour0 : COLOR0;
    float2 Texcoord0 : TEXCOORD0;
    float4 Tangent : TANGENT;
};


VS_OUTPUT main(VS_INPUT input, uint iid : SV_InstanceID)
{
    VS_OUTPUT output;
    float3 bpos, bnorm, btang;
    BoneTransform(input.BlendWeights, input.BlendIndices, input.Position.xyz, input.Normal, input.Tangent.xyz, bpos, bnorm, btang);
    float3 opos = ModelTransform(bpos, input.Colour0.xyz, input.Colour0.xyz, iid);
    float4 cpos = ScreenTransform(opos);
    float3 onorm = NormalTransform(bnorm);
    float3 otang = NormalTransform(btang);

    float4 tnt = ColourTint(input.Colour0.b, 0, iid); //colour tinting if enabled

    float4 lightspacepos;
    float shadowdepth = ShadowmapSceneDepth(opos, lightspacepos);
    output.LightShadow = lightspacepos;
    output.Shadows = float4(shadowdepth, 0, 0, 0);

    output.Position = cpos;
    output.CamRelPos = opos;
    output.Normal = onorm;
    output.Texcoord0 = GlobalUVAnim(input.Texcoord0);
    output.Texcoord1 = 0.5; // input.Texcoord;
    output.Texcoord2 = 0.5; // input.Texcoord;
    output.Colour0 = input.Colour0;
    output.Colour1 = float4(0.5, 0.5, 0.5, 1); //input.Colour
    output.Tint = tnt;
    output.Tangent = float4(otang, input.Tangent.w);
    output.Bitangent = float4(cross(otang, onorm) * input.Tangent.w, 0);
    return output;
}

