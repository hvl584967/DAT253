#ifndef HITABLE
#define HITABLE
struct hit_record
{
    float t;
    float3 p;
    float3 normal;
};
#endif
