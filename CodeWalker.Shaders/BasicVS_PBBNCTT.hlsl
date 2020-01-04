#include "BasicVS.hlsli"

struct VS_INPUT
{
    float4 Position : POSITION;
    float4 BlendWeights : BLENDWEIGHTS;
    float4 BlendIndices : BLENDINDICES;
    float3 Normal : NORMAL;
    float4 Colour0 : COLOR0;
    float2 Texcoord0 : TEXCOORD0;
    float2 Texcoord1 : TEXCOORD1;
};


VS_OUTPUT main(VS_INPUT input, uint iid : SV_InstanceID)
{
    VS_OUTPUT output;
    float3 bpos, bnorm, btang;
    BoneTransform(input.BlendWeights, input.BlendIndices, input.Position.xyz, input.Normal, float3(0, 1, 0), bpos, bnorm, btang);
    float3 opos = ModelTransform(bpos, input.Colour0.xyz, input.Colour0.xyz, iid);
    float4 cpos = ScreenTransform(opos);
    float3 onorm = NormalTransform(bnorm);
    float3 otang = 0.5; // NormalTransform(float3(1, 0, 0)); //no tangent to use on this vertex type...
    
    float4 tnt = ColourTint(input.Colour0.b, 0, iid); //colour tinting if enabled

    float4 lightspacepos;
    float shadowdepth = ShadowmapSceneDepth(opos, lightspacepos);
    output.LightShadow = lightspacepos;
    output.Shadows = float4(shadowdepth, 0, 0, 0);

    output.Position = cpos;
    output.CamRelPos = opos;
    output.Normal = onorm;
    output.Texcoord0 = GlobalUVAnim(input.Texcoord0);
    output.Texcoord1 = input.Texcoord1;
    output.Texcoord2 = 0.5; // input.Texcoord;
    output.Colour0 = input.Colour0;
    output.Colour1 = float4(0.5, 0.5, 0.5, 1); //input.Colour;
    output.Tint = tnt;
    output.Tangent = float4(otang, 1);
    output.Bitangent = float4(cross(otang, onorm), 0);
    return output;
}

