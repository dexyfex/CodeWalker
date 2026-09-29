struct VS_Output
{
    float4 Pos : SV_POSITION;              
    float2 Tex : TEXCOORD0;
};

VS_Output main( float4 pos : POSITION )
{
    VS_Output output;
    output.Pos = pos;
    output.Tex.x = (pos.x*0.5)+0.5;
    output.Tex.y = (pos.y*-0.5)+0.5;
    return output;
}