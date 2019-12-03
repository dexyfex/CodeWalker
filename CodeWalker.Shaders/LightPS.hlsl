#include "Shadowmap.hlsli"


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

struct VS_Output
{
    float4 Pos : SV_POSITION;
    float4 Screen : TEXCOORD0;
    uint IID : SV_INSTANCEID;
};

struct PS_OUTPUT
{
    float4 Colour : SV_TARGET;
    float Depth : SV_DEPTH;
};


Texture2D DepthTex : register(t0);
Texture2D DiffuseTex : register(t2);
Texture2D NormalTex : register(t3);
Texture2D SpecularTex : register(t4);
Texture2D IrradianceTex : register(t5);



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
    uint Pad0;
    uint Pad1;
}

StructuredBuffer<LODLight> LODLights : register(t6);




PS_OUTPUT main(VS_Output input)
{
    PS_OUTPUT output;
    output.Depth = input.Pos.z;
    
    uint3 ssloc = uint3(input.Pos.xy, 0); //pixel location
    float depth = DepthTex.Load(ssloc).r;
    float4 diffuse = DiffuseTex.Load(ssloc);
    float4 normal = NormalTex.Load(ssloc);
    float4 specular = SpecularTex.Load(ssloc);
    float4 irradiance = IrradianceTex.Load(ssloc);
    
    if (depth == 0) discard; //no existing pixel rendered here
    
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
    
    float3 incident = normalize(camRel);
    float3 refl = normalize(reflect(incident, norm));
    
    if (LightType == 0) //directional light
    {
        float specb = saturate(dot(refl, GlobalLights.LightDir));
        float specp = max(exp(specb * 10) - 1, 0);
        float3 spec = GlobalLights.LightDirColour.rgb * 0.00006 * specp * specular.r;
        float4 lightspacepos;
        float shadowdepth = ShadowmapSceneDepth(camRel, lightspacepos);
        float3 c = FullLighting(diffuse.rgb, spec, norm, irradiance, GlobalLights, EnableShadows, shadowdepth, lightspacepos);
        
        c += diffuse.rgb * irradiance.b;//emissive multiplier
        
        PS_OUTPUT output;
        output.Colour = float4(c, 1);
        output.Depth = depth;
        return output;
    }
    
    float3 wpos = camRel + CameraPos.xyz;
    
    LODLight lodlight = LODLights[input.IID];
    float3 srpos = lodlight.Position - wpos; //light position relative to surface position
    float ldist = length(srpos);
    if (ldist > lodlight.Falloff) discard; //out of range of the light... TODO: capsules!
    if (ldist <= 0) discard;
    
    float4 rgbi = Unpack4x8UNF(lodlight.Colour).gbar;
    float3 lcol = rgbi.rgb * rgbi.a * 10.0f;
    
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
        if (ang > oang) discard;
        lamt *= saturate(1 - ((ang - iang) / (oang - iang)));
        lamt *= pow(saturate(1 - (ldist / lodlight.Falloff)), lodlight.FalloffExponent);
    }
    else if (LightType == 4)//capsule
    {
        lamt *= pow(saturate(1 - (ldist / lodlight.Falloff)), lodlight.FalloffExponent); //TODO! proper capsule lighting... (use point-line dist!)
    }
    
    pclit *= lamt;
    
    if (pclit <= 0) discard;
    
    float specb = saturate(dot(refl, ldir));
    float specp = max(exp(specb * 10) - 1, 0);
    float3 spec = lcol * (0.00006 * specp * specular.r * lamt);

    
    lcol = lcol * diffuse.rgb * pclit + spec;
    
    
    output.Colour =  float4(lcol, 0.5);
    return output;
    
    //return float4(diffuse.rgb, 1);
    //return float4(normal.rgb, 1);
    //return float4(specular.rgb, 1);
    //return float4(irradiance.rgb, 1);
}

