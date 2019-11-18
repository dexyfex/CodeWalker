

cbuffer clouds_locals : register(b0)
{
    float3 gSkyColor;                  // Offset:    0 Size:    12 [unused]
    float gPad00;
    float3 gEastMinusWestColor;        // Offset:   16 Size:    12 [unused]
    float gPad01;
    float3 gWestColor;                 // Offset:   32 Size:    12 [unused]
    float gPad02;
    float3 gSunDirection;              // Offset:   48 Size:    12
    float gPad03;
    float3 gSunColor;                  // Offset:   64 Size:    12
    float gPad04;
    float3 gCloudColor;                // Offset:   80 Size:    12 [unused]
    float gPad05;
    float3 gAmbientColor;              // Offset:   96 Size:    12 [unused]
    float gPad06;
    float3 gBounceColor;               // Offset:  112 Size:    12 [unused]
    float gPad07;
    float4 gDensityShiftScale;         // Offset:  128 Size:    16 [unused]
    float4 gScatterG_GSquared_PhaseMult_Scale;// Offset:  144 Size:    16
    float4 gPiercingLightPower_Strength_NormalStrength_Thickness;// Offset:  160 Size:    16
    float3 gScaleDiffuseFillAmbient;   // Offset:  176 Size:    12 [unused]
    float gPad08;
    float3 gWrapLighting_MSAARef;      // Offset:  192 Size:    12 [unused]
    float gPad09;
    float4 gNearFarQMult;              // Offset:  208 Size:    16 [unused]
    float3 gAnimCombine;               // Offset:  224 Size:    12 [unused]
    float gPad10;
    float3 gAnimSculpt;                // Offset:  240 Size:    12 [unused]
    float gPad11;
    float3 gAnimBlendWeights;          // Offset:  256 Size:    12 [unused]
    float gPad12;
    float4 gUVOffset[2];               // Offset:  272 Size:    32
    row_major float4x4 gCloudViewProj; // Offset:  304 Size:    64
    float4 gCameraPos;                 // Offset:  368 Size:    16
    float2 gUVOffset1;                 // Offset:  384 Size:     8
    float2 gUVOffset2;                 // Offset:  392 Size:     8
    float2 gUVOffset3;                 // Offset:  400 Size:     8
    float2 gRescaleUV1;                // Offset:  408 Size:     8
    float2 gRescaleUV2;                // Offset:  416 Size:     8
    float2 gRescaleUV3;                // Offset:  424 Size:     8
    float gSoftParticleRange;          // Offset:  432 Size:     4 [unused]
    float gEnvMapAlphaScale;           // Offset:  436 Size:     4 [unused]
    float2 cloudLayerAnimScale1;       // Offset:  440 Size:     8
    float2 cloudLayerAnimScale2;       // Offset:  448 Size:     8
    float2 cloudLayerAnimScale3;       // Offset:  456 Size:     8
};
