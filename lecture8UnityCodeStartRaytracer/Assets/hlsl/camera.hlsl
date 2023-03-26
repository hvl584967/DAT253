#ifndef CAMERA
#define CAMERA
#include "ray.hlsl"

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

#endif