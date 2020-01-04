#include "LightPS.hlsli"


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

PS_OUTPUT main(VS_Output input)
{
    uint3 ssloc = uint3(input.Pos.xy, 0); //pixel location
    float depth = DepthTex.Load(ssloc).r;
    if (depth == 0) discard; //no existing pixel rendered here
    
    float4 diffuse = DiffuseTex.Load(ssloc);
    float4 normal = NormalTex.Load(ssloc);
    float4 specular = SpecularTex.Load(ssloc);
    float4 irradiance = IrradianceTex.Load(ssloc);
    
    PS_OUTPUT output;
    output.Depth = input.Pos.z;
    
    switch (RenderMode)
    {
        case 5: output.Colour = float4(diffuse.rgb, 1); return output;
        case 6: output.Colour = float4(normal.rgb, 1); return output;
        case 7: output.Colour = float4(specular.rgb, 1); return output;
    }
    
    float4 spos = float4(input.Screen.xy/input.Screen.w, depth, 1);
    float4 cpos = mul(spos, ViewProjInv);
    float3 camRel = cpos.xyz * (1/cpos.w);
    float3 norm = normal.xyz * 2 - 1;
    
    float3 c = DeferredDirectionalLight(camRel, norm, diffuse, specular, irradiance);
        
    output.Colour = float4(c, 1);
    output.Depth = depth;
    return output;
}

