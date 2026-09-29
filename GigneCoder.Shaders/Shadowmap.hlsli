#include "Common.hlsli"

//Texture2DArray<float> Depthmap : register(t1);
//SamplerState DepthmapSS : register(s1);
Texture2D Depthmap : register(t1);
SamplerComparisonState DepthmapSS : register(s1);

cbuffer ShadowmapVars : register(b1)
{
    float4 CamScenePos; //in shadow scene coords
    float4x4 CamSceneView;
    float4x4 LightView;
    float4 LightDir;
    float4 CascadeOffsets[16];
    float4 CascadeScales[16];
    float4 CascadeDepths[16]; //in scene eye space
    int CascadeCount;
    int CascadeVisual;
    int PCFLoopStart;
    int PCFLoopEnd;
    float BorderPaddingMin;
    float BorderPaddingMax;
    float Bias;
    float BlurBetweenCascades;
    float CascadeCountInv;
    float TexelSize;
    float TexelSizeX;
    float ShadowMaxDistance; //2000 or so
};



float ShadowmapSceneDepth(float3 camRelPos, out float4 lspos)
{
    float4 scenePos = float4(camRelPos + CamScenePos.xyz, 1.0);
    lspos = mul(scenePos, LightView);
    return mul(scenePos, CamSceneView).z;
}




//--------------------------------------------------------------------------------------
// Use PCF to sample the depth map and return a percent lit value.
//--------------------------------------------------------------------------------------
void ShadowmapCalculatePCFPercentLit(in float4 vShadowTexCoord,
    in float fRightTexelDepthDelta,
    in float fUpTexelDepthDelta,
    in float fBlurRowSize,
    in float fCascade,
    out float fPercentLit
)
{
    fPercentLit = 0.0f;
    // This loop could be unrolled, and texture immediate offsets could be used if the kernel size were fixed.
    // This would be performance improvment.
    for (int x = PCFLoopStart; x < PCFLoopEnd; ++x)
    {
        for (int y = PCFLoopStart; y < PCFLoopEnd; ++y)
        {
            float depthcompare = vShadowTexCoord.z;
            // A very simple solution to the depth bias problems of PCF is to use an offset.
            // Unfortunately, too much offset can lead to Peter-panning (shadows near the base of object disappear )
            // Too little offset can lead to shadow acne ( objects that should not be in shadow are partially self shadowed ).
            depthcompare -= Bias;
            //depthcompare += Bias;
            // Compare the transformed pixel depth to the depth read from the map.
            //fPercentLit += Depthmap.SampleLevel(DepthmapSS,
            fPercentLit += Depthmap.SampleCmpLevelZero(DepthmapSS,
                float2(
                    vShadowTexCoord.x + (((float)x) * TexelSizeX),
                    vShadowTexCoord.y + (((float)y) * TexelSize)//,
                    ), depthcompare);
                    //fCascade
                    //), 0).r > depthcompare ? 1.0 : 0.0;// , depthcompare);//== 1.0)?1:0;//
        }
    }
    fPercentLit /= (float)fBlurRowSize;
}


//--------------------------------------------------------------------------------------
// Calculate amount to blend between two cascades and the band where blending will occure.
//--------------------------------------------------------------------------------------
void ShadowmapCalculateBlendAmountForMap(in float4 vShadowMapTextureCoord,
    in out float fCurrentPixelsBlendBandLocation,
    out float fBlendBetweenCascadesAmount)
{
    // Calcaulte the blend band for the map based selection.
    float2 distanceToOne = float2 (1.0f - vShadowMapTextureCoord.x, 1.0f - vShadowMapTextureCoord.y);
    fCurrentPixelsBlendBandLocation = min(vShadowMapTextureCoord.x, vShadowMapTextureCoord.y);
    float fCurrentPixelsBlendBandLocation2 = min(distanceToOne.x, distanceToOne.y);
    fCurrentPixelsBlendBandLocation =
        min(fCurrentPixelsBlendBandLocation, fCurrentPixelsBlendBandLocation2);
    fBlendBetweenCascadesAmount = BlurBetweenCascades != 0.0 ? 
        (fCurrentPixelsBlendBandLocation / BlurBetweenCascades) : 0.0;
}

static const float4 vCascadeColorsMultiplier[8] =
{
    float4 (1.5f, 0.0f, 0.0f, 1.0f),
    float4 (0.0f, 1.5f, 0.0f, 1.0f),
    float4 (0.0f, 0.0f, 5.5f, 1.0f),
    float4 (1.5f, 0.0f, 5.5f, 1.0f),
    float4 (1.5f, 1.5f, 0.0f, 1.0f),
    float4 (1.0f, 1.0f, 1.0f, 1.0f),
    float4 (0.0f, 1.0f, 5.5f, 1.0f),
    float4 (0.5f, 3.5f, 0.75f, 1.0f)
};

void ShadowComputeCoordinatesTransform(in int iCascadeIndex, inout float4 vShadowTexCoord) 
{
    //transform X coord for current cascade.
    vShadowTexCoord.x *= CascadeCountInv;// m_fShadowPartitionSize;  // precomputed (float)iCascadeIndex / (float)CASCADE_CNT
    vShadowTexCoord.x += (CascadeCountInv*(float)iCascadeIndex);// (m_fShadowPartitionSize * (float)iCascadeIndex);
} 


