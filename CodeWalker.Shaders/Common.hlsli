


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
    float naturalDiffuseFactor = 1.0;
    float artificialDiffuseFactor = saturate(vc0.g);
    c *= BasicLighting(globalLights.LightDirColour, globalLights.LightDirAmbColour, lf);
    c += AmbientLight(fc, norm.z, globalLights.LightNaturalAmbUp, globalLights.LightNaturalAmbDown, naturalDiffuseFactor);
    c += AmbientLight(fc, norm.z, globalLights.LightArtificialAmbUp, globalLights.LightArtificialAmbDown, artificialDiffuseFactor);
    return c;
}





























