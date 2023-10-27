#include "LightPS.hlsli"


Texture2DMS<float> DepthTex : register(t0);
Texture2DMS<float4> DiffuseTex : register(t2);
Texture2DMS<float4> NormalTex : register(t3);
Texture2DMS<float4> SpecularTex : register(t4);
Texture2DMS<float4> IrradianceTex : register(t5);

struct VS_Output
{
    float4 Pos : SV_POSITION;
    float4 Screen : TEXCOORD0;
};

PS_OUTPUT main(VS_Output input)
{
    
    //switch (RenderMode)
    //{
    //    case 5: c += diffuse.rgb; break;
    //    case 6: c += normal.rgb; break;
    //    case 7: c += specular.rgb; break;
    //}
    
    uint2 ssloc = uint2(input.Pos.xy); //pixel location
    float2 spos = float2(input.Screen.xy / input.Screen.w);
    float3 c = 0;
    float d = 0;
    float a = 0;
    int sc = min(SampleCount, 8);
    
    [unroll]
    for (int i = 0; i < sc; i++)
    {
        float depth = DepthTex.Load(ssloc, i);
        if (depth == 0)
            continue; //no existing subpixel rendered here
    
        float4 diffuse = DiffuseTex.Load(ssloc, i);
        float4 normal = NormalTex.Load(ssloc, i);
        float4 specular = SpecularTex.Load(ssloc, i);
        float4 irradiance = IrradianceTex.Load(ssloc, i);
        
        float4 cpos = mul(float4(spos, depth, 1), ViewProjInv);
        float3 camRel = cpos.xyz * (1 / cpos.w);
        float3 norm = normal.xyz * 2 - 1;
        
        float3 colour = DeferredDirectionalLight(camRel, norm, diffuse, specular, irradiance);
        
        c += colour;
        d += depth;
        a += 1;
    }
    
    c *= SampleMult;
    d *= SampleMult;
    a *= SampleMult;
    
    if (d <= 0)
        discard;
    
    PS_OUTPUT output;
    output.Colour = float4(c, a);
    output.Depth = d;
    return output;
}

