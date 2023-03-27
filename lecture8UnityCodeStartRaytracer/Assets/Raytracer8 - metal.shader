// Fra https://docs.unity3d.com/Manual/SL-VertexFragmentShaderExamples.html
//https://msdn.microsoft.com/en-us/library/windows/desktop/bb509640(v=vs.85).aspx
//https://msdn.microsoft.com/en-us/library/windows/desktop/ff471421(v=vs.85).aspx
// rand num generator http://gamedev.stackexchange.com/questions/32681/random-number-hlsl
// http://www.reedbeta.com/blog/2013/01/12/quick-and-easy-gpu-random-numbers-in-d3d11/
// https://docs.unity3d.com/Manual/RenderDocIntegration.html
// https://docs.unity3d.com/Manual/SL-ShaderPrograms.html

Shader "Unlit/SingleColor"
{
    SubShader
    {
        Pass
        {

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #define FLOAT_MAX 3.402823466e38F
            
            #include "hlsl/ray.hlsl"
            #include "hlsl/sphere.hlsl"
            #include "hlsl/camera.hlsl"
            #include "hlsl/hash.hlsl"
            #include "hlsl/matfunc.hlsl"
            #include "UnityCG.cginc"
            typedef vector <float, 3> vec3; // to get more similar code to book
            typedef vector <fixed, 3> col3;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float hit_sphere(float3 center, float radius, ray r)
            {
                float3 oc = r.origin() - center;
                float a = dot(r.direction(),r.direction());
                float b = 2.0 * dot(oc,r.direction());
                float c = dot(oc,oc) - radius*radius;
                float discrimination = b*b - 4*a*c;
                if(discrimination < 0)
                {
                    return -1;
                }
                
                return (-b - sqrt(discrimination)) / (2*a);
            }

            sphere getsphere(int i)
            {
                sphere sph;
                if (i == 0) { sph = make_sphere(float3( 0, 0, -1),0.5,create_material(0, float3(0.8, 0.3, 0.3)));
                }
                if (i == 3) { sph = make_sphere(float3( 0, -100.5, -1),100,create_material(0, float3(0.8, 0.8, 0.0)));
                }
                if (i == 2) { sph = make_sphere(float3( 1, 0, -1),0.5,create_material(1, float3(0.8, 0.6, 0.2),0.3));
                }
                if (i == 1) { sph = make_sphere(float3( -1, 0, -1),0.5,create_material(1, float3(0.8, 0.8, 0.8),1.0));
                }
                return sph;
            }
            
            float rand(in float2 uv)
            {
                float2 noise = (frac(sin(dot(uv, float2(12.9898, 78.233)*2.0)) * 43758.5453));
                return abs(noise.x + noise.y) * 0.5;
            }

            bool hit(ray r, float t_min, float t_max,out hit_record rec)
            {
                hit_record temp_rec;
                for(int i = 0; i<4;i++)
                {
                    if(getsphere(i).sphere_hit(r,t_min,t_max,temp_rec))
                    {
                        rec = temp_rec;
                        return true;
                    }
                }
                return false;
            }

            float3 color(ray r)
            {
                float tmax = FLOAT_MAX;
                hit_record rec;
                float3 col = float3(1,1,1);
                float max = 7;
                bool hit_world = hit(r,0.001,tmax,rec);
                if(hit_world)
                {
                    while(hit_world)
                    {
                        ray s;
                        float3 color;
                        if(scatter(r,rec,color,s) && max>0)
                        {
                            col *= color;
                            r = s;
                        }
                        else
                        {
                            col = 0;
                            break;
                        }
                        hit_world = hit(r,0.001,tmax,rec);
                        max--;
                    }
                    if(hit_world && max==0)
                        return col;
                }
                float3 unit_direction = unit_vector(r.direction());
                float t = 0.5*(unit_direction.y +1.0);
                return col*((1.0-t)*1.0 + t*float3(0.5,0.7,1));
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            fixed4 frag(v2f c) : SV_Target
            {
                int ns = 100;
                float3 col = float3(0.0,0.0,0.0);
                camera cam = make_camera();
                for(int s = 0;s<ns;s++)
                {
                    float u = c.uv.x + hash13(float3(s,ns,c.uv.x))/3000;
                    float v = c.uv.y + hash13(float3(ns,s,c.uv.y))/2000;
                    ray r = cam.get_rey(u,v);
                    float3 p = r.point_at_parameter(2.0);
                    col += color(r);
                }
                
                col /= float(ns);
                col = float3(sqrt(col.x),sqrt(col.y),sqrt(col.z));
                return fixed4(col, 1);
            }

            ////////////////////////////////////////////////////////////////////////////////////
            ENDHLSL

        }
    }
}