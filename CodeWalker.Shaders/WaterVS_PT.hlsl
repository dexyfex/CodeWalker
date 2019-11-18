#include "WaterVS.hlsli"

struct VS_INPUT
{
    float4 Position  : POSITION;
    float2 Texcoord0 : TEXCOORD0;
};


VS_OUTPUT main(VS_INPUT input)
{
    VS_OUTPUT output;
    float3 opos = ModelTransform(input.Position.xyz);
    float4 cpos = ScreenTransform(opos);
    float3 bnorm = float3(0, 0, 1);// NormalTransform(float3(0, 0, 1));
    float3 btang = float3(0,1,0);// NormalTransform(float3(1, 0, 0)); //no tangent to use on this vertex type...

    float4 lightspacepos;
    float shadowdepth = ShadowmapSceneDepth(opos, lightspacepos);
    output.LightShadow = lightspacepos;
    output.Shadows = float4(shadowdepth, 0,0,0);
    output.Position = cpos;
    output.CamRelPos = opos;
    output.Normal = bnorm;
    output.Texcoord0 = GetWaterTexcoords(input.Texcoord0);
    output.Colour0 = 0.1;
    output.Tangent = float4(btang, 1);
    output.Bitangent = float4(cross(btang, bnorm), 0);
    output.Flow = GetWaterFlow(input.Texcoord0, 1);
    return output;
}

