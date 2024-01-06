#include "LightPS.hlsli"

SamplerState TextureSS : register(s0);

Texture2D DepthTex : register(t0);
Texture2D DiffuseTex : register(t2);
Texture2D NormalTex : register(t3);
Texture2D SpecularTex : register(t4);
Texture2D IrradianceTex : register(t5);

struct VS_Output
{
    float4 Pos : SV_POSITION;
    float4 Screen : TEXCOORD0;
};


float4 main(VS_Output input) : SV_TARGET
{
    uint3 ssloc = uint3(input.Pos.xy, 0); //pixel location
    float depth = DepthTex.Load(ssloc).r;
    if (depth <= 9.99999997e-07)
        discard; //no existing pixel rendered here
    
    float4 diffuse = DiffuseTex.Load(ssloc);
    float4 normal = NormalTex.Load(ssloc);
    float4 specular = SpecularTex.Load(ssloc);
    float4 irradiance = IrradianceTex.Load(ssloc);
    
    float4 spos = float4(input.Screen.xy/input.Screen.w, depth, 1);
    float4 cpos = mul(spos, ViewProjInv);
    float3 camRel = cpos.xyz * (1/cpos.w);
    float3 norm = normal.xyz * 2 - 1;
    
    float4 lcol = DeferredLight(camRel, norm, diffuse, specular, irradiance);
    if (lcol.a <= 0)
        discard;

    return lcol;
}

