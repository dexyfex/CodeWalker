StructuredBuffer<float> Target : register( t0 );
RWStructuredBuffer<float> Current : register( u0 );

cbuffer cb0
{
    float BlendFactor;
    float3 pad;
}

[numthreads(1, 1, 1)]
void main( uint3 DTid : SV_DispatchThreadID )
{
    float t = max(Target[0],0);
    float c = max(Current[0],0);
    Current[0] = c + ((t - c) * BlendFactor);
}