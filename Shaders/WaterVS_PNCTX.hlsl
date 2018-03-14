#include "WaterVS.hlsli"

struct VS_INPUT
{
    float4 Position  : POSITION;
    float3 Normal    : NORMAL;
    float2 Texcoord0 : TEXCOORD0;
    float4 Colour0   : COLOR0;
    float4 Tangent   : TANGENT;
};


VS_OUTPUT main(VS_INPUT input)
{
    VS_OUTPUT output;
    float3 opos = ModelTransform(input.Position.xyz);
    float4 cpos = ScreenTransform(opos);
    float3 bnorm = NormalTransform(input.Normal);
    float3 btang = NormalTransform(input.Tangent.xyz);

    float4 lightspacepos;
    float shadowdepth = ShadowmapSceneDepth(opos, lightspacepos);
    output.LightShadow = lightspacepos;
    output.Shadows = float4(shadowdepth, 0,0,0);
    output.Position = cpos;
    output.CamRelPos = opos;
    output.Normal = bnorm;
    output.Texcoord0 = GetWaterTexcoords(input.Texcoord0);
    output.Colour0 = input.Colour0;
    output.Tangent = float4(btang, input.Tangent.w);
    output.Bitangent = float4(cross(btang, bnorm) * input.Tangent.w, 0);
    output.Flow = GetWaterFlow(input.Texcoord0, input.Colour0);
    return output;
}