float ShadowAmount(float4 shadowcoord, float shadowdepth)//, inout float4 colour)
{

    float4 vShadowMapTextureCoord = 0.0f;
    float4 vShadowMapTextureCoord_blend = 0.0f;

    float4 vVisualizeCascadeColor = float4(0.0f, 0.0f, 0.0f, 1.0f);

    float fPercentLit = 0.0f;
    float fPercentLit_blend = 0.0f;


    float fUpTextDepthWeight = 0;
    float fRightTextDepthWeight = 0;
    float fUpTextDepthWeight_blend = 0;
    float fRightTextDepthWeight_blend = 0;

    int iBlurRowSize = PCFLoopEnd - PCFLoopStart;
    iBlurRowSize *= iBlurRowSize;
    float fBlurRowSize = (float)iBlurRowSize;

    int iCascadeFound = 0;
    int iNextCascadeIndex = 1;

    // The interval based selection technique compares the pixel's depth against the frustum's cascade divisions.
    float fCurrentPixelDepth = shadowdepth;

    // This for loop is not necessary when the frustum is uniformaly divided and interval based selection is used.
    // In this case fCurrentPixelDepth could be used as an array lookup into the correct frustum. 
    int iCurrentCascadeIndex = 0;

    float4 vShadowMapTextureCoordViewSpace = shadowcoord;

    if (CascadeCount == 1)
    {
        vShadowMapTextureCoord = vShadowMapTextureCoordViewSpace * CascadeScales[0];
        vShadowMapTextureCoord += CascadeOffsets[0];
    }
    if (CascadeCount > 1) {
        for (int iCascadeIndex = 0; iCascadeIndex < CascadeCount && iCascadeFound == 0; ++iCascadeIndex)
        {
            vShadowMapTextureCoord = vShadowMapTextureCoordViewSpace * CascadeScales[iCascadeIndex];
            vShadowMapTextureCoord += CascadeOffsets[iCascadeIndex];

            if (min(vShadowMapTextureCoord.x, vShadowMapTextureCoord.y) > BorderPaddingMin
                && max(vShadowMapTextureCoord.x, vShadowMapTextureCoord.y) < BorderPaddingMax)
            {
                iCurrentCascadeIndex = iCascadeIndex;
                iCascadeFound = 1;
            }
        }
    }

    if (iCascadeFound == 0 || vShadowMapTextureCoord.z>=1)
    {
        //colour = float4(0.1, 0.3, 1.0, 0.5);
        return 1.0; //out of range!
    }
    else
    {
        ShadowComputeCoordinatesTransform(iCurrentCascadeIndex, vShadowMapTextureCoord);    


        vVisualizeCascadeColor = vCascadeColorsMultiplier[iCurrentCascadeIndex];
        //colour = vVisualizeCascadeColor;

        // Repeat texcoord calculations for the next cascade. 
        // The next cascade index is used for blurring between maps.
        iNextCascadeIndex = min(CascadeCount - 1, iCurrentCascadeIndex + 1);

        float fBlendBetweenCascadesAmount = 1.0f;
        float fCurrentPixelsBlendBandLocation = 1.0f;

        ShadowmapCalculateBlendAmountForMap(vShadowMapTextureCoord, fCurrentPixelsBlendBandLocation, fBlendBetweenCascadesAmount);


        ShadowmapCalculatePCFPercentLit(vShadowMapTextureCoord, fRightTextDepthWeight, fUpTextDepthWeight, fBlurRowSize, (float)iCurrentCascadeIndex, fPercentLit);

        if (CascadeCount > 1)
        {
            if (fCurrentPixelsBlendBandLocation < BlurBetweenCascades)
            {  // the current pixel is within the blend band.

               // Repeat texcoord calculations for the next cascade. 
               // The next cascade index is used for blurring between maps.
                vShadowMapTextureCoord_blend = vShadowMapTextureCoordViewSpace * CascadeScales[iNextCascadeIndex];
                vShadowMapTextureCoord_blend += CascadeOffsets[iNextCascadeIndex];

                ShadowComputeCoordinatesTransform(iNextCascadeIndex, vShadowMapTextureCoord_blend);

                // We repeat the calcuation for the next cascade layer, when blending between maps.
                if (fCurrentPixelsBlendBandLocation < BlurBetweenCascades)
                {  
                    // the current pixel is within the blend band.
                    ShadowmapCalculatePCFPercentLit(vShadowMapTextureCoord_blend, fRightTextDepthWeight_blend, fUpTextDepthWeight_blend, fBlurRowSize, (float)iNextCascadeIndex, fPercentLit_blend);
                    fPercentLit = lerp(fPercentLit_blend, fPercentLit, fBlendBetweenCascadesAmount);
                    // Blend the two calculated shadows by the blend amount.
                }
            }
        }

        return fPercentLit;

    }
}




float3 FullLighting(float3 diff, float3 spec, float3 norm, float4 vc0, uniform ShaderGlobalLightParams globalLights, uint enableShadows, float shadowdepth, float4 shadowcoord)
{
    float lf = saturate(dot(norm, globalLights.LightDir.xyz));

    float shadowlit = 1.0;
    if (enableShadows == 1)
    {
        //float shadowdepth = input.Shadows.x;// *0.000001;
        if (abs(shadowdepth) < ShadowMaxDistance)//2km
        {
            //float4 shadowcoord = input.LightShadow;
            //float4 shadowcolour = (float4)1;
            shadowlit = ShadowAmount(shadowcoord, shadowdepth);// , shadowcolour);
        }
    }

    lf *= shadowlit;

    float3 speclit = spec*shadowlit;

    return GlobalLighting(diff, norm, vc0, lf, globalLights) + speclit;
}

