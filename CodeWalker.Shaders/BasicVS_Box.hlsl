#include "BasicVS.hlsli"

struct VS_INPUT
{
    float4 Position  : POSITION;
    float3 Normal    : NORMAL;
};

struct RenderableBox
{
    float3 Corner;
    uint Colour;
    float3 Edge1;
    float Pad1;
    float3 Edge2;
    float Pad2;
    float3 Edge3;
    float Pad3;
};

StructuredBuffer<RenderableBox> Boxes : register(t1);


VS_OUTPUT main(VS_INPUT input, uint iid : SV_InstanceID)
{
    VS_OUTPUT output;

    RenderableBox box = Boxes[iid];

    //float3 mpos = input.Position.xyz * box.Size;
    //float3 ipos = mulvq(mpos, box.Orientation);
    //float3 inorm = mulvq(input.Normal, box.Orientation);

    float3 p = input.Position.xyz;
    float3 n = input.Normal;
    //float3 s = float3(box.Length1, box.Length2, box.Length3);
    float3 ipos = (p.x*box.Edge1 + p.y*box.Edge2 + p.z*box.Edge3);// *s;
    float3 inorm = normalize(n.x*box.Edge1 + n.y*box.Edge2 + n.z*box.Edge3);

    float3 spos = (box.Corner + ipos) * Scale;
    float3 rpos = mulvq(spos, Orientation);
    float3 opos = CamRel.xyz + rpos;// bpos ModelTransform(input.Position.xyz);
    float4 cpos = ScreenTransform(opos);
    float3 bnorm = inorm;// mulvq(inorm, Orientation));// NormalTransform(input.Normal);
    float3 btang = 0.5;// NormalTransform(float3(1, 0, 0)); //no tangent to use on this vertex type...

    float4 c = Unpack4x8UNF(box.Colour).abgr;
    //c = float4(abs(input.Normal),1);

    float4 lightspacepos;
    float shadowdepth = ShadowmapSceneDepth(opos, lightspacepos);
    output.LightShadow = lightspacepos;
    output.Shadows = float4(shadowdepth, 0,0,0);

    output.Position = cpos;
    output.CamRelPos = opos;
    output.Normal = bnorm;
    output.Texcoord0 = 0.5;// input.Texcoord0;
    output.Texcoord1 = 0.5;// input.Texcoord;
    output.Texcoord2 = 0.5;// input.Texcoord;
    output.Colour0 = c;// float4(0.5, 0.5, 0.5, 1); //input.Colour0;
    output.Colour1 = float4(0.5,0.5,0.5,1); //input.Colour1
    output.Tint = 0;
    output.Tangent = float4(btang, 1);
    output.Bitangent = float4(cross(btang, bnorm), 0);
    return output;
}

