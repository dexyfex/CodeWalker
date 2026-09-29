Texture2D<float4> Colourmap : register(t0);
SamplerState TextureSS : register(s0);

cbuffer PSGeomVars : register(b0)
{
    uint EnableTexture;
    uint EnableTint;
    uint IsDecal;
    uint Pad3;
}


struct VS_OUTPUT
{
    float4 Position : SV_POSITION;
    //float3 Normal   : NORMAL;
    float2 Texcoord : TEXCOORD0;
    //float4 Colour   : COLOR0;
    //float4 Tint     : COLOR1;
};


float4 main(VS_OUTPUT input) : SV_TARGET
{
    if (EnableTexture == 1)
    {
        float4 c = Colourmap.Sample(TextureSS, input.Texcoord);
        if (EnableTint == 2) { c.a = 1; }
        if ((IsDecal == 0) && (c.a <= 0.33)) discard;
        if ((IsDecal == 1) && (c.a <= 0.0)) discard;
    }
	return float4(1.0f, 1.0f, 1.0f, 1.0f);
}