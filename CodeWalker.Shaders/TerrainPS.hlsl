#include "TerrainPS.hlsli"


// Sample and blend terrain height maps based on layer weights
// Returns inverted height (1.0 - sample) for correct POM ray marching
float BlendTerrainHeight(float4 layerBlends, float2 texCoord)
{
    float result = 0.0f;
    result += layerBlends.x * Heightmap0.SampleLevel(TextureSS, texCoord, 0).r;
    result += layerBlends.y * Heightmap1.SampleLevel(TextureSS, texCoord, 0).r;
    result += layerBlends.z * Heightmap2.SampleLevel(TextureSS, texCoord, 0).r;
    result += layerBlends.w * Heightmap3.SampleLevel(TextureSS, texCoord, 0).r;
    // Invert height: GTA V height maps use 1.0=raised, 0.0=base
    // POM ray march expects 0.0=raised (hit early), 1.0=base (hit late)
    return 1.0f - result;
}

// Get blended height scale and bias based on layer weights
float2 GetBlendedScaleBias(float4 layerBlends)
{
    float2 result = float2(0.0f, 0.0f);
    result += layerBlends.x * float2(heightScale.x, heightBias.x);
    result += layerBlends.y * float2(heightScale.y, heightBias.y);
    result += layerBlends.z * float2(heightScale.z, heightBias.z);
    result += layerBlends.w * float2(heightScale.w, heightBias.w);
    return result;
}

