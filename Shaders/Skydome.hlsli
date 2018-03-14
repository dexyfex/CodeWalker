


cbuffer SkySystemLocals : register(b0)
{
    float4 azimuthEastColor;           // Offset:    0 Size:    12 [unused]
    float4 azimuthWestColor;           // Offset:   16 Size:    12 [unused]
    float3 azimuthTransitionColor;     // Offset:   32 Size:    12 [unused]
    float azimuthTransitionPosition;    // Offset:   44 Size:     4 [unused]
    float4 zenithColor;                // Offset:   48 Size:    12 [unused]
    float4 zenithTransitionColor;      // Offset:   64 Size:    12 [unused]
    float4 zenithConstants;            // Offset:   80 Size:    16 [unused]
    float4 skyPlaneColor;              // Offset:   96 Size:    16 [unused]
    float4 skyPlaneParams;             // Offset:  112 Size:    16 [unused]
    float hdrIntensity;                 // Offset:  128 Size:     4 [unused]
    float3 sunColor;                   // Offset:  132 Size:    12
    float4 sunColorHdr;                // Offset:  144 Size:    12
    float4 sunDiscColorHdr;            // Offset:  160 Size:    12 [unused]
    float4 sunConstants;               // Offset:  176 Size:    16
    float4 sunDirection;               // Offset:  192 Size:    12
    float4 sunPosition;                // Offset:  208 Size:    12 [unused]
    float4 cloudBaseMinusMidColour;    // Offset:  224 Size:    12
    float4 cloudMidColour;             // Offset:  240 Size:    12
    float4 cloudShadowMinusBaseColourTimesShadowStrength;// Offset:  256 Size:    12
    float4 cloudDetailConstants;       // Offset:  272 Size:    16
    float4 cloudConstants1;            // Offset:  288 Size:    16
    float4 cloudConstants2;            // Offset:  304 Size:    16
    float4 smallCloudConstants;        // Offset:  320 Size:    16
    float4 smallCloudColorHdr;         // Offset:  336 Size:    12
    float4 effectsConstants;           // Offset:  352 Size:    16
    float horizonLevel;                 // Offset:  368 Size:     4 [unused]
    float3 speedConstants;             // Offset:  372 Size:    12 [unused]
    float starfieldIntensity;           // Offset:  384 Size:     4
    float3 moonDirection;              // Offset:  388 Size:    12
    float3 moonPosition;               // Offset:  400 Size:    12 [unused]
    float moonIntensity;                // Offset:  412 Size:     4 [unused]
    float4 lunarCycle;                 // Offset:  416 Size:    12
    float3 moonColor;                  // Offset:  432 Size:    12
    float noiseFrequency;               // Offset:  444 Size:     4 [unused]
    float noiseScale;                   // Offset:  448 Size:     4 [unused]
    float noiseThreshold;               // Offset:  452 Size:     4 [unused]
    float noiseSoftness;                // Offset:  456 Size:     4 [unused]
    float noiseDensityOffset;           // Offset:  460 Size:     4 [unused]
    float4 noisePhase;                 // Offset:  464 Size:     8 [unused]
};
