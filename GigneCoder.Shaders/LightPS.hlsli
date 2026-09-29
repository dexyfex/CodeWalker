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

cbuffer PSLightInstVars : register(b2)
{
    float3 InstPosition;//camera relative
    float InstIntensity;
    float3 InstColour;
    float InstFalloff;
    float3 InstDirection;
    float InstFalloffExponent;
    float3 InstTangentX;
    float InstConeInnerAngle;
    float3 InstTangentY;
    float InstConeOuterAngle;
    float3 InstCapsuleExtent;
    uint InstType;
    float3 InstCullingPlaneNormal;
    float InstCullingPlaneOffset;
    uint InstCullingPlaneEnable;
    uint InstUnused1;
    uint InstUnused2;
    uint InstUnused3;
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

float4 GetLineSegmentNearestPoint(float3 v, float3 a, float3 b)
{
    float3 ab = b - a;
    float3 av = v - a;
    if (dot(av, ab) <= 0.0f)// Point is lagging behind start of the segment, so perpendicular distance is not viable.
    {
        return float4(av, length(av));
    }
    else
    {
        float3 bv = v - b;
        if (dot(bv, ab) >= 0.0f)// Point is advanced past the end of the segment, so perpendicular distance is not viable.
        {
            return float4(bv, length(bv));
        }
        else
        {
            float3 abv = cross(ab, av);
            float d = length(abv) / length(ab);
            return float4(normalize(cross(abv, ab)) * d, d); //improve this!
        }
    }
}

float GetAttenuation(float ldist, float falloff, float falloffExponent)
{
    float d = ldist / falloff;
    return saturate((1 - d) / (1 + d*d*falloffExponent));
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
    if (LightType == 4)//capsule
    {
        float3 ext = lodlight.Direction.xyz * lodlight.OuterAngleOrCapExt;
        float4 lsn = GetLineSegmentNearestPoint(srpos, ext, -ext);
        ldist = lsn.w;
        srpos.xyz = lsn.xyz;
    }

    if (ldist > lodlight.Falloff) return 0; //out of range of the light...
    if (ldist <= 0) return 0;
    
    float4 rgbi = Unpack4x8UNF(lodlight.Colour).gbar;
    float3 lcol = rgbi.rgb * rgbi.a * 96.0f;
    float3 ldir = srpos / ldist;
    float pclit = saturate(dot(ldir, norm));
    float lamt = 1;
    
    if (LightType == 1)//point (sphere)
    {
        lamt *= GetAttenuation(ldist, lodlight.Falloff, lodlight.FalloffExponent);
    }
    else if (LightType == 2)//spot (cone)
    {
        float ang = acos(-dot(ldir, lodlight.Direction));
        float iang = lodlight.InnerAngle;
        float oang = lodlight.OuterAngleOrCapExt;
        if (ang > oang) return 0;
        lamt *= saturate(1 - ((ang - iang) / (oang - iang)));
        lamt *= GetAttenuation(ldist, lodlight.Falloff, lodlight.FalloffExponent);
    }
    else if (LightType == 4)//capsule
    {
        lamt *= GetAttenuation(ldist, lodlight.Falloff, lodlight.FalloffExponent); //TODO! proper capsule lighting... (use point-line dist!)
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

float4 DeferredLight(float3 camRel, float3 norm, float4 diffuse, float4 specular, float4 irradiance)
{
    float3 srpos = InstPosition - camRel; //light position relative to surface position
    float ldist = length(srpos);
    if (InstCullingPlaneEnable == 1)
    {
        float d = dot(srpos, InstCullingPlaneNormal) - InstCullingPlaneOffset;
        if (d > 0) return 0;
    }
    if (InstType == 4)//capsule
    {
        float3 ext = InstDirection.xyz * (InstCapsuleExtent.x * 0.5);
        float4 lsn = GetLineSegmentNearestPoint(srpos, ext, -ext);
        ldist = lsn.w;
        srpos.xyz = lsn.xyz;
    }
    if (ldist > InstFalloff) return 0;
    if (ldist <= 0) return 0;
    float4 rgbi = float4(InstColour, InstIntensity);
    float3 lcol = rgbi.rgb;// * rgbi.a; // * 5.0f;
    float3 ldir = srpos / ldist;
    float pclit = saturate(dot(ldir, norm));
    float lamt = 1;
    
    if (InstType == 1)//point (sphere)
    {
        lamt *= GetAttenuation(ldist, InstFalloff, InstFalloffExponent);
    }
    else if (InstType == 2)//spot (cone)
    {
        float ang = acos(-dot(ldir, InstDirection));
        float iang = InstConeInnerAngle;
        float oang = InstConeOuterAngle;
        if (ang > oang) return 0;
        lamt *= saturate(1 - ((ang - iang) / (oang - iang)));
        lamt *= GetAttenuation(ldist, InstFalloff, InstFalloffExponent);
    }
    else if (InstType == 4)//capsule
    {
        lamt *= GetAttenuation(ldist, InstFalloff, InstFalloffExponent);
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