// Calculate terrain parallax offset using blended heights
// Includes distance-based fade and vertex edge weight to reduce noise at steep angles and mesh edges
float2 CalculateTerrainParallaxOffset(float4 layerBlends, float2 texCoord, float3 viewDir, float3 normal, float3 tangent, float3 bitangent, float viewDistance, float2 edgeWeightData)
{
    // Get blended scale and bias
    float2 scaleBias = GetBlendedScaleBias(layerBlends);
    float hScale = scaleBias.x * POM_HEIGHT_SCALE;  // Apply global strength multiplier
    float hBias = scaleBias.y * POM_HEIGHT_SCALE;

    if (hScale == 0.0f)
        return float2(0.0f, 0.0f);

    // Vertex edge weight from mesh data
    // edgeWeightData.x: 0 = full POM, 1 = no POM (at mesh edges)
    // edgeWeightData.y: controls dynamic zLimit adjustment
    float vertexEdgeWeight = 1.0f - saturate(edgeWeightData.x);

    // Early out if vertex says no POM at this point
    if (vertexEdgeWeight <= 0.0f)
        return float2(0.0f, 0.0f);

    // Transform view direction to tangent space
    float3 tanEyePos;
    tanEyePos.x = dot(tangent.xyz, viewDir.xyz);
    tanEyePos.y = dot(bitangent.xyz, viewDir.xyz);
    tanEyePos.z = dot(normal.xyz, viewDir.xyz);
    tanEyePos = normalize(tanEyePos);

    // Dynamic zLimit from vertex edge weight
    // Higher edgeWeightData.y = lower zLimit = allow steeper angles
    float zLimit = 1.0f - clamp(edgeWeightData.y, 0.1f, 1.0f);
    zLimit = max(zLimit, 0.1f); // Ensure minimum zLimit
    float clampedZ = max(zLimit, tanEyePos.z);

    // Calculate view-dependent step count
    float VdotN = abs(dot(normalize(viewDir.xyz), normalize(normal.xyz)));
    float numberOfSteps = lerp(POM_MAX_STEPS, POM_MIN_STEPS, VdotN);

    // Close-range step boost - increase precision when very close to surface (reduces ring artifacts)
    float closeBoost = saturate(1.0f - viewDistance / POM_CLOSE_DISTANCE);
    numberOfSteps *= lerp(1.0f, POM_CLOSE_STEP_MULTIPLIER, closeBoost);

    // Distance-based fade - reduces noise at steep angles/far distances
    float distanceBlend = saturate((viewDistance - POM_DISTANCE_START) / (POM_DISTANCE_END - POM_DISTANCE_START));
    float distanceFade = ComputePOMDistanceFade(distanceBlend);

    // Reduce steps over distance - artifacts become less noticeable at distance
    numberOfSteps *= distanceFade;

    // Early out if steps reduced to nearly zero
    if (numberOfSteps < 1.0f)
        return float2(0.0f, 0.0f);

    // Calculate weight distance blend for smooth fade-out near zero steps
    float scaleOutRange = (POM_DISTANCE_END - POM_DISTANCE_START) * 0.35f;
    float weightDistanceBlend = saturate(((viewDistance - POM_DISTANCE_START) - (POM_DISTANCE_END - POM_DISTANCE_START) + scaleOutRange) / scaleOutRange);

    // Combined edge weight: vertex edge * view angle fade * step count fade * distance fade
    float edgeWeight = vertexEdgeWeight * (1.0f - weightDistanceBlend);
    edgeWeight *= saturate(numberOfSteps - 1.0f) * saturate(VdotN / POM_VDOTN_BLEND_FACTOR);

    // Apply combined scale
    float globalScale = edgeWeight;

    float2 maxParallaxOffset = (-tanEyePos.xy / clampedZ) * hScale * globalScale;
    float2 heightBiasOffset = (tanEyePos.xy / clampedZ) * hBias * globalScale;

    float heightStep = 1.0f / max(numberOfSteps, 1.0f);
    float2 offsetPerStep = maxParallaxOffset * heightStep;

    float currentHeight = 1.0f;
    float previousHeight = currentHeight;

    float2 texCoordOffset = heightBiasOffset;

    float terrainHeight = BlendTerrainHeight(layerBlends, texCoord) + 1e-6f;
    float previousTerrainHeight = terrainHeight;

    // Ray march through the height field
    int maxSteps = (int)numberOfSteps;
    for (int i = 0; i < maxSteps; ++i)
    {
        if (terrainHeight < currentHeight)
        {
            previousHeight = currentHeight;
            previousTerrainHeight = terrainHeight;

            currentHeight -= heightStep;
            texCoordOffset += offsetPerStep;
            terrainHeight = BlendTerrainHeight(layerBlends, texCoord + texCoordOffset);
        }
        else
        {
            break;
        }
    }

    // Binary search refinement for more precise intersection (reduces ring artifacts at close range)
    float2 prevOffset = texCoordOffset - offsetPerStep;
    float2 currOffset = texCoordOffset;
    float prevHeight = previousHeight;
    float currHeight = currentHeight;

    [unroll(POM_BINARY_SEARCH_STEPS)]
    for (int j = 0; j < POM_BINARY_SEARCH_STEPS; ++j)
    {
        float2 midOffset = (prevOffset + currOffset) * 0.5f;
        float midHeight = (prevHeight + currHeight) * 0.5f;
        float midTerrainHeight = BlendTerrainHeight(layerBlends, texCoord + midOffset);

        if (midTerrainHeight < midHeight)
        {
            // Intersection is in second half
            prevOffset = midOffset;
            prevHeight = midHeight;
        }
        else
        {
            // Intersection is in first half
            currOffset = midOffset;
            currHeight = midHeight;
        }
    }

    // Final interpolation between the refined bracket
    float finalTerrainHeight = BlendTerrainHeight(layerBlends, texCoord + currOffset);
    float currentDelta = currHeight - finalTerrainHeight;
    float previousDelta = prevHeight - BlendTerrainHeight(layerBlends, texCoord + prevOffset);
    float denominator = previousDelta - currentDelta;

    float refinedHeight = 1.0f;
    if (abs(denominator) > 1e-6f)
    {
        refinedHeight = (currHeight * previousDelta - prevHeight * currentDelta) / denominator;
    }
    else
    {
        refinedHeight = 1.0f - (currOffset.x / maxParallaxOffset.x);
    }

    return heightBiasOffset + (maxParallaxOffset * (1.0f - saturate(refinedHeight)));
}

