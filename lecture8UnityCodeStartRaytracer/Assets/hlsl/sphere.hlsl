#ifndef SPHERE
#define SPHERE

#include "ray.hlsl"
#include "hitable.hlsl"

struct sphere
{
    float3 center;
    float radius;

    bool sphere_hit(ray r, float t_min,float t_max,out hit_record rec)
    {
        float3 oc = r.origin() - center;
        float a = dot(r.direction(),r.direction());
        float b = dot(oc,r.direction());
        float c = dot(oc,oc) - radius*radius;
        float discrimination = b*b - a*c;
        if(discrimination > 0)
        {
            float temp = (-b - sqrt(b*b-a*c))/a;
            if(temp < t_max && temp > t_min)
            {
                rec.t = temp;
                rec.p = r.point_at_parameter(rec.t);
                rec.normal = (rec.p - center) / radius;
                return true;
            }
            temp = (-b + sqrt(b*b-a*c))/a;
            if(temp < t_max && temp > t_min)
            {
                rec.t = temp;
                rec.p = r.point_at_parameter(rec.t);
                rec.normal = (rec.p - center)/radius;
                return true;
            }
        }
        return false;
    }
};

sphere make_sphere(float3 center,float radius,int materialtype,float3 matproperties)
{
    sphere sphere;
    sphere.center = center;
    sphere.radius = radius;
    //sphere.materialtype = materialtype;
    //sphere.matproperties.xyz = matproperties;
    return sphere;
}
#endif