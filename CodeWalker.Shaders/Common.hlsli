


struct ShaderGlobalLightParams
{
    float3 LightDir;
    float LightHdr; //global intensity
    float4 LightDirColour;
    float4 LightDirAmbColour;
    float4 LightNaturalAmbUp;
    float4 LightNaturalAmbDown;
    float4 LightArtificialAmbUp;
    float4 LightArtificialAmbDown;
};





//for unpacking colours etc
uint4 Unpack4x8(uint v)
{
    return uint4(v >> 24, v >> 16, v >> 8, v) & 0xFF;
}
float4 Unpack4x8UNF(uint v)
{
    float4 u = (float4)Unpack4x8(v);
    return u*0.0039215686274509803921568627451f;// u * 1/255
}



float DepthFunc(float2 zw)
{
    return zw.x;

	////this sort of works for reverse depth buffering, but has issues with vertices behind the near clip plane.
	////might need to adjust the viewproj matrix to fix that...
	////(for this to work, also need to change GpuBuffers.Clear,.ClearDepth and ShaderManager DepthComparison,RenderFinalPass)
	//return max(0.001 / zw.x, 0);




    //return zw.x * (0.1 + 0.00001*(abs(zw.y)));
    //return zw.x * (0.1 + 0.00001*((zw.y)));



    //const float far = 1000.0; //outerra version - needs logz written to frag depth in PS...
    //const float C = 0.01; //~10m linearization
    //const float FC = 1.0/log(far*C + 1);  
    //////logz = gl_Position.w*C + 1;  //version with fragment code 
    ////logz = log(gl_Position.w*C + 1)*FC;
    ////gl_Position.z = (2*logz - 1)*gl_Position.w;
    //float logz = log(zw.y*C + 1)*FC;
    //return (2*logz - 1)*zw.y;

}






float3 GeomWindMotion(float3 ipos, float3 vc0, float4 windvec, float4 overrideparams)
{

    //lt r1.x, r0.x, l(1.000000)
    //mul r1.yzw, v2.xxxz, cb12[0].xxxy //umGlobalParams
    //mul r1.yzw, r1.yyzw, cb9[13].xxxy //umGlobalOverrideParams
    //add r2.x, v2.y, cb9[0].w          //_worldPlayerPos_umGlobalPhaseShift
    //mul r2.x, |r2.x|, l(6.283185)
    //mul r2.yzw, cb9[13].zzzw, cb12[0].zzzw  //umGlobalOverrideParams, umGlobalParams
    //mad r2.xyz, cb2[12].xxxx, r2.yzwy, r2.xxxx  //globalScalars2
    //sincos r2.xyz, null, r2.xyzx
    //mad r1.yzw, r2.xxyz, r1.yyzw, v0.xxyz
    //movc r1.xyz, r1.xxxx, r1.yzwy, v0.xyzx
    //add r1.w, -r0.x, l(1.000000)
    //mul r0.xyz, r0.yzwy, r0.xxxx
    //mad r0.xyz, r1.wwww, r1.xyzx, r0.xyzx
    //mul r1.xyzw, r0.yyyy, cb1[9].xyzw
    //mad r1.xyzw, r0.xxxx, cb1[8].xyzw, r1.xyzw
    //mad r0.xyzw, r0.zzzz, cb1[10].xyzw, r1.xyzw
    //add o0.xyzw, r0.xyzw, cb1[11].xyzw    //screen pos out
    //mov o1.xy, v4.xyxx

    float3 f1 = vc0.xxz * windvec.xxy * overrideparams.xxy;
    float phase = vc0.y + 0.0; //playerpos/global phase shift?
    float phrad = abs(phase)*6.283185;
    float3 f2 = windvec.zzw * overrideparams.zzw + phrad; //globalScalars2
    f2 = sin(f2);
    f1 = f2*f1 + ipos;
    return f1;

    //return ipos;
}




float3 NormalMap(float2 nmv, float bumpinezz, float3 norm, float3 tang, float3 bita)
{
    //r1 = nmv; //sample r1.xyzw, v2.xyxx, t2.xyzw, s2  (BumpSampler)
    //float bmp = max(bumpinezz, 0.001);   //max r0.x, bumpiness, l(0.001000)
    float2 nxy = nmv.xy * 2 - 1;          //mad r0.yz, r1.xxyx, l(0.000000, 2.000000, 2.000000, 0.000000), l(0.000000, -1.000000, -1.000000, 0.000000)
    float2 bxy = nxy * max(bumpinezz, 0.001);          //mul r0.xw, r0.xxxx, r0.yyyz
    float bxyz = sqrt(abs(1 - dot(nxy, nxy)));    //r0.y = dot(nxy, nxy);       //dp2 r0.y, r0.yzyy, r0.yzyy  //r0.y = 1.0 - r0.y;              //add r0.y, -r0.y, l(1.000000)  //r0.y = sqrt(abs(r0.y));         //sqrt r0.y, |r0.y|
    float3 t1 = tang * bxy.x; //mad r0.xzw, r0.xxxx, v4.xxyz, r1.xxyz
    float3 t2 = bita * bxy.y + t1;    //mul r1.xyz, r0.wwww, v5.xyzx
    float3 t3 = norm * bxyz + t2; //mad r0.xyz, r0.yyyy, v3.xyzx, r0.xzwx
    return normalize(t3);
    //r0.w = dot(t3, t3);     //dp3 r0.w, r0.xyzx, r0.xyzx
    //r0.w = 1.0 / sqrt(r0.w);        //rsq r0.w, r0.w
    ////r1.x = r0.z*r0.w - 0.35;        //mad r1.x, r0.z, r0.w, l(-0.350000)
    //t3 = t3*r0.w;           //mul r0.xyz, r0.wwww, r0.xyzx
    ////mad o1.xyz, t3.xyzx, l(0.500000, 0.500000, 0.500000, 0.000000), l(0.500000, 0.500000, 0.500000, 0.000000)
    //return t3;
}


