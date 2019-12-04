#include "Shadowmap.hlsli"


struct PS_OUTPUT
{
    float4 Colour : SV_TARGET;
    float Depth : SV_DEPTH;
};

cbuffer PSLightVars : register(b0)
{
    ShaderGlobalLightParams GlobalLights;
    float4x4 ViewProjInv;
    float4 CameraPos;
    uint EnableShadows;
    uint RenderMode; //0=default, 1=normals, 2=tangents, 3=colours, 4=texcoords, 5=diffuse, 6=normalmap, 7=spec, 8=direct
    uint RenderModeIndex;
    uint RenderSamplerCoord;
    uint LightType; //0=directional, 1=Point, 2=Spot, 4=Capsule
    uint IsLOD; //useful or not?
    uint SampleCount;//for MSAA
    float SampleMult;//for MSAA
}




struct LODLight
{
    float3 Position;
    uint Colour;
    float3 Direction;
    uint TimeAndStateFlags;
    float4 TangentX;
    float4 TangentY;
    float Falloff;
    float FalloffExponent;
    float InnerAngle; //for cone
    float OuterAngleOrCapExt; //outer angle for cone, cap extent for capsule
};

StructuredBuffer<LODLight> LODLights : register(t6);




float3 GetReflectedDir(float3 camRel, float3 norm)
{
    float3 incident = normalize(camRel);
    float3 refl = normalize(reflect(incident, norm));
    return refl;
}




float3 DeferredDirectionalLight(float3 camRel, float3 norm, float4 diffuse, float4 specular, float4 irradiance)
{
    float3 refl = GetReflectedDir(camRel, norm);
    float specb = saturate(dot(refl, GlobalLights.LightDir));
    float specp = max(exp(specb * 10) - 1, 0);
    float3 spec = GlobalLights.LightDirColour.rgb * 0.00006 * specp * specular.r;
    float4 lightspacepos;
    float shadowdepth = ShadowmapSceneDepth(camRel, lightspacepos);
    float3 c = FullLighting(diffuse.rgb, spec, norm, irradiance, GlobalLights, EnableShadows, shadowdepth, lightspacepos);
    c += diffuse.rgb * irradiance.b; //emissive multiplier
    return c;
}

float4 DeferredLODLight(float3 camRel, float3 norm, float4 diffuse, float4 specular, float4 irradiance, uint iid)
{
    LODLight lodlight = LODLights[iid];
    float3 srpos = lodlight.Position - (camRel + CameraPos.xyz); //light position relative to surface position
    float ldist = length(srpos);
    if (ldist > lodlight.Falloff) return 0; //out of range of the light... TODO: capsules!
    if (ldist <= 0) return 0;
    
    float4 rgbi = Unpack4x8UNF(lodlight.Colour).gbar;
    float3 lcol = rgbi.rgb * rgbi.a * 5.0f;
    float3 ldir = srpos / ldist;
    float pclit = saturate(dot(ldir, norm));
    float lamt = 1;
    
    if (LightType == 1)//point (sphere)
    {
        lamt *= pow(saturate(1 - (ldist / lodlight.Falloff)), lodlight.FalloffExponent);
    }
    else if (LightType == 2)//spot (cone)
    {
        float ang = acos(-dot(ldir, lodlight.Direction));
        float iang = lodlight.InnerAngle * 0.01745329;
        float oang = lodlight.OuterAngleOrCapExt * 0.01745329 * 0.5;
        if (ang > oang) return 0;
        lamt *= saturate(1 - ((ang - iang) / (oang - iang)));
        lamt *= pow(saturate(1 - (ldist / lodlight.Falloff)), lodlight.FalloffExponent);
    }
    else if (LightType == 4)//capsule
    {
        lamt *= pow(saturate(1 - (ldist / lodlight.Falloff)), lodlight.FalloffExponent); //TODO! proper capsule lighting... (use point-line dist!)
    }
    
    pclit *= lamt;
    
    if (pclit <= 0) return 0;
    
    float3 refl = GetReflectedDir(camRel, norm);
    float specb = saturate(dot(refl, ldir));
    float specp = max(exp(specb * 10) - 1, 0);
    float3 spec = lcol * (0.00006 * specp * specular.r * lamt);

    lcol = lcol * diffuse.rgb * pclit + spec;

    return float4(lcol, 1);
}



