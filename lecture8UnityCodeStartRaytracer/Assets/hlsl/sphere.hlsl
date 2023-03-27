#ifndef SPHERE
#define SPHERE

#include "ray.hlsl"
#include "hitable.hlsl"
#include "material.hlsl"

struct sphere
{
    float3 center;
    float radius;
    material mat;

    bool sphere_hit(ray r, float t_min,float t_max,out hit_record rec)
    {
        float3 oc = r.origin() - center;
        float a = dot(r.direction(),r.direction());
        float b = 2.0*dot(oc,r.direction());
        float c = dot(oc,oc) - radius*radius;
        float discrimination = b*b - 4*a*c;
        if(discrimination > 0)
        {
            float temp = (-b - sqrt(b*b-4*a*c))/(2*a);
            if(temp < t_max && temp > t_min)
            {
                rec.t = temp;
                rec.p = r.point_at_parameter(rec.t);
                rec.normal = (rec.p - center) / radius;
                rec.mat = mat;
                return true;
            }
            temp = (-b + sqrt(b*b-4*a*c))/(2*a);
            if(temp < t_max && temp > t_min)
            {
                rec.t = temp;
                rec.p = r.point_at_parameter(rec.t);
                rec.normal = (rec.p - center)/radius;
                rec.mat = mat;
                return true;
            }
        }
        return false;
    }
};

sphere make_sphere(float3 center,float radius)
{
    sphere sphere;
    sphere.center = center;
    sphere.radius = radius;
    sphere.mat = create_material(0,float3(0,0,0));
    return sphere;
}

sphere make_sphere(float3 center,float radius,material mat)
{
    sphere sphere;
    sphere.center = center;
    sphere.radius = radius;
    sphere.mat = mat;
    return sphere;
}
#endif