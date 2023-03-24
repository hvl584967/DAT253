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
            #include "hlsl/ray.hlsl"
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

            float3 unit_vector(float3 v)
            {
                return v / length(v);
            }

            bool hit_sphere(float3 center, float radius, ray r)
            {
                float3 oc = r.origin() - center;
                float a = dot(r.direction(),r.direction());
                float b = 2.0 * dot(oc,r.direction());
                float c = dot(oc,oc) - radius*radius;
                float discrimination = b*b - 4*a*c;
                return discrimination > 0;
            }

            float3 color(ray r)
            {
                if(hit_sphere(float3(0,0,-1),0.5,r))
                    return float3(1,0,0);
                float3 unit_direction = unit_vector(r.direction());
                float t = 0.5*(unit_direction.y +1.0);
                return (1.0-t)*float3(1,1,1) + t*float3(0.5,0.7,1);
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            float3 frag(v2f c) : SV_Target
            {
                float3 lower_left_corner = {-2, -1, -1};
                float3 horizontal = {4, 0, 0};
                float3 vertical = {0, 2, 0};
                float3 origin = {0, 0, 0};
                float u = c.uv.x;
                float v = c.uv.y;
                ray r = make_ray(origin, lower_left_corner + u * horizontal + v * vertical);
                float3 col = color(r);
                return fixed4(col, 1);
            }

            ////////////////////////////////////////////////////////////////////////////////////
            ENDHLSL

        }
    }
}