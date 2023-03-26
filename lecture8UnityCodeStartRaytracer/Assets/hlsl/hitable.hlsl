#ifndef HITABLE
#define HITABLE
struct hit_record
{
    float t;
    float3 p;
    float3 normal;
};

hit_record make_record()
{
    hit_record rec;
    rec.t  = 0.0;
    rec.p = float3(0,0,0);
    rec.normal = float3(0,0,0);
    return rec;
}
#endif
