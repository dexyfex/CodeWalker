#include "TerrainVS.hlsli"

struct VS_INPUT
{
    float4 Position : POSITION;
    float3 Normal   : NORMAL;
    float4 Colour0   : COLOR0;
    float4 Colour1   : COLOR1;
    float2 Texcoord0 : TEXCOORD0;
    float4 Tangent   : TANGENT;
};

VS_OUTPUT main(VS_INPUT input)
{
    VS_OUTPUT output;
    float3 opos = ModelTransform(input.Position.xyz);
    float4 cpos = ScreenTransform(opos);
    float3 bnorm = NormalTransform(input.Normal);
    float3 btang = NormalTransform(input.Tangent.xyz);

    float4 tnt = ColourTint(input.Colour0.b); //colour tinting if enabled

    float4 lightspacepos;
    float shadowdepth = ShadowmapSceneDepth(opos, lightspacepos);
    output.LightShadow = lightspacepos;
    output.Shadows = float4(shadowdepth, 0,0,0);

    output.Position = cpos;
    output.CamRelPos = opos;
    output.Normal = bnorm;
    output.Colour0 = input.Colour0;
    output.Colour1 = input.Colour1;
    output.Texcoord0 = input.Texcoord0;
    output.Texcoord1 = 0.5;
    output.Texcoord2 = 0.5;
    output.Tangent = float4(btang, input.Tangent.w);
    output.Bitangent = float4(cross(btang, bnorm) * input.Tangent.w, 0);
    output.Tint = tnt;
    return output;
}
