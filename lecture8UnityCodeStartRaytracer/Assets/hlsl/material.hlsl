#ifndef MATERIAL
#define MATERIAL

#include "ray.hlsl"
#include "hitable.hlsl"

struct material
{
    uint type;
    float3 property;
    float fuzz;
    float ridx;
};

material create_material(uint type,float3 property)
{
    material mat;
    mat.type = type;
    mat.property = property;
    mat.fuzz = 0;
    mat.ridx = 0;
    return mat;
}

material create_material(uint type,float3 property,float refrac)
{
    material mat;
    mat.type = type;
    mat.property = property;
    if(type == 1)
    {
        mat.fuzz = refrac;
        mat.ridx = 0;
    }
    if(type == 2)
    {
        mat.fuzz = 0;
        mat.ridx = refrac;
    }
    
    return mat;
}

material create_material(uint type,float3 property,float fuzz,float ridx)
{
    material mat;
    mat.type = type;
    mat.property = property;
    mat.fuzz = fuzz;
    mat.ridx = ridx;
    return mat;
}

#endif