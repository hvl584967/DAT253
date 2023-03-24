#ifndef RAYHLSL
#define RAYHLSL

struct ray
{
    float3 a;
    float3 b;

    float3 origin()
    {
        return a;
    }

    float3 direction()
    {
        return b;
    }

    float3 point_at_parameter(const float t)
    {
        return a+t*b;
    }
};

ray make_ray(float3 a, float3 b)
{
    ray ray;
    ray.a = a;
    ray.b = b;
    return ray;
}

#endif
