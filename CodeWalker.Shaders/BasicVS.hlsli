#include "Quaternion.hlsli"
#include "Shadowmap.hlsli"

cbuffer VSSceneVars : register(b0)
{
    float4x4 ViewProj;
    float4 WindVector;
}
cbuffer VSEntityVars : register(b2)
{
    float4 CamRel;
    float4 Orientation;
    uint HasSkeleton;
    uint HasTransforms;
    uint TintPaletteIndex;
    uint Pad1;
    float3 Scale;
    uint IsInstanced;
}
cbuffer VSModelVars : register(b3)
{
    float4x4 Transform;
}
cbuffer VSGeomVars : register(b4)
{
    uint EnableTint;
    float TintYVal;
    uint IsDecal;
    uint EnableWind;
    float4 WindOverrideParams;
    float4 globalAnimUV0;
    float4 globalAnimUV1;
}
cbuffer VSInstGlobals : register(b5)
{
    float4 gInstanceVars[24]; //instance rotation matrices
}
cbuffer VSInstLocals : register(b6)
{
    float3 vecBatchAabbMin;            // Offset:    0 Size:    12
    float instPad0;
    float3 vecBatchAabbDelta;          // Offset:   16 Size:    12
    float instPad1;
    float4 vecPlayerPos;               // Offset:   32 Size:    16
    float2 _vecCollParams;             // Offset:   48 Size:     8
    float2 instPad2;
    float4 fadeAlphaDistUmTimer;       // Offset:  256 Size:    16
    float4 uMovementParams;            // Offset:  272 Size:    16
    float4 _fakedGrassNormal;          // Offset:  288 Size:    16 [unused]
    float3 gScaleRange;                // Offset:  304 Size:    12
    float instPad3;
    float4 gWindBendingGlobals;        // Offset:  320 Size:    16
    float2 gWindBendScaleVar;          // Offset:  336 Size:     8
    float gAlphaTest;                  // Offset:  344 Size:     4 [unused]
    float gAlphaToCoverageScale;       // Offset:  348 Size:     4 [unused]
    float3 gLodFadeInstRange;          // Offset:  352 Size:    12 [unused]
    uint gUseComputeShaderOutputBuffer;// Offset:  364 Size:     4
}
cbuffer BoneMatrices : register(b7) //rage_bonemtx
{
    row_major float3x4 gBoneMtx[255]; // Offset:    0 Size: 12240
}
cbuffer ClothVertices : register(b8) //pedcloth
{
    float4 clothVertices[254]; // Offset:   64 Size:  4060
}

struct VS_OUTPUT
{
    float4 Position  : SV_POSITION;
    float3 Normal    : NORMAL;
    float2 Texcoord0 : TEXCOORD0;
    float2 Texcoord1 : TEXCOORD1;
    float2 Texcoord2 : TEXCOORD2;
    float4 Shadows   : TEXCOORD3;
    float4 LightShadow : TEXCOORD4;
    float4 Colour0   : COLOR0;
    float4 Colour1   : COLOR1;
    float4 Tint      : COLOR2;
    float4 Tangent   : TEXCOORD5;
    float4 Bitangent : TEXCOORD6;
    float3 CamRelPos : TEXCOORD7;
};

Texture2D<float4> TintPalette : register(t0);
SamplerState TextureSS : register(s0);





struct rage__fwGrassInstanceListDef__InstanceData //16 bytes, Key:2740378365 rage__fwGrassInstanceListDef__InstanceData//3985044770
{
    uint PosXY_u16;                // Offset:    0
    uint PosZ_u16_NormXY_u8;       // Offset:    4
    uint ColorRGB_Scale_u8;        // Offset:    8
    uint Ao_Pad3_u8;               // Offset:   12
};
StructuredBuffer<rage__fwGrassInstanceListDef__InstanceData> GrassInstances : register(t2);


