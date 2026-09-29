#include "WaterPS.hlsli"


PS_OUTPUT main(VS_OUTPUT input)
{
    float4 c = float4(0.1, 0.18, 0.25, 0.8);
    //if (RenderMode == 0) c = float4(1, 1, 1, 1);

    //c.a *= input.Colour0.a;

    float3 camrel = input.CamRelPos;
    float3 worldpos = camrel + CameraPos.xyz;
    if ((EnableFoamMap == 0) && (EnableFogtex == 1))
    {
        float2 fogtc = saturate((worldpos.xy - WaterFogParams.xy) * WaterFogParams.zw);
        fogtc.y = 1.0 - fogtc.y;
        c = WaterFog.Sample(TextureSS, fogtc);
        c.a = 0.9;
    }


    float3 norm = EnableFoamMap ? normalize(input.Normal) : RippleNormal(input, worldpos); //  normalize(input.Normal);

    if (RenderMode == 1) //normals
    {
        c.rgb = norm * 0.5 + 0.5;
    }
    else if (RenderMode == 2) //tangents
    {
        c.rgb = normalize(input.Tangent.rgb) * 0.5 + 0.5;
    }
    else if (RenderMode == 3) //colours
    {
        c.rgb = input.Colour0.rgb;
    }
    else if (RenderMode == 4) //texcoords
    {
        c.rgb = float3(input.Texcoord0, 0);
    }
    else if ((RenderMode == 8) || (EnableTexture == 1)) //single texture or diffuse enabled
    {
        c.rgb = Colourmap.Sample(TextureSS, input.Texcoord0).rgb;
    }
    else if (EnableFoamMap)
    {
        c = Foammap.Sample(TextureSS, input.Texcoord0);
    }


    float3 spec = 0;

    if (RenderMode == 0)
    {

        float4 nv = Bumpmap.Sample(TextureSS, input.Texcoord0); //sample r1.xyzw, v2.xyxx, t3.xyzw, s3  (BumpSampler)


        float2 nmv = nv.xy;
        float4 r0 = 0, r1, r2, r3;

        float bumpiness = 0.5;

        if (EnableBumpMap)
        {
            norm = NormalMap(nmv, bumpiness, input.Normal.xyz, input.Tangent.xyz, input.Bitangent.xyz);
        }


        float3 tc = c.rgb;
        c.rgb = tc; // *r0.z; //diffuse factors...

        
        spec.xy = sqrt(10.0 * SpecularIntensity);
        spec.z = 1;//r0.z;

        

        if (ShaderMode == 1) //river foam
        {
            c.a *= input.Colour0.g;
        }
        else if (ShaderMode == 2) //terrain foam
        {
            c.a *= c.r;
            c.a *= input.Colour0.r;
        }
        else
        {
            ///c.a = 1;
        }
    }
    
    c.a = saturate(c.a);
    
    PS_OUTPUT output;
    output.Diffuse = c;
    output.Normal = float4(saturate(norm * 0.5 + 0.5), c.a);
    output.Specular = float4(spec, c.a);
    output.Irradiance = float4(1, 0, 0, c.a);
    
    return output;
}



