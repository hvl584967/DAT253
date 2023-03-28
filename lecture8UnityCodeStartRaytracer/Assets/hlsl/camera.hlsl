#ifndef CAMERA
#define CAMERA
#include "ray.hlsl"
#include "matfunc.hlsl"

struct camera
{
    float3 lower_left_corner;
    float3 horizontal;
    float3 vertical;
    float3 origin;

    ray get_rey(float u,float v)
    {
        return make_ray(origin,lower_left_corner + u*horizontal + v*vertical -origin);
    }
};

camera make_camera()
{
    camera cam;
    cam.lower_left_corner = float3(-2.0,-1.0,-1.0);
    cam.horizontal = float3(4.0,0.0,0.0);
    cam.vertical = float3(0.0,2.0,0.0);
    cam.origin = float3(0.0,0.0,0.0);
    return cam;
}

camera cam(float3 lookfrom,float3 lookat,float3 vup,float vfov, float aspect)
{
    camera cam = make_camera();
    float3 u, v, w;
    const float pi = 3.14159265f;
    float theta = vfov*pi/180;
    float half_height = tan(theta/2);
    float half_width = aspect * half_height;
    cam.origin = lookfrom;
    w = unit_vector(lookfrom - lookat);
    u = unit_vector(cross(vup,w));
    v = cross(w,u);
    cam.lower_left_corner = float3(-half_width,-half_height,-1.0);
    cam.lower_left_corner = cam.origin - half_width*u - half_height*v -w;
    cam.horizontal = 2*half_width*u;
    cam.vertical = 2*half_height*v;
    return cam;
}

#endif