float3 GetGrassInstancePosition(float3 ipos, float3 vc0, float3 vc1, uint iid)
{
    float3 opos = ipos;


    float4 windBendParam = float4(gWindBendScaleVar.xy, gWindBendingGlobals.xy);
    if (EnableWind == 0)
    {
        windBendParam = 0;
    }


    float4 r0, r1, r2, r3, r4, r5, r6, r7;
    uint4 u0, u1, u2, u6;

    //r0.x = iid; //ld_structured r0.x, v5.x, l(0), t0.xxxx
    //ushr r0.y, r0.x, l(16)
    //and r0.y, r0.y, l(255)
    //utof r0.y, r0.y
    u0.y = (iid >> 16) & 255;
    r0.y = (float)u0.y;
    r1.y = r0.y * 0.0039215686; //mul r1.y, r0.y, l(0.003922)
    //r2.x=iid              //mov r2.x, v5.x
    //r2.yz=1               //mov r2.yz, l(0,1.000000,1.000000,0)
    u0.y = (iid >> 24);     //ushr r0.y, r0.x, l(24)
    u1.x = u0.x & 0xFFFF;   //and r1.x, r0.x, l(0x0000ffff)
    r0.x = (float)u0.y;     //utof r0.x, r0.y
    r1.z = r0.x * 0.0039215686; //mul r1.z, r0.x, l(0.003922)
    r0.xyz = (gUseComputeShaderOutputBuffer != 0) ? float3(u1.x, r1.yz) : float3(iid, 1, 1); //movc r0.xyz, gUseComputeShaderOutputBuffer, r1.xyzx, r2.xyzx
    float amo = r0.z*r0.y;  //mul o4.x, r0.z, r0.y //alpha multiplier output
    
    r0.xyz = float3(iid, 1, 1);

    //ld_structured r1.xyzw, r0.x, l(0), t1.xyzw
    rage__fwGrassInstanceListDef__InstanceData inst = GrassInstances[iid];
    u1.x = inst.PosXY_u16;
    u1.y = inst.PosZ_u16_NormXY_u8;
    u1.z = inst.ColorRGB_Scale_u8;
    u1.w = inst.Ao_Pad3_u8;

    ////debug positioning
    //float px = (float)((u1.x & 0xFFFF));
    //float py = (float)((u1.x >> 16) & 0xFFFF);
    //float pz = (float)((u1.y & 0xFFFF));
    //float3 pab = float3(px, py, pz)*0.000015;
    //float3 fpos = vecBatchAabbMin.xyz + vecBatchAabbDelta.xyz * pab;
    //return opos + fpos - vecPlayerPos.xyz;


    u2.w = u1.z >> 8;       //ushr r2.w, r1.z, l(8)
    u2.xy = u1.xy >> 16;    //ushr r2.xy, r1.xyxx, l(16)
    u2.yz = u2.yw & 255;    //and r2.yz, r2.yywy, l(0, 255, 255, 0)
    r3.y = (float)u2.x;     //utof r3.y, r2.x
    r2.x = (float)u2.y;     //utof r2.x, r2.y
    u0.w = u1.y >> 24;      //ushr r0.w, r1.y, l(24)
    r2.y = (float)u0.w;     //utof r2.y, r0.w
    r2.xy = r2.xy*0.0078431373 - 1.0;   //mad r2.xy, r2.xyxx, l(0.007843, 0.007843, 0.000000, 0.000000), l(-1.000000, -1.000000, 0.000000, 0.000000)
    r0.w = dot(r2.xy, r2.xy);       //dp2 r0.w, r2.xyxx, r2.xyxx
    r0.w = 1.0 - r0.w;              //add r0.w, -r0.w, l(1.000000)
    r2.z = sqrt(r0.w);              //sqrt r2.z, r0.w
    r0.w = r2.z*-0.018729 + 0.074261;   //mad r0.w, r2.z, l(-0.018729), l(0.074261)
    r0.w = r0.w*r2.z - 0.212114;        //mad r0.w, r0.w, r2.z, l(-0.212114)
    r0.w = r0.w*r2.z + 1.570729;        //mad r0.w, r0.w, r2.z, l(1.570729)
    r2.w = 1.0 - r2.z;                  //add r2.w, -r2.z, l(1.000000)
    r2.w = sqrt(r2.w);                  //sqrt r2.w, r2.w
    r2.xyz = -r2.z*float3(0, 0, 1) + r2.xyz;    //mad r2.xyz, -r2.zzzz, l(0.000000, 0.000000, 1.000000, 0.000000), r2.xyzx
    r0.w = r0.w*r2.w;   //mul r0.w, r0.w, r2.w
    r0.w = r0.w * vecPlayerPos.w;   //mul r0.w, r0.w, vecPlayerPos.w
    r5.x = sin(r0.w);   //sincos r5.x, r6.x, r0.w
    r6.x = cos(r0.w);
    //r0.w = dot(r2.xyz, r2.xyz); //dp3 r0.w, r2.xyzx, r2.xyzx
    //r0.w = 1.0 / sqrt(r0.w);    //rsq r0.w, r0.w
    //r2.xyz = r2.xyz*r0.w;       //mul r2.xyz, r0.wwww, r2.xyzx
    r2.xyz = normalize(r2.xyz);
    r2.xyz = r2.xyz * r5.x;                 //mul r2.xyz, r5.xxxx, r2.xyzx
    r2.xyz = r6.x*float3(0, 0, 1) + r2.xyz; //mad r2.xyz, r6.xxxx, l(0.000000, 0.000000, 1.000000, 0.000000), r2.xyzx
    r5.xy = r2.yz*float2(1, 0);             //mul r5.xy, r2.yzyy, l(1.000000, 0.000000, 0.000000, 0.000000)
    r5.xy = r2.zx*float2(0, 1) - r5.xy;     //mad r5.xy, r2.zxzz, l(0.000000, 1.000000, 0.000000, 0.000000), -r5.xyxx
    r0.w = dot(r5.xy, r5.xy);   //dp2 r0.w, r5.xyxx, r5.xyxx
    r0.w = sqrt(r0.w);          //sqrt r0.w, r0.w
    r5.xy = r5.xy / r0.w;       //div r5.xy, r5.xyxx, r0.wwww
    r0.w = 1.0 - r2.z;          //add r0.w, -r2.z, l(1.000000)
    r2.w = r5.x*r0.w;           //mul r2.w, r5.x, r0.w
    r5.x = r2.w*r5.x + r2.z;    //mad r5.x, r2.w, r5.x, r2.z
    r5.z = r5.y*r2.w;           //mul r5.z, r5.y, r2.w
    r2.w = r5.y*r5.y;           //mul r2.w, r5.y, r5.y
    r5.w = r2.w*r0.w + r2.z;    //mad r5.w, r2.w, r0.w, r2.z
    u0.w = r2.z < 1.0;          //lt r0.w, r2.z, l(1.000000)
    r2.xyz = r2.xyz*float3(-1, -1, 1);  //mul r2.xyz, r2.xyzx, l(-1.000000, -1.000000, 1.000000, 0.000000)
    r2.xyz = (u0.w != 0) ? r2.xyz : float3(0, 0, 1);    //movc r2.xyz, r0.wwww, r2.xyzx, l(0, 0, 1.000000, 0)
    r5.xyz = (u0.w != 0) ? r5.xzw : float3(1, 0, 1);    //movc r5.xyz, r0.wwww, r5.xzwx, l(1.000000, 0, 1.000000, 0)
    r0.w = vc0.y * 6.283185;     //mul r0.w, v2.y, l(6.283185)
    r6.xyz = fadeAlphaDistUmTimer.z*uMovementParams.zzw + r0.w; //mad r6.xyz, fadeAlphaDistUmTimer.zzzz, uMovementParams.zzwz, r0.wwww
    r6.xyz = sin(r6.xyz);       //sincos r6.xyz, null, r6.xyzx
    r0.w = 1.0 - vc0.z;         //add r0.w, -v2.z, l(1.000000)
    r7.z = r0.w * uMovementParams.y;    //mul r7.z, r0.w, uMovementParams.y
    r7.xy = vc0.x * uMovementParams.x;  //mul r7.xy, v2.xxxx, uMovementParams.xxxx
    r6.xyz = r6.xyz * r7.xyz;           //mul r6.xyz, r6.xyzx, r7.xyzx
    u0.w = u1.z >> 24;          //ushr r0.w, r1.z, l(24)
    r0.w = (float)u0.w;         //utof r0.w, r0.w
    r0.w = r0.w * 0.0039215686;     //mul r0.w, r0.w, l(0.003922)
    r2.w = gScaleRange.y - gScaleRange.x;   //add r2.w, -gScaleRange.x, gScaleRange.y
    r0.w = r2.w*r0.w + gScaleRange.x;       //mad r0.w, r2.w, r0.w, gScaleRange.x
    r2.w = gScaleRange.z * gScaleRange.y;   //mul r2.w, gScaleRange.z, gScaleRange.y
    u0.x = iid & 7;     //and r0.x, r0.x, l(7)
    u0.x = u0.x * 3;    //imul null, r0.x, r0.x, l(3)
    r2.w = dot(gInstanceVars[u0.x].ww, r2.ww);      //dp2 r2.w, gInstanceVars[r0.x + 0].wwww, r2.wwww
    r2.w = -gScaleRange.y * gScaleRange.z + r2.w;   //mad r2.w, -gScaleRange.y, gScaleRange.z, r2.w
    r0.w = r0.w + r2.w;     //add r0.w, r0.w, r2.w
    r0.w = max(r0.w, 0.0);  //max r0.w, r0.w, l(0.000000)
    r0.w = r0.w  * r0.z;    //mul r0.w, r0.z, r0.w //compute cull scale fadeout?

    r0.yzw = opos*r0.w + r6.xyz;  //mad r0.yzw, v0.xxyz, r0.wwww, r6.xxyz  //scale and offset (movement?)
    r6.xyz = r0.z * gInstanceVars[u0.x + 1].xyz;    //mul r6.xyz, r0.zzzz, gInstanceVars[r0.x + 1].xyzx   //rotate vertex positions with instance matrix
    r6.xyz = r0.y * gInstanceVars[u0.x].xyz + r6.xyz;   //mad r6.xyz, r0.yyyy, gInstanceVars[r0.x + 0].xyzx, r6.xyzx
    r0.yzw = r0.w * gInstanceVars[u0.x + 2].xyz + r6.xyz;   //mad r0.yzw, r0.wwww, gInstanceVars[r0.x + 2].xxyz, r6.xxyz
    //instance matrices!!!

    u6.xyzw = u1.xyzw & uint4(0xFFFF, 0xFFFF, 255, 255);    //and r6.xyzw, r1.xyzw, l(0x0000ffff, 0x0000ffff, 255, 255)
    u1.x = u1.z >> 16;  //ushr r1.x, r1.z, l(16)
    u1.x = u1.x & 255;  //and r1.x, r1.x, l(255)
    r3.xzw = (float3)u6.xyw;    //utof r3.xzw, r6.xxyw
    r4.z = (float)u1.x;     //utof r4.z, r1.x
    r4.y = (float)u2.z;     //utof r4.y, r2.z
    r4.x = (float)u6.z;     //utof r4.x, r6.z
    float3 tnt = r4.xyz * 0.0039215686; //mul o1.xyz, r4.xyzx, l(0.003922, 0.003922, 0.003922, 0.000000) ///colour tint output
    float aoo = r3.w * 0.0039215686;    //mul o4.y, r3.w, l(0.003922) //ambient occlusion value output

    r1.xyz = r3.xyz*vecBatchAabbDelta.xyz;  //mul r1.xyz, r3.xyzx, vecBatchAabbDelta.xyzx
    r1.xyz = r1.xyz/65535 + vecBatchAabbMin.xyz; //mad r1.xyz, r1.xyzx, l(0.000015, 0.000015, 0.000015, 0.000000), vecBatchAabbMin.xyzx
    r1.w = dot(r1.xyz, r1.xyz); //dp3 r1.w, r1.xyzx, r1.xyzx
    r1.w = cos(r1.w);           //sincos null, r1.w, r1.w
    r1.w = r1.w*0.5 + 0.5;      //mad r1.w, r1.w, l(0.500000), l(0.500000)
    r1.w = r1.w*windBendParam.y + 1.0;   //gWindBendScaleVar.y + 1.0;  //mad r1.w, gWindBendScaleVar.y, r1.w, l(1.000000)
    r1.w = r1.w*windBendParam.x;         //gWindBendScaleVar.x;        //mul r1.w, r1.w, gWindBendScaleVar.x
    r3.xy = r1.w*windBendParam.zw;       //gWindBendingGlobals.xy;    //mul r3.xy, r1.wwww, gWindBendingGlobals.xyxx
    r5.w = r3.x;                //mov r5.w, r3.x
    r4.x = dot(r0.yzw, r5.xyw); //dp3 r4.x, r0.yzwy, r5.xywx
    r3.zw = r5.yz;              //mov r3.zw, r5.yyyz
    r4.y = dot(r0.wyz, r3.yzw); //dp3 r4.y, r0.wyzw, r3.yzwy
    r4.z = dot(r0.yzw, r2.xyz); //dp3 r4.z, r0.yzwy, r2.xyzx
    r0.yzw = r1.xyz + r4.xyz;   //add r0.yzw, r1.xxyz, r4.xxyz  //add vertex position to instance position

    r1.yzw = r0.yzw - (vecPlayerPos.xyz - float3(0,0,-0.2)); //add r1.yzw, r0.yyzw, -vecPlayerPos.xxyz
    r1.w = dot(r1.yzw, r1.yzw);     //dp3 r1.w, r1.yzwy, r1.yzwy
    r2.w = 1.0 / sqrt(r1.w);        //rsq r2.w, r1.w
    r1.w = _vecCollParams.x - r1.w; //add r1.w, -r1.w, _vecCollParams.x
    r1.w = saturate(_vecCollParams.y * r1.w);   //mul_sat r1.w, r1.w, _vecCollParams.y
    r1.w = r1.w * 0.5;      //mul r1.w, r1.w, l(0.500000)
    r1.yz = r1.yz * r2.w;   //mul r1.yz, r1.yyzy, r2.wwww
    r1.yz = r1.yz * r1.w;   //mul r1.yz, r1.yyzy, r1.wwww
    r0.yz = r1.yz * vc1.z + r0.yz;   //mad r0.yz, r1.yyzy, v3.zzzz, r0.yyzy  //contribute player collision


    //float3 fpos = r0.yzw;
    return r0.yzw - vecPlayerPos.xyz;


    //return CamRel.xyz + opos;
}




