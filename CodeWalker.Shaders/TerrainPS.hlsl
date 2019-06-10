#include "Shadowmap.hlsli"

Texture2D<float4> Colourmap0 : register(t0);
Texture2D<float4> Colourmap1 : register(t2);
Texture2D<float4> Colourmap2 : register(t3);
Texture2D<float4> Colourmap3 : register(t4);
Texture2D<float4> Colourmap4 : register(t5);
Texture2D<float4> Colourmask : register(t6);
Texture2D<float4> Normalmap0 : register(t7);
Texture2D<float4> Normalmap1 : register(t8);
Texture2D<float4> Normalmap2 : register(t9);
Texture2D<float4> Normalmap3 : register(t10);
Texture2D<float4> Normalmap4 : register(t11);
SamplerState TextureSS : register(s0);


cbuffer PSSceneVars : register(b0)
{
    ShaderGlobalLightParams GlobalLights;
    uint EnableShadows;
    uint RenderMode; //0=default, 1=normals, 2=tangents, 3=colours, 4=texcoords, 5=diffuse, 6=normalmap, 7=spec, 8=direct
    uint RenderModeIndex; //colour or texcoord index
    uint RenderSamplerCoord;
}
cbuffer PSEntityVars : register(b2)
{
    uint EnableTexture0;
    uint EnableTexture1;
    uint EnableTexture2;
    uint EnableTexture3;
    uint EnableTexture4;
    uint EnableTextureMask;
    uint EnableNormalMap;
    uint ShaderName;
    uint EnableTint;
    uint EnableVertexColour;
    float bumpiness;
    uint Pad102;
}


struct VS_OUTPUT
{
    float4 Position : SV_POSITION;
    float3 Normal   : NORMAL;
    float4 Colour0   : COLOR0;
    float4 Colour1   : COLOR1;
    float4 Tint      : COLOR2;
    float2 Texcoord0 : TEXCOORD0;
    float2 Texcoord1 : TEXCOORD1;
    float2 Texcoord2 : TEXCOORD2;
    float4 Shadows   : TEXCOORD4;
    float4 LightShadow : TEXCOORD5;
    float4 Tangent     : TEXCOORD6;
    float4 Bitangent   : TEXCOORD7;
    float3 CamRelPos   : TEXCOORD8;
};



