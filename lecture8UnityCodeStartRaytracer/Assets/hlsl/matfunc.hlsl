#ifndef MATFUNC
#define MATFUNC

#include "ray.hlsl"
#include "hitable.hlsl"
#include "hash.hlsl"

bool scatter_lambertian(ray r_in,hit_record rec,out float3 attenuation,out ray scattered);
bool scatter_metal(ray r_in,hit_record rec,out float3 attenuation,out ray scattered);
bool scatter_dielectric(ray r_in,hit_record rec,out float3 attenuation,out ray scattered);

bool scatter(ray r_in,hit_record rec,out float3 attenuation,out ray scattered)
{
    switch (rec.mat.type)
    {
    case 0:
        {
            return scatter_lambertian(r_in,rec,attenuation,scattered);
        }
    case 1:
        {
            return scatter_metal(r_in,rec,attenuation,scattered);
        }
    case 2:
        {
            return scatter_dielectric(r_in,rec,attenuation,scattered);
        }
        default:
        {
            return false;
        }
    }
}

float3 unit_vector(float3 v)
{
    return v / length(v);
}

float3 reflect(float3 v,float3 n)
{
    return v-2*dot(v,n)*n;
}

bool refract(float3 v, float3 n, float ni_over_nt,out float3 refracted)
{
    float3 uv = unit_vector(v);
    float dt = dot(uv,n);
    float dis = 1.0-ni_over_nt*ni_over_nt*(1-dt*dt);
    if(dis > 0)
    {
        refracted = ni_over_nt*(uv-n*dt)-n*sqrt(dis);
        return true;
    }
    return false;
}

float schlick(float cosine,float ridx)
{
    float r0 = (1-ridx)/(1+ridx);
    r0 = r0*r0;
    return r0 + (1-r0)*pow((1-cosine),5);
}

bool scatter_lambertian(ray r_in,hit_record rec,out float3 attenuation,out ray scattered)
{
    float3 target = rec.p + rec.normal + random_unit_sphere(123.123);
    scattered = make_ray(rec.p,target-rec.p);
    attenuation = rec.mat.property;
    return true;
}
bool scatter_metal(ray r_in,hit_record rec,out float3 attenuation,out ray scattered)
{
    float3 reflected = reflect(r_in.direction(),rec.normal);
    scattered = make_ray(rec.p,reflected + rec.mat.fuzz*random_unit_sphere(123.123));
    attenuation = rec.mat.property;
    return dot(scattered.direction(),rec.normal)>0;
}
bool scatter_dielectric(ray r_in,hit_record rec,out float3 attenuation,out ray scattered)
{
    float3 out_normal;
    float3 reflected = reflect(r_in.direction(),rec.normal);
    float ni_over_nt;
    attenuation = float3(1.0,1.0,1.0);
    float3 refracted;
    float refprob;
    float cosine;
    if(dot(r_in.direction(),rec.normal) > 0)
    {
        out_normal = -rec.normal;
        ni_over_nt = rec.mat.ridx;
        cosine = rec.mat.ridx*dot(r_in.direction(),rec.normal) / length(r_in.direction());
    }else
    {
        out_normal = rec.normal;
        ni_over_nt = 1.0/rec.mat.ridx;
        cosine = dot(r_in.direction(),rec.normal) / length(r_in.direction());
    }
    if(refract(r_in.direction(),out_normal,ni_over_nt,refracted))
    {
        refprob = schlick(cosine,rec.mat.ridx);
    }else
    {
        scattered = make_ray(rec.p,reflected);
        refprob = 1.0;
    }
    if(refract()
    {
        scattered = make_ray(rec.p,refracted);
    }else
    {
        scattered = make_ray(rec.p,reflected);
    }
    return true;
}

#endif