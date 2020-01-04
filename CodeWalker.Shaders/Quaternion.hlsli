



float3 mulvq(float3 v, float4 q)
{
    float3 u = q.xyz;
    float s = q.w;
    return (dot(u, v)*u*2.0f) + (s*s - dot(u, u)) * v + (cross(u, v)*s*2.0f);
}