float4 main(VS_OUTPUT input) : SV_TARGET
{
    float4 vc0 = input.Colour0;
    float4 vc1 = input.Colour1;
    float2 tc0 = input.Texcoord0;
    float2 tc1 = input.Texcoord1;
    float2 tc2 = input.Texcoord2;

    float2 sc0 = tc0;
    float2 sc1 = tc0;
    float2 sc2 = tc0;
    float2 sc3 = tc0;
    float2 sc4 = tc0;
    float2 scm = tc1;

    ////switch (ShaderName)
    ////{
    ////    case 3965214311: //terrain_cb_w_4lyr_cm_pxm_tnt  vt: PNCTTTX_3 //vb_35_beache
    ////    case 4186046662: //terrain_cb_w_4lyr_cm_pxm  vt: PNCTTTX_3 //cs6_08_struct08
    ////        //vc1 = vc0;
    ////        //sc1 = tc0*25;
    ////        //sc2 = sc1;
    ////        //sc3 = sc1;
    ////        //sc4 = sc1;
    ////        //scm = tc0;
    ////        break;
    ////}

    float4 bc0 = float4(0.5, 0.5, 0.5, 1);
    //if (EnableVertexColour)
    //{
    //    bc0 = vc0;
    //}

    if (RenderMode == 8) //direct texture - choose texcoords
    {
        if (RenderSamplerCoord == 2) sc0 = tc1;
        else if (RenderSamplerCoord == 3) sc0 = tc2;
    }


    float4 c0 = (EnableTexture0 == 1) ? Colourmap0.Sample(TextureSS, sc0) : bc0;
    float4 c1 = (EnableTexture1 == 1) ? Colourmap1.Sample(TextureSS, sc1) : bc0;
    float4 c2 = (EnableTexture2 == 1) ? Colourmap2.Sample(TextureSS, sc2) : bc0;
    float4 c3 = (EnableTexture3 == 1) ? Colourmap3.Sample(TextureSS, sc3) : bc0;
    float4 c4 = (EnableTexture4 == 1) ? Colourmap4.Sample(TextureSS, sc4) : bc0;
    float4 m = (EnableTextureMask == 1) ? Colourmask.Sample(TextureSS, scm) : vc1;
    float4 b0 = (EnableNormalMap == 1) ? Normalmap0.Sample(TextureSS, sc0) : float4(0.5, 0.5, 0.5, 1);// float4(input.Normal, 0);
    float4 b1 = (EnableNormalMap == 1) ? Normalmap1.Sample(TextureSS, sc1) : b0;
    float4 b2 = (EnableNormalMap == 1) ? Normalmap2.Sample(TextureSS, sc2) : b0;
    float4 b3 = (EnableNormalMap == 1) ? Normalmap3.Sample(TextureSS, sc3) : b0;
    float4 b4 = (EnableNormalMap == 1) ? Normalmap4.Sample(TextureSS, sc4) : b0;

    float4 tv=0, nv=0;
    float4 t1, t2, n1, n2;

    switch (ShaderName)
    {
    case 137526804: //terrain_cb_w_4lyr_lod  vt: PNCCT //brdgeplatform_01_lod..
        //return float4(vc1.rgb, vc1.a*0.5 + 0.5);
        t1 = c1*(1 - vc1.b) + c2*vc1.b;
        t2 = c3*(1 - vc1.b) + c4*vc1.b;
        tv = t1*(1 - vc1.g) + t2*vc1.g;
        n1 = b1*(1 - vc1.b) + b2*vc1.b;
        n2 = b3*(1 - vc1.b) + b4*vc1.b;
        nv = n1*(1 - vc1.g) + n2*vc1.g;
        break;

    default:
    case 2535953532: //terrain_cb_w_4lyr_2tex_blend_lod  vt: PNCCTT //cs1_12_riverbed1_lod..
        //return float4(vc0.rgb, vc0.a*0.5 + 0.5);
        //return float4(vc1.rgb, vc1.a*0.5 + 0.5);
        vc1 = m*(1 - vc0.a) + vc1*vc0.a;
        t1 = c1*(1 - vc1.b) + c2*vc1.b;
        t2 = c3*(1 - vc1.b) + c4*vc1.b;
        tv = t1*(1 - vc1.g) + t2*vc1.g;
        n1 = b1*(1 - vc1.b) + b2*vc1.b;
        n2 = b3*(1 - vc1.b) + b4*vc1.b;
        nv = n1*(1 - vc1.g) + n2*vc1.g;
        break;


    case 653544224: //terrain_cb_w_4lyr_2tex_blend_pxm_spm  vt: PNCCTTTX //ch2_04_land06, rd_04_20..
    case 2486206885: //terrain_cb_w_4lyr_2tex_blend_pxm  vt: PNCCTTTX //cs2_06c_lkbed_05..
    case 1888432890: //terrain_cb_w_4lyr_2tex_pxm  vt: PNCCTTTX //ch1_04b_vineland01..
        //return float4(0, 1, 0, 1);
        vc1 = m*(1 - vc0.a) + vc1*vc0.a; //perhaps another sampling of the mask is needed here
        t1 = c1*(1 - vc1.b) + c2*vc1.b;
        t2 = c3*(1 - vc1.b) + c4*vc1.b;
        tv = t1*(1 - vc1.g) + t2*vc1.g;
        n1 = b1*(1 - vc1.b) + b2*vc1.b;
        n2 = b3*(1 - vc1.b) + b4*vc1.b;
        nv = n1*(1 - vc1.g) + n2*vc1.g;
        break;


    case 3051127652: //terrain_cb_w_4lyr  vt: PNCCTX //ss1_05_gr..
    case 646532852: //terrain_cb_w_4lyr_spec  vt: PNCCTX //hw1_07_grnd_c..
        //return float4(1, 1, 0, 1);
        vc1 = m*(1 - vc0.a) + vc1*vc0.a; //perhaps another sampling of the mask is needed here
        t1 = c1*(1 - vc1.b) + c2*vc1.b;
        t2 = c3*(1 - vc1.b) + c4*vc1.b;
        tv = t1*(1 - vc1.g) + t2*vc1.g;
        n1 = b1*(1 - vc1.b) + b2*vc1.b;
        n2 = b3*(1 - vc1.b) + b4*vc1.b;
        nv = n1*(1 - vc1.g) + n2*vc1.g;
        break;


    case 2316006813: //terrain_cb_w_4lyr_2tex_blend  vt: PNCCTTX //ch2_04_land02b, vb_34_beachn_01..
    case 3112820305: //terrain_cb_w_4lyr_2tex  vt: PNCCTTX //ch1_05_land5..
    case 2601000386: //terrain_cb_w_4lyr_spec_pxm  vt: PNCCTTX_2 //ch2_03_land05, grnd_low2.. _road
    case 4105814572: //terrain_cb_w_4lyr_pxm  vt: PNCCTTX_2 //ch2_06_house02.. vb_35_beache
    case 3400824277: //terrain_cb_w_4lyr_pxm_spm  vt: PNCCTTX_2 //ch2_04_land02b, ch2_06_terrain01a .. vb_35_beacha
        //return float4(1, 1, 1, 1);
        vc1 = m*(1 - vc0.a) + vc1*vc0.a; //perhaps another sampling of the mask is needed here
        t1 = c1*(1 - vc1.b) + c2*vc1.b;
        t2 = c3*(1 - vc1.b) + c4*vc1.b;
        tv = t1*(1 - vc1.g) + t2*vc1.g;
        n1 = b1*(1 - vc1.b) + b2*vc1.b;
        n2 = b3*(1 - vc1.b) + b4*vc1.b;
        nv = n1*(1 - vc1.g) + n2*vc1.g;
        break;


    case 295525123: //terrain_cb_w_4lyr_cm  vt: PNCTTX //_prewtrproxy_2..
    case 417637541: //terrain_cb_w_4lyr_cm_tnt  vt: PNCTTX //_prewtrproxy_2..  //golf course..
        //tv = 1;// c1;// *vc0.r; //TODO!
        //nv = b0;
        vc1 = m; //needs work!
        t1 = c1*(1 - vc1.b) + c2*vc1.b;
        t2 = c3*(1 - vc1.b) + c4*vc1.b;
        tv = t1*(1 - vc1.g) + t2*vc1.g;
        n1 = b1*(1 - vc1.b) + b2*vc1.b;
        n2 = b3*(1 - vc1.b) + b4*vc1.b;
        nv = n1*(1 - vc1.g) + n2*vc1.g;
        break;

    case 3965214311: //terrain_cb_w_4lyr_cm_pxm_tnt  vt: PNCTTTX_3 //vb_35_beache
    case 4186046662: //terrain_cb_w_4lyr_cm_pxm  vt: PNCTTTX_3 //cs6_08_struct08
        //m = min(m, vc0);
        //return float4(m.rgb, m.a*0.5 + 0.5);
        //return float4(vc0.rgb, vc0.a*0.5 + 0.5);
        //return float4(0, 1, 1, 1);
        //m = vc0;
        vc1 = m; //needs work!
        t1 = c1*(1 - vc1.b) + c2*vc1.b;
        t2 = c3*(1 - vc1.b) + c4*vc1.b;
        tv = t1*(1 - vc1.g) + t2*vc1.g;
        n1 = b1*(1 - vc1.b) + b2*vc1.b;
        n2 = b3*(1 - vc1.b) + b4*vc1.b;
        nv = n1*(1 - vc1.g) + n2*vc1.g;
        break;

    }
    if (EnableTint == 1)
    {
        tv.rgb *= input.Tint.rgb;
    }


    if (RenderMode == 1) //normals
    {
        tv.rgb = normalize(input.Normal)*0.5+0.5;
    }
    else if (RenderMode == 2) //tangents
    {
        tv.rgb = normalize(input.Tangent.xyz)*0.5+0.5;
    }
    else if (RenderMode == 3) //colours
    {
        tv.rgb = input.Colour0.rgb;
        if (RenderModeIndex == 2) tv.rgb = input.Colour1.rgb;
    }
    else if (RenderMode == 4) //texcoords
    {
        tv.rgb = float3(input.Texcoord0, 0);
        if (RenderModeIndex == 2) tv.rgb = float3(input.Texcoord1, 0);
        if (RenderModeIndex == 3) tv.rgb = float3(input.Texcoord2, 0);
    }
    else if (RenderMode == 5) //render diffuse maps
    {
        //nothing to do here yet, diffuse maps rendered by default
    }
    else if (RenderMode == 6) //render normalmaps
    {
        tv.rgb = nv.rgb;
    }
    else if (RenderMode == 7) //render spec maps
    {
        tv.rgb = 0.5; //nothing to see here yet...
    }
    else if (RenderMode == 8) //render direct texture
    {
        tv = c0;
    }
    
    //nv = normalize(nv*2-1);
    //float4 tang = input.Tangent;
    float3 nz = normalize(input.Normal);
    //float3 nx = normalize(tang.xyz);
    //float3 ny = normalize(cross(nz, nx));
    ////float3 norm = normalize(nx*nv.x + ny*nv.y + nz*nv.z);
    float3 norm = nz;// normalize(input.Normal)


    if ((RenderMode == 0) && (EnableNormalMap == 1))
    {
        norm = NormalMap(nv.xy, bumpiness, input.Normal.xyz, input.Tangent.xyz, input.Bitangent.xyz);
    }

    float3 spec = 0;

    tv.rgb = FullLighting(tv.rgb, spec, norm, vc0, GlobalLights, EnableShadows, input.Shadows.x, input.LightShadow);


    return float4(tv.rgb, saturate(tv.a));
}