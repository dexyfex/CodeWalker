

cbuffer SceneVars : register(b0)
{
    float4x4 ViewProj;
    uint Mode; //0=Vertices, 1=Arc
    float Size; //world units
    float SegScale; //arc angle / number of segments
    float SegOffset; //angle offset of arc
    float3 CamRel; //center position
    uint CullBack; //culls pixels behind 0,0,0
    float4 Colour; //colour for arc
    float3 Axis1; //axis 1 of arc
    float WidgetPad2;
    float3 Axis2; //axis 2 of arc
    float WidgetPad3;
}

struct WidgetShaderVertex
{
    float4 Position;
    float4 Colour;
};

StructuredBuffer<WidgetShaderVertex> Vertices : register(t0);

struct VS_OUTPUT
{
    float4 Position  : SV_POSITION;
    float4 Colour : COLOR0;
    float CullValue : TEXCOORD0;
};

VS_OUTPUT main(uint id : SV_VertexID)
{
    float3 ipos;
    float4 colour;
    float cull;
    if (Mode == 0) //Vertices
    {
        ipos = CamRel + Vertices[id].Position.xyz;
        colour = Vertices[id].Colour;
        cull = 1;
    }
    else //(Mode == 1) //Arc
    {
        float a = SegOffset + (id * SegScale);
        float3 a1 = Axis1 * sin(a);
        float3 a2 = Axis2 * cos(a);
        ipos = CamRel + (a1 + a2) * Size;
        colour = Colour;
        cull = (CullBack == 1) ? ((length(CamRel) - length(ipos)) / Size) : 1;
    }

    float4 opos = mul(float4(ipos, 1), ViewProj);

    VS_OUTPUT output;
    output.Position = opos;
    output.Colour = colour;
    output.CullValue = cull;
	return output;
}

