#include "CablePS.hlsli"


PS_OUTPUT main(VS_OUTPUT input)
{
    float4 c = float4(0.2, 0.2, 0.2, 1);
    if (EnableTexture == 1)
    {
        float2 texc = input.Texcoord0;
        if (RenderMode >= 5)
        {
            if (RenderSamplerCoord == 2) texc = input.Texcoord1;
            else if (RenderSamplerCoord == 3) texc = input.Texcoord2;
        }

        c = Colourmap.Sample(TextureSS, texc);
        //c = Depthmap.SampleLevel(DepthmapSS, input.Texcoord, 0); c.a = 1;
        //if ((IsDecal == 0) && (c.a <= 0.33)) discard;
        //if ((IsDecal == 1) && (c.a <= 0.0)) discard;
        //if(IsDecal==0) c.a = 1;
        c.a = 1;
    }
    //else //if(RenderMode!=8)//
    //{
    //    c = float4(input.Colour0.rgb, 1);
    //}
    if (EnableTint > 0)
    {
        c.rgb *= input.Tint.rgb;
    }
    //if (IsDecal == 1)
    //{
    //    c.a *= input.Colour0.a;
    //}

    float3 norm = normalize(input.Normal);

    if (RenderMode == 1) //normals
    {
        c.rgb = norm*0.5+0.5;
    }
    else if (RenderMode == 2) //tangents
    {
        c.rgb = normalize(input.Tangent.rgb)*0.5+0.5;
    }
    else if (RenderMode == 3) //colours
    {
        c.rgb = input.Colour0.rgb;
        if (RenderModeIndex == 2) c.rgb = input.Colour1.rgb;
    }
    else if (RenderMode == 4) //texcoords
    {
        c.rgb = float3(input.Texcoord0, 0);
        if (RenderModeIndex == 2) c.rgb = float3(input.Texcoord1, 0);
        if (RenderModeIndex == 3) c.rgb = float3(input.Texcoord2, 0);
    }



    float3 spec = 0;

    c.a = saturate(c.a);
    
    PS_OUTPUT output;
    output.Diffuse = c;
    output.Normal = float4(saturate(norm * 0.5 + 0.5), c.a);
    output.Specular = float4(spec, c.a);
    output.Irradiance = float4(input.Colour0.rg, 0, c.a);

    return output;
}