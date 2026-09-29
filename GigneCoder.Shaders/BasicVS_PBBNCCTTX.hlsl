#include "BasicVS.hlsli"

struct VS_INPUT
{
    float4 Position : POSITION;
    float4 BlendWeights : BLENDWEIGHTS;
    float4 BlendIndices : BLENDINDICES;
    float3 Normal : NORMAL;
    float4 Colour0 : COLOR0;
    float4 Colour1 : COLOR1;
    float2 Texcoord0 : TEXCOORD0;
    float2 Texcoord1 : TEXCOORD1;
    float4 Tangent : TANGENT;
};


VS_OUTPUT main(VS_INPUT input, uint iid : SV_InstanceID)
{
    VS_OUTPUT output;
    float3 bpos, bnorm, btang;
    BoneTransform(input.BlendWeights, input.BlendIndices, input.Position.xyz, input.Normal, input.Tangent.xyz, bpos, bnorm, btang);
    float3 opos = ModelTransform(bpos, input.Colour0.xyz, input.Colour1.xyz, iid);
    float4 cpos = ScreenTransform(opos);
    float3 onorm = NormalTransform(bnorm);
    float3 otang = NormalTransform(btang);

    float4 tnt = ColourTint(input.Colour0.b, input.Colour1.b, iid); //colour tinting if enabled

    float4 lightspacepos;
    float shadowdepth = ShadowmapSceneDepth(opos, lightspacepos);
    output.LightShadow = lightspacepos;
    output.Shadows = float4(shadowdepth, 0, 0, 0);

    output.Position = cpos;
    output.CamRelPos = opos;
    output.Normal = onorm;
    output.Texcoord0 = GlobalUVAnim(input.Texcoord0);
    output.Texcoord1 = input.Texcoord1; // input.Texcoord;
    output.Texcoord2 = 0.5; // input.Texcoord;
    output.Colour0 = input.Colour0;
    output.Colour1 = input.Colour1;
    output.Tint = tnt;
    output.Tangent = float4(otang, input.Tangent.w);
    output.Bitangent = float4(cross(otang, onorm) * input.Tangent.w, 0);
    return output;
}


/*




WIND FOR CLOTHING

mul r1.xyz, v8.xxzx, umGlobalParams.xxyx   //colour[1].XXZX
mul r1.xyz, r1.xyzx, umPedGlobalOverrideParams.xxyx
mul r0.w, v8.y, l(6.283185)   //colour[1].Y
mul r2.xyz, umPedGlobalOverrideParams.zzwz, umGlobalParams.zzwz
mad r2.xyz, globalScalars2.xxxx, r2.xyzx, r0.wwww
sincos r2.xyz, null, r2.xyzx
mad r1.xyz, r2.xyzx, r1.xyzx, r4.xyzx //OUTPUT - r4 is base bone transform, r1,r2?


translation:

r1.xyz = umGlobalParams.xxy * umPedGlobalOverrideParams.xxy * vc[1].xxz;
r2.xyz = umGlobalParams.zzw * umPedGlobalOverrideParams.zzw * globalScalars2.xxx + (vc[1].y * 6.283185)
pos.xyz += r1 * cos(r2);



*/