void BoneTransform(float4 weights, float4 indices, float3 ipos, float3 inorm, float3 itang, out float3 opos, out float3 onorm, out float3 otang)
{
    uint4 binds = (uint4)(indices * 255.001953);
    if (binds.z > 254) //this is the signal to use clothVertices!
    {
        float4 cv0 = clothVertices[binds.w];
        float4 cv1 = clothVertices[binds.x];
        float4 cv2 = clothVertices[binds.y];
        float3 r0 = cv0.zxy - cv1.zxy;
        float3 r8 = cv2.yzx - cv1.yzx;
        float3 r4 = normalize((r0.zxy * r8.yzx) - (r0.xyz * r8.xyz));
        float3 r5 = (cv2.xyz*weights.x) + (cv1.xyz*weights.y) + (cv0.xyz*weights.z);
        float r0w = (weights.w - 0.5) * 0.1;
        r5 = (r0w * -r4) + r5;
        opos = r5;
        onorm = r4;
        
        float3 r7 = r4.yzx * itang.zxy;//itang? transformed by bone?? weird
        float3 r9 = r4.zxy * itang.yzx;
        otang = (r9 - r7);
    }
    else
    {
        float3x4 b0 = gBoneMtx[binds.x];
        float3x4 b1 = gBoneMtx[binds.y];
        float3x4 b2 = gBoneMtx[binds.z];
        float3x4 b3 = gBoneMtx[binds.w];
        float4 t0 = b0[0]*weights.x + b1[0]*weights.y + b2[0]*weights.z + b3[0]*weights.w;
        float4 t1 = b0[1]*weights.x + b1[1]*weights.y + b2[1]*weights.z + b3[1]*weights.w;
        float4 t2 = b0[2]*weights.x + b1[2]*weights.y + b2[2]*weights.z + b3[2]*weights.w;
        float3x4 m = float3x4(t0, t1, t2);
        float4 p = float4(ipos, 1);
        opos = float3(dot(m[0], p), dot(m[1], p), dot(m[2], p));
        onorm = float3(dot(m[0].xyz, inorm), dot(m[1].xyz, inorm), dot(m[2].xyz, inorm));
        otang = float3(dot(m[0].xyz, itang), dot(m[1].xyz, itang), dot(m[2].xyz, itang));
    }
}
float3 ModelTransform(float3 ipos, float3 vc0, float3 vc1, uint iid)
{
    if (IsInstanced)
    {
        return GetGrassInstancePosition(ipos, vc0, vc1, iid);
    }
    else
    {
        float3 tpos = (HasTransforms == 1) ? mul(float4(ipos, 1), Transform).xyz : ipos;
        float3 spos = tpos * Scale;
        float3 bpos = mulvq(spos, Orientation);
        if (EnableWind)
        {
            bpos = GeomWindMotion(bpos, vc0, WindVector, WindOverrideParams);
        }
        return CamRel.xyz + bpos;
    }
}
float4 ScreenTransform(float3 opos)
{
    float4 pos = float4(opos, 1);
    float4 cpos = mul(pos, ViewProj);
    //if (IsDecal == 1)
    //{
    //    //cpos.z -= 0.003; //todo: correct decal z-bias
    //}
    cpos.z = DepthFunc(cpos.zw);
    return cpos;
}
float3 NormalTransform(float3 inorm)
{
    float3 tnorm = (HasTransforms == 1) ? mul(inorm, (float3x3)Transform) : inorm;
    float3 bnorm = normalize(mulvq(tnorm, Orientation));
    return bnorm;
}

