#include "TreesLodPS.hlsli"


float4 main(VS_OUTPUT input) : SV_TARGET
{
    //return float4(1,0,0,1);//red

    float4 c = 0;// float4(input.Colour.rgb, 1);
    //return c;

    if (EnableTexture == 1)
    {
        //c = Colourmap.SampleLevel(TextureSS, input.Texcoord, 0);
        c = Colourmap.Sample(TextureSS, input.Texcoord);
        if (c.a <= 0.25) discard;
        c.a = 1;
           // c = float4(input.Colour.rgb, 1);
    }

    float3 norm = input.Normal;
    float lf = saturate(dot(normalize(norm), GlobalLights.LightDir.xyz));

    c.rgb = GlobalLighting(c.rgb, norm, input.Colour, lf, GlobalLights);
    c.a = saturate(c.a);

    return c;
}