// POM constants
#define POM_MIN_STEPS 3
#define POM_MAX_STEPS 16
#define POM_VDOTN_BLEND_FACTOR 0.25f
#define POM_HEIGHT_SCALE 0.1f          // Global parallax strength multiplier (1.0 = full, 0.5 = half)

// Distance-based POM fade constants (reduces noise at steep angles/distance)
#define POM_DISTANCE_START 5.0f    // Distance where fade begins
#define POM_DISTANCE_END 50.0f     // Distance where POM is fully disabled

// Binary search refinement for more precise intersection (reduces ring artifacts at close range)
#define POM_BINARY_SEARCH_STEPS 5   // Number of binary search iterations after linear search
#define POM_CLOSE_DISTANCE 2.0f     // Distance threshold for close-range step boost
#define POM_CLOSE_STEP_MULTIPLIER 2.0f  // Step multiplier when very close to surface

// Distance fade lookup table (5 control points for smooth non-linear falloff)
// Based on GTA V pomWeights table
#define NUM_POM_CTRL_POINTS 5
static const float pomWeights[NUM_POM_CTRL_POINTS] = {
    1.0f,   // Full quality at close range
    0.9f,
    0.5f,   // 50% at mid distance
    0.1f,
    0.0f    // Disabled at far distance
};

// Compute smooth distance-based fade for POM steps
float ComputePOMDistanceFade(float distanceBlend)
{
    if (distanceBlend >= 1.0f)
        return 0.0f;

    // Find the nearest control points and interpolate
    int startPoint = clamp(int(distanceBlend * (NUM_POM_CTRL_POINTS - 1)), 0, NUM_POM_CTRL_POINTS - 2);
    int endPoint = startPoint + 1;

    float t = distanceBlend * (NUM_POM_CTRL_POINTS - 1) - float(startPoint);
    return lerp(pomWeights[startPoint], pomWeights[endPoint], t);
}

// Performs relief mapping by tracing through the height field
float TraceHeight(Texture2D<float4> heightMapSampler, SamplerState samplerState, float2 texCoords, float2 direction, float2 bias, int maxNumberOfSteps)
{
    if (maxNumberOfSteps == 0)
    {
        return 0.0f;
    }

    float heightStep = 1.0f / float(maxNumberOfSteps);
    float2 offsetPerStep = direction * heightStep;

    float currentBound = 1.0f;
    float previousBound = currentBound;

    float2 texCoordOffset = bias;

    // Use derivatives for proper mip selection to avoid aliasing
    float2 ddx0 = ddx(texCoords.xy);
    float2 ddy0 = ddy(texCoords.xy);

    float currentHeight = heightMapSampler.SampleGrad(samplerState, texCoords.xy, ddx0, ddy0).r + 1e-6f;
    float previousHeight = currentHeight;

    [unroll(POM_MAX_STEPS)]
    for (int s = 0; s < maxNumberOfSteps; ++s)
    {
        if (currentHeight < currentBound)
        {
            previousBound = currentBound;
            previousHeight = currentHeight;

            currentBound -= heightStep;
            texCoordOffset += offsetPerStep;
            currentHeight = heightMapSampler.SampleGrad(samplerState, texCoords + texCoordOffset, ddx0, ddy0).r;
        }
        else
        {
            break;
        }
    }

    // Interpolate between the two points to find a more precise height
    float currentDelta = currentBound - currentHeight;
    float previousDelta = previousBound - previousHeight;
    float denominator = previousDelta - currentDelta;

    float finalHeight = currentHeight;

    if (denominator > 0)
    {
        finalHeight = ((currentBound * previousDelta) - (previousBound * currentDelta)) / denominator;
    }

    return clamp(finalHeight, 0.0, 1.0f);
}

// Parallax self-shadow: traces through heightmap in light direction to find occlusion
// Returns shadow factor (0 = fully lit, 1 = fully in shadow)
float TraceSelfShadow(Texture2D<float4> heightMapSampler, SamplerState samplerState, float2 texCoords, float3 tanLightDir, float edgeWeight, float hScale)
{
    float2 inXY = (tanLightDir.xy * hScale * edgeWeight) / max(tanLightDir.z, 0.01f);

    // Sample base height at current (displaced) position
    float sh0 = heightMapSampler.SampleLevel(samplerState, texCoords, 0).r;

    // Trace 7 samples along light direction with increasing weight for closer occlusion
    float shA = (heightMapSampler.SampleLevel(samplerState, texCoords + inXY * 0.88, 0).r - sh0 - 0.88) *  1;
    float sh9 = (heightMapSampler.SampleLevel(samplerState, texCoords + inXY * 0.77, 0).r - sh0 - 0.77) *  2;
    float sh8 = (heightMapSampler.SampleLevel(samplerState, texCoords + inXY * 0.66, 0).r - sh0 - 0.66) *  4;
    float sh7 = (heightMapSampler.SampleLevel(samplerState, texCoords + inXY * 0.55, 0).r - sh0 - 0.55) *  6;
    float sh6 = (heightMapSampler.SampleLevel(samplerState, texCoords + inXY * 0.44, 0).r - sh0 - 0.44) *  8;
    float sh5 = (heightMapSampler.SampleLevel(samplerState, texCoords + inXY * 0.33, 0).r - sh0 - 0.33) * 10;
    float sh4 = (heightMapSampler.SampleLevel(samplerState, texCoords + inXY * 0.22, 0).r - sh0 - 0.22) * 12;

    float finalHeight = max(max(max(max(max(max(shA, sh9), sh8), sh7), sh6), sh5), sh4);
    return saturate(finalHeight);
}

#define PARALLAX_SELF_SHADOW_AMOUNT 0.95f

// Calculate parallax texture coordinate offset
float2 ParallaxOffset(Texture2D<float4> heightMapSampler, SamplerState samplerState, float2 texCoords,
                       float3 viewDir, float3 normal, float3 tangent, float3 bitangent,
                       float inHeightScale, float inHeightBias)
{
    // Transform view direction to tangent space
    float3 tanEyePos;
    tanEyePos.x = dot(tangent.xyz, viewDir.xyz);
    tanEyePos.y = dot(bitangent.xyz, viewDir.xyz);
    tanEyePos.z = dot(normal.xyz, viewDir.xyz);
    tanEyePos = normalize(tanEyePos);

    // Clamp Z to avoid division issues at grazing angles
    float zLimit = 0.1f;
    float clampedZ = max(zLimit, tanEyePos.z);

    // Calculate view-dependent step count for quality/performance balance
    float VdotN = abs(dot(normalize(viewDir.xyz), normalize(normal.xyz)));
    float numberOfSteps = lerp(POM_MAX_STEPS, POM_MIN_STEPS, VdotN);

    // Apply global scale based on view angle for smooth falloff
    float globalScale = saturate(numberOfSteps - 1.0f) * saturate(VdotN / POM_VDOTN_BLEND_FACTOR);

    // Calculate max parallax offset and bias offset
    float2 maxParallaxOffset = (-tanEyePos.xy / clampedZ) * inHeightScale * globalScale;
    float2 heightBiasOffset = (tanEyePos.xy / clampedZ) * inHeightBias * globalScale;

    // Trace through height field
    float height = TraceHeight(heightMapSampler, samplerState, texCoords, maxParallaxOffset, heightBiasOffset, (int)numberOfSteps);

    // Calculate final texture coordinate offset
    float2 texCoordOffset = heightBiasOffset + (maxParallaxOffset * (1.0f - height));

    return texCoordOffset;
}




float3 BasicLighting(float4 lightcolour, float4 ambcolour, float pclit)
{
    return (ambcolour.rgb + lightcolour.rgb*pclit);
}

float3 AmbientLight(float3 diff, float normz, float4 upcolour, float4 downcolour, float amount)
{
    float bf = normz*0.5 + 0.5;
    float3 upval = upcolour.rgb*saturate(1.0-bf);
    float3 downval = downcolour.rgb*saturate(bf);
    return diff*(upval + downval)*amount;
    //return (float3)0;
}

float3 GlobalLighting(float3 diff, float3 norm, float4 vc0, float lf, uniform ShaderGlobalLightParams globalLights)
{
    float3 c = saturate(diff);
    float3 fc = c;
    float naturalDiffuseFactor = vc0.r;
    float artificialDiffuseFactor = saturate(vc0.g);
    c *= BasicLighting(globalLights.LightDirColour, globalLights.LightDirAmbColour, lf);
    c += AmbientLight(fc, norm.z, globalLights.LightNaturalAmbUp, globalLights.LightNaturalAmbDown, naturalDiffuseFactor);
    c += AmbientLight(fc, norm.z, globalLights.LightArtificialAmbUp, globalLights.LightArtificialAmbDown, artificialDiffuseFactor);
    return c;
}





