float4 ColourTint(float tx, float tx2, uint iid)
{
    float4 tnt = 1;
    if (IsInstanced)
    {
        //RenderableGrassInstance inst = GrassInstances[iid];
        //tnt = Unpack4x8UNF(inst.Colour);
        rage__fwGrassInstanceListDef__InstanceData inst = GrassInstances[iid];
        uint c = inst.ColorRGB_Scale_u8;
        float3 rgb = float3(c & 0xFF, (c >> 8) & 0xFF, (c >> 16) & 0xFF);
        tnt = float4(rgb * 0.003922, 1);
    }
    else if (EnableTint > 0)
    {
        float tu = (EnableTint == 1) ? tx : tx2;
        tnt = TintPalette.SampleLevel(TextureSS, float2(tu, TintYVal), 0);
    }
    return tnt;
}





float2 GlobalUVAnim(float2 uv)
{
    float2 r;
    float3 uvw = float3(uv, 1);
    r.x = dot(globalAnimUV0.xyz, uvw);
    r.y = dot(globalAnimUV1.xyz, uvw);
    return r;
}



















//grass_batch.fxc_VS_Transform
//
// Generated by Microsoft (R) HLSL Shader Compiler 9.29.952.3111
//
//
// Buffer Definitions: 
//
// cbuffer rage_matrices
// {
//
//   row_major float4x4 gWorld;         // Offset:    0 Size:    64 [unused]
//   row_major float4x4 gWorldView;     // Offset:   64 Size:    64 [unused]
//   row_major float4x4 gWorldViewProj; // Offset:  128 Size:    64
//   row_major float4x4 gViewInverse;   // Offset:  192 Size:    64 [unused]
//
// }
//
// cbuffer grassglobals
// {
//
//   bool bVehColl0Enabled;             // Offset:    0 Size:     4
//   bool bVehColl1Enabled;             // Offset:    4 Size:     4
//   bool bVehColl2Enabled;             // Offset:    8 Size:     4
//   bool bVehColl3Enabled;             // Offset:   12 Size:     4
//   float4 depthValueBias;             // Offset:   16 Size:    16 [unused]
//
// }
//
// cbuffer grassbatchlocals
// {
//
//   float3 vecBatchAabbMin;            // Offset:    0 Size:    12
//   float3 vecBatchAabbDelta;          // Offset:   16 Size:    12
//   float4 vecPlayerPos;               // Offset:   32 Size:    16
//   float2 _vecCollParams;             // Offset:   48 Size:     8
//   float4 _vecVehColl0B;              // Offset:   64 Size:    16
//   float4 _vecVehColl0M;              // Offset:   80 Size:    16
//   float4 _vecVehColl0R;              // Offset:   96 Size:    16
//   float4 _vecVehColl1B;              // Offset:  112 Size:    16
//   float4 _vecVehColl1M;              // Offset:  128 Size:    16
//   float4 _vecVehColl1R;              // Offset:  144 Size:    16
//   float4 _vecVehColl2B;              // Offset:  160 Size:    16
//   float4 _vecVehColl2M;              // Offset:  176 Size:    16
//   float4 _vecVehColl2R;              // Offset:  192 Size:    16
//   float4 _vecVehColl3B;              // Offset:  208 Size:    16
//   float4 _vecVehColl3M;              // Offset:  224 Size:    16
//   float4 _vecVehColl3R;              // Offset:  240 Size:    16
//   float4 fadeAlphaDistUmTimer;       // Offset:  256 Size:    16
//   float4 uMovementParams;            // Offset:  272 Size:    16
//   float4 _fakedGrassNormal;          // Offset:  288 Size:    16 [unused]
//   float3 gScaleRange;                // Offset:  304 Size:    12
//   float4 gWindBendingGlobals;        // Offset:  320 Size:    16
//   float2 gWindBendScaleVar;          // Offset:  336 Size:     8
//   float gAlphaTest;                  // Offset:  344 Size:     4 [unused]
//   float gAlphaToCoverageScale;       // Offset:  348 Size:     4 [unused]
//   float3 gLodFadeInstRange;          // Offset:  352 Size:    12 [unused]
//   uint gUseComputeShaderOutputBuffer;// Offset:  364 Size:     4
//   float2 gInstCullParams;            // Offset:  368 Size:     8 [unused]
//   uint gNumClipPlanes;               // Offset:  376 Size:     4 [unused]
//   float4 gClipPlanes[16];            // Offset:  384 Size:   256 [unused]
//   float3 gCameraPosition;            // Offset:  640 Size:    12 [unused]
//   uint gLodInstantTransition;        // Offset:  652 Size:     4 [unused]
//   float4 gLodThresholds;             // Offset:  656 Size:    16 [unused]
//   float2 gCrossFadeDistance;         // Offset:  672 Size:     8 [unused]
//   float2 gLodFadeStartDist;          // Offset:  680 Size:     8 [unused]
//   float2 gLodFadeRange;              // Offset:  688 Size:     8 [unused]
//   float2 gLodFadePower;              // Offset:  696 Size:     8 [unused]
//   float2 gLodFadeTileScale;          // Offset:  704 Size:     8 [unused]
//   uint gIsShadowPass;                // Offset:  712 Size:     4 [unused]
//   float3 gLodFadeControlRange;       // Offset:  720 Size:    12 [unused]
//   float4 bDebugSwitches;             // Offset:  736 Size:    16 [unused]
//
// }
//
// cbuffer grassbatch_instmtx
// {
//
//   float4 gInstanceVars[24];          // Offset:    0 Size:   384
//
// }
//
// Resource bind info for InstanceBuffer
// {
//
//   struct
//   {
//       
//       uint InstId_u16_CrossFade_Scale_u8;// Offset:    0
//
//   } $Element;                        // Offset:    0 Size:     4
//
// }
//
// Resource bind info for RawInstanceBuffer
// {
//
//   struct
//   {
//       
//       uint PosXY_u16;                // Offset:    0
//       uint PosZ_u16_NormXY_u8;       // Offset:    4
//       uint ColorRGB_Scale_u8;        // Offset:    8
//       uint Ao_Pad3_u8;               // Offset:   12
//
//   } $Element;                        // Offset:    0 Size:    16
//
// }
//
//
// Resource Bindings:
//
// Name                                 Type  Format         Dim      HLSL Bind  Count
// ------------------------------ ---------- ------- ----------- -------------- ------
// InstanceBuffer                    texture  struct         r/o             t0      1 
// RawInstanceBuffer                 texture  struct         r/o             t1      1 
// rage_matrices                     cbuffer      NA          NA            cb1      1 
// grassglobals                      cbuffer      NA          NA            cb4      1 
// grassbatch_instmtx                cbuffer      NA          NA            cb7      1 
// grassbatchlocals                  cbuffer      NA          NA           cb11      1 
//
//
//
// Input signature:
//
// Name                 Index   Mask Register SysValue  Format   Used
// -------------------- ----- ------ -------- -------- ------- ------
// POSITION                 0   xyzw        0     NONE   float   xyz 
// NORMAL                   0   xyz         1     NONE   float   xyz 
// COLOR                    0   xyzw        2     NONE   float   xyz 
// COLOR                    1   xyzw        3     NONE   float   x z 
// TEXCOORD                 0   xy          4     NONE   float   xy  
// SV_InstanceID            0   x           5   INSTID    uint   x   
//
//
// Output signature:
//
// Name                 Index   Mask Register SysValue  Format   Used
// -------------------- ----- ------ -------- -------- ------- ------
// SV_Position              0   xyzw        0      POS   float   xyzw
// TEXCOORD                 0   xyzw        1     NONE   float   xyzw
// TEXCOORD                 1   xyzw        2     NONE   float   xyzw
// TEXCOORD                 2   xyzw        3     NONE   float   xyzw
// TEXCOORD                 5   xy          4     NONE   float   xy  
//
/*
vs_4_0
dcl_globalFlags refactoringAllowed | enableRawAndStructuredBuffers
dcl_constantbuffer CB1[12], immediateIndexed
dcl_constantbuffer CB4[1], immediateIndexed
dcl_constantbuffer CB11[23], immediateIndexed
dcl_constantbuffer CB7[24], dynamicIndexed
dcl_resource_structured t0, 4
dcl_resource_structured t1, 16
dcl_input v0.xyz
dcl_input v1.xyz
dcl_input v2.xyz
dcl_input v3.xz
dcl_input v4.xy
dcl_input_sgv v5.x, instance_id
dcl_output_siv o0.xyzw, position
dcl_output o1.xyzw
dcl_output o2.xyzw
dcl_output o3.xyzw
dcl_output o4.xy
dcl_temps 8

ld_structured r0.x, v5.x, l(0), t0.xxxx
ushr r0.y, r0.x, l(16)
and r0.y, r0.y, l(255)
utof r0.y, r0.y
mul r1.y, r0.y, l(0.003922)
mov r2.x, v5.x
mov r2.yz, l(0,1.000000,1.000000,0)
ushr r0.y, r0.x, l(24)
and r1.x, r0.x, l(0x0000ffff)
utof r0.x, r0.y
mul r1.z, r0.x, l(0.003922)
movc r0.xyz, gUseComputeShaderOutputBuffer, r1.xyzx, r2.xyzx
ld_structured r1.xyzw, r0.x, l(0), t1.xyzw
ushr r2.w, r1.z, l(8)
ushr r2.xy, r1.xyxx, l(16)
and r2.yz, r2.yywy, l(0, 255, 255, 0)
utof r3.y, r2.x
utof r2.x, r2.y
utof r4.y, r2.z
ushr r0.w, r1.y, l(24)
utof r2.y, r0.w
mad r2.xy, r2.xyxx, l(0.007843, 0.007843, 0.000000, 0.000000), l(-1.000000, -1.000000, 0.000000, 0.000000)
dp2 r0.w, r2.xyxx, r2.xyxx
add r0.w, -r0.w, l(1.000000)
sqrt r2.z, r0.w
mad r0.w, r2.z, l(-0.018729), l(0.074261)
mad r0.w, r0.w, r2.z, l(-0.212114)
mad r0.w, r0.w, r2.z, l(1.570729)
add r2.w, -r2.z, l(1.000000)
mad r2.xyz, -r2.zzzz, l(0.000000, 0.000000, 1.000000, 0.000000), r2.xyzx
sqrt r2.w, r2.w
mul r0.w, r0.w, r2.w
mul r0.w, r0.w, vecPlayerPos.w
sincos r5.x, r6.x, r0.w
dp3 r0.w, r2.xyzx, r2.xyzx
rsq r0.w, r0.w
mul r2.xyz, r0.wwww, r2.xyzx
mul r2.xyz, r5.xxxx, r2.xyzx
mad r2.xyz, r6.xxxx, l(0.000000, 0.000000, 1.000000, 0.000000), r2.xyzx
mul r5.xy, r2.yzyy, l(1.000000, 0.000000, 0.000000, 0.000000)
mad r5.xy, r2.zxzz, l(0.000000, 1.000000, 0.000000, 0.000000), -r5.xyxx
dp2 r0.w, r5.xyxx, r5.xyxx
sqrt r0.w, r0.w
div r5.xy, r5.xyxx, r0.wwww
add r0.w, -r2.z, l(1.000000)
mul r2.w, r5.x, r0.w
mad r5.x, r2.w, r5.x, r2.z
mul r5.z, r5.y, r2.w
mul r2.w, r5.y, r5.y
mad r5.w, r2.w, r0.w, r2.z
lt r0.w, r2.z, l(1.000000)
mul r2.xyz, r2.xyzx, l(-1.000000, -1.000000, 1.000000, 0.000000)
movc r2.xyz, r0.wwww, r2.xyzx, l(0,0,1.000000,0)
movc r5.xyz, r0.wwww, r5.xzwx, l(1.000000,0,1.000000,0)
mul r0.w, v2.y, l(6.283185)
mad r6.xyz, fadeAlphaDistUmTimer.zzzz, uMovementParams.zzwz, r0.wwww
sincos r6.xyz, null, r6.xyzx
add r0.w, -v2.z, l(1.000000)
mul r7.z, r0.w, uMovementParams.y
mul r7.xy, v2.xxxx, uMovementParams.xxxx
mul r6.xyz, r6.xyzx, r7.xyzx
ushr r0.w, r1.z, l(24)
utof r0.w, r0.w
mul r0.w, r0.w, l(0.003922)
add r2.w, -gScaleRange.x, gScaleRange.y
mad r0.w, r2.w, r0.w, gScaleRange.x
mul r2.w, gScaleRange.z, gScaleRange.y
and r0.x, r0.x, l(7)
imul null, r0.x, r0.x, l(3)
dp2 r2.w, gInstanceVars[r0.x + 0].wwww, r2.wwww
mad r2.w, -gScaleRange.y, gScaleRange.z, r2.w
add r0.w, r0.w, r2.w
max r0.w, r0.w, l(0.000000)
mul r0.w, r0.z, r0.w

mul o4.x, r0.z, r0.y //alpha multiplier output

mad r0.yzw, v0.xxyz, r0.wwww, r6.xxyz  //scale and offset (movement?)
mul r6.xyz, r0.zzzz, gInstanceVars[r0.x + 1].xyzx   //rotate vertex positions with instance matrix
mad r6.xyz, r0.yyyy, gInstanceVars[r0.x + 0].xyzx, r6.xyzx
mad r0.yzw, r0.wwww, gInstanceVars[r0.x + 2].xxyz, r6.xxyz

and r6.xyzw, r1.xyzw, l(0x0000ffff, 0x0000ffff, 255, 255)
ushr r1.x, r1.z, l(16)
and r1.x, r1.x, l(255)
utof r4.z, r1.x
utof r3.xzw, r6.xxyw
utof r4.x, r6.z
mul o1.xyz, r4.xyzx, l(0.003922, 0.003922, 0.003922, 0.000000) ///colour tint output
mul o4.y, r3.w, l(0.003922) //ambient occlusion value output

mul r1.xyz, r3.xyzx, vecBatchAabbDelta.xyzx
mad r1.xyz, r1.xyzx, l(0.000015, 0.000015, 0.000015, 0.000000), vecBatchAabbMin.xyzx
dp3 r1.w, r1.xyzx, r1.xyzx
sincos null, r1.w, r1.w
mad r1.w, r1.w, l(0.500000), l(0.500000)
mad r1.w, gWindBendScaleVar.y, r1.w, l(1.000000)
mul r1.w, r1.w, gWindBendScaleVar.x
mul r3.xy, r1.wwww, gWindBendingGlobals.xyxx
mov r5.w, r3.x
dp3 r4.x, r0.yzwy, r5.xywx
mov r3.zw, r5.yyyz
dp3 r4.y, r0.wyzw, r3.yzwy
dp3 r4.z, r0.yzwy, r2.xyzx
add r0.yzw, r1.xxyz, r4.xxyz  //add vertex position to instance position

add r1.yzw, r0.yyzw, -vecPlayerPos.xxyz
dp3 r1.w, r1.yzwy, r1.yzwy
rsq r2.w, r1.w
add r1.w, -r1.w, _vecCollParams.x
mul_sat r1.w, r1.w, _vecCollParams.y
mul r1.w, r1.w, l(0.500000)
mul r1.yz, r1.yyzy, r2.wwww
mul r1.yz, r1.yyzy, r1.wwww
mad r0.yz, r1.yyzy, v3.zzzz, r0.yyzy  //contribute player collision

add r1.x, -r0.w, _vecVehColl0R.w
add r1.yz, r0.yyzy, -_vecVehColl0B.xxyx
dp2 r1.y, _vecVehColl0M.xyxx, r1.yzyy
mul_sat r1.y, r1.y, _vecVehColl0M.w
mad r1.yz, r1.yyyy, _vecVehColl0M.xxyx, _vecVehColl0B.xxyx
add r1.yz, r0.yyzy, -r1.yyzy
dp2 r1.w, r1.yzyy, r1.yzyy
add r2.w, -r1.w, _vecVehColl0R.y
mul_sat r2.w, r2.w, _vecVehColl0R.z
mad r1.x, r2.w, r1.x, r0.w
mul r2.w, r2.w, _vecVehColl0R.x
mul r3.x, _vecVehColl0R.x, l(0.500000)
mul r3.x, r3.x, r3.x
lt r3.x, r1.w, r3.x
rsq r1.w, r1.w
mul r1.yz, r1.wwww, r1.yyzy
mul r1.yz, r1.yyzy, r2.wwww
mad r4.xy, r1.yzyy, v3.zzzz, r0.yzyy
add r1.y, _vecVehColl0R.w, l(-0.250000)
movc r4.z, r3.x, r1.y, r1.x
add r1.x, r0.w, -_vecVehColl0R.w
ge r1.x, l(2.000000), |r1.x|
movc r1.xyz, r1.xxxx, r4.xyzx, r0.yzwy
movc r0.yzw, bVehColl0Enabled, r1.xxyz, r0.yyzw  //contribute vehicle collision 0

add r1.x, -r0.w, _vecVehColl1R.w
add r1.yz, r0.yyzy, -_vecVehColl1B.xxyx
dp2 r1.y, _vecVehColl1M.xyxx, r1.yzyy
mul_sat r1.y, r1.y, _vecVehColl1M.w
mad r1.yz, r1.yyyy, _vecVehColl1M.xxyx, _vecVehColl1B.xxyx
add r1.yz, r0.yyzy, -r1.yyzy
dp2 r1.w, r1.yzyy, r1.yzyy
add r2.w, -r1.w, _vecVehColl1R.y
mul_sat r2.w, r2.w, _vecVehColl1R.z
mad r1.x, r2.w, r1.x, r0.w
mul r2.w, r2.w, _vecVehColl1R.x
mul r3.x, _vecVehColl1R.x, l(0.500000)
mul r3.x, r3.x, r3.x
lt r3.x, r1.w, r3.x
rsq r1.w, r1.w
mul r1.yz, r1.wwww, r1.yyzy
mul r1.yz, r1.yyzy, r2.wwww
mad r4.xy, r1.yzyy, v3.zzzz, r0.yzyy
add r1.y, _vecVehColl1R.w, l(-0.250000)
movc r4.z, r3.x, r1.y, r1.x
add r1.x, r0.w, -_vecVehColl1R.w
ge r1.x, l(2.000000), |r1.x|
movc r1.xyz, r1.xxxx, r4.xyzx, r0.yzwy
movc r0.yzw, bVehColl1Enabled, r1.xxyz, r0.yyzw  //contribute vehicle collision 1

add r1.x, -r0.w, _vecVehColl2R.w
add r1.yz, r0.yyzy, -_vecVehColl2B.xxyx
dp2 r1.y, _vecVehColl2M.xyxx, r1.yzyy
mul_sat r1.y, r1.y, _vecVehColl2M.w
mad r1.yz, r1.yyyy, _vecVehColl2M.xxyx, _vecVehColl2B.xxyx
add r1.yz, r0.yyzy, -r1.yyzy
dp2 r1.w, r1.yzyy, r1.yzyy
add r2.w, -r1.w, _vecVehColl2R.y
mul_sat r2.w, r2.w, _vecVehColl2R.z
mad r1.x, r2.w, r1.x, r0.w
mul r2.w, r2.w, _vecVehColl2R.x
mul r3.x, _vecVehColl2R.x, l(0.500000)
mul r3.x, r3.x, r3.x
lt r3.x, r1.w, r3.x
rsq r1.w, r1.w
mul r1.yz, r1.wwww, r1.yyzy
mul r1.yz, r1.yyzy, r2.wwww
mad r4.xy, r1.yzyy, v3.zzzz, r0.yzyy
add r1.y, _vecVehColl2R.w, l(-0.250000)
movc r4.z, r3.x, r1.y, r1.x
add r1.x, r0.w, -_vecVehColl2R.w
ge r1.x, l(2.000000), |r1.x|
movc r1.xyz, r1.xxxx, r4.xyzx, r0.yzwy
movc r0.yzw, bVehColl2Enabled, r1.xxyz, r0.yyzw  //contribute vehicle collision 2

add r1.x, -r0.w, _vecVehColl3R.w
add r1.yz, r0.yyzy, -_vecVehColl3B.xxyx
dp2 r1.y, _vecVehColl3M.xyxx, r1.yzyy
mul_sat r1.y, r1.y, _vecVehColl3M.w
mad r1.yz, r1.yyyy, _vecVehColl3M.xxyx, _vecVehColl3B.xxyx
add r1.yz, r0.yyzy, -r1.yyzy
dp2 r1.w, r1.yzyy, r1.yzyy
add r2.w, -r1.w, _vecVehColl3R.y
mul_sat r2.w, r2.w, _vecVehColl3R.z
mad r1.x, r2.w, r1.x, r0.w
mul r2.w, r2.w, _vecVehColl3R.x
mul r3.x, _vecVehColl3R.x, l(0.500000)
mul r3.x, r3.x, r3.x
lt r3.x, r1.w, r3.x
rsq r1.w, r1.w
mul r1.yz, r1.wwww, r1.yyzy
mul r1.yz, r1.yyzy, r2.wwww
mad r4.xy, r1.yzyy, v3.zzzz, r0.yzyy
add r1.y, _vecVehColl3R.w, l(-0.250000)
movc r4.z, r3.x, r1.y, r1.x
add r1.x, r0.w, -_vecVehColl3R.w
ge r1.x, l(2.000000), |r1.x|
movc r1.xyz, r1.xxxx, r4.xyzx, r0.yzwy
movc r0.yzw, bVehColl3Enabled, r1.xxyz, r0.yyzw  //contribute vehicle collision 3

mul r1.xyzw, r0.zzzz, gWorldViewProj[1].xyzw
mad r1.xyzw, r0.yyyy, gWorldViewProj[0].xyzw, r1.xyzw
mad r1.xyzw, r0.wwww, gWorldViewProj[2].xyzw, r1.xyzw  //screen transform

mov o3.xyz, r0.yzwy
add o0.xyzw, r1.xyzw, gWorldViewProj[3].xyzw  //main position output
mov o1.w, v3.x

mul r0.yzw, v1.yyyy, gInstanceVars[r0.x + 1].xxyz   //rotate normals with instance matrix
mad r0.yzw, v1.xxxx, gInstanceVars[r0.x + 0].xxyz, r0.yyzw
mad r0.xyz, v1.zzzz, gInstanceVars[r0.x + 2].xyzx, r0.yzwy
dp3 r1.x, r0.xyzx, r5.xywx
dp3 r1.y, r0.zxyz, r3.yzwy
dp3 r1.z, r0.xyzx, r2.xyzx
dp3 r0.x, r1.xyzx, r1.xyzx
rsq r0.x, r0.x

mul o2.xyz, r0.xxxx, r1.xyzx  //lighting stuff
mov o2.w, v4.x  //main texcoords
mov o3.w, v4.y


ret 
*/
// Approximately 224 instruction slots used