// Terrain-specific self-shadow using blended heightmaps
float TraceTerrainSelfShadow(float4 layerBlends, float2 texCoords, float3 tanLightDir, float edgeWeight, float hScale)
{
    float2 inXY = (tanLightDir.xy * hScale * edgeWeight) / max(tanLightDir.z, 0.01f);

    float sh0 = 1.0f - BlendTerrainHeight(layerBlends, texCoords);

    float shA = (1.0f - BlendTerrainHeight(layerBlends, texCoords + inXY * 0.88) - sh0 - 0.88) *  1;
    float sh9 = (1.0f - BlendTerrainHeight(layerBlends, texCoords + inXY * 0.77) - sh0 - 0.77) *  2;
    float sh8 = (1.0f - BlendTerrainHeight(layerBlends, texCoords + inXY * 0.66) - sh0 - 0.66) *  4;
    float sh7 = (1.0f - BlendTerrainHeight(layerBlends, texCoords + inXY * 0.55) - sh0 - 0.55) *  6;
    float sh6 = (1.0f - BlendTerrainHeight(layerBlends, texCoords + inXY * 0.44) - sh0 - 0.44) *  8;
    float sh5 = (1.0f - BlendTerrainHeight(layerBlends, texCoords + inXY * 0.33) - sh0 - 0.33) * 10;
    float sh4 = (1.0f - BlendTerrainHeight(layerBlends, texCoords + inXY * 0.22) - sh0 - 0.22) * 12;

    float finalHeight = max(max(max(max(max(max(shA, sh9), sh8), sh7), sh6), sh5), sh4);
    return saturate(finalHeight);
}

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

    // Calculate layer blend weights from vertex colors (4-layer blending)
    // Layer weights: x=(1-g)*(1-b), y=(1-g)*b, z=g*(1-b), w=g*b
    float4 layerBlends;
    layerBlends.x = (1.0f - vc1.g) * (1.0f - vc1.b);
    layerBlends.y = (1.0f - vc1.g) * vc1.b;
    layerBlends.z = vc1.g * (1.0f - vc1.b);
    layerBlends.w = vc1.g * vc1.b;

    // Calculate single parallax offset using blended heights
    float parallaxSelfShadow = 1.0;
    if (EnableHeightMap && RenderMode == 0)
    {
        float3 viewDir = -normalize(input.CamRelPos); // Negate to get direction FROM surface TO camera
        float3 norm = normalize(input.Normal);
        float3 tang = normalize(input.Tangent.xyz);
        float3 bitang = normalize(input.Bitangent.xyz);

        // Calculate single offset from blended height values (with distance fade and edge weight)
        float2 parallaxOffset = CalculateTerrainParallaxOffset(layerBlends, tc0, viewDir, norm, tang, bitang, input.ViewDistance, input.EdgeWeight);

        // Apply same offset to all texture coordinates
        sc0 += parallaxOffset;
        sc1 += parallaxOffset;
        sc2 += parallaxOffset;
        sc3 += parallaxOffset;
        sc4 += parallaxOffset;

        // Parallax self-shadow, transform light dir to tangent space and trace blended heights
        float3 tanLightDir;
        tanLightDir.x = dot(tang, GlobalLights.LightDir.xyz);
        tanLightDir.y = dot(bitang, GlobalLights.LightDir.xyz);
        tanLightDir.z = dot(norm, GlobalLights.LightDir.xyz);
        float2 blendedScaleBias = GetBlendedScaleBias(layerBlends);
        float blendedHScale = blendedScaleBias.x * POM_HEIGHT_SCALE;
        float edgeWeight = 1.0f - saturate(input.EdgeWeight.x);
        float shadowAmount = TraceTerrainSelfShadow(layerBlends, sc0, tanLightDir, edgeWeight, blendedHScale);
        parallaxSelfShadow = 1.0 - shadowAmount * PARALLAX_SELF_SHADOW_AMOUNT;
    }

    float4 bc0 = float4(0.5, 0.5, 0.5, 1);

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

    tv.rgb = FullLighting(tv.rgb, spec, norm, vc0, GlobalLights, EnableShadows, input.Shadows.x, input.LightShadow, parallaxSelfShadow);


    return float4(tv.rgb, saturate(tv.a));
}