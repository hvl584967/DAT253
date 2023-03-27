#ifndef HITABLE
#define HITABLE

#include "material.hlsl"

struct hit_record
{
    float t;
    float3 p;
    float3 normal;
    material mat;
};

hit_record make_record()
{
    hit_record rec;
    rec.t  = 0.0;
    rec.p = float3(0,0,0);
    rec.normal = float3(0,0,0);
    material mat;
    rec.mat = mat;
    return rec;
}
#endif
