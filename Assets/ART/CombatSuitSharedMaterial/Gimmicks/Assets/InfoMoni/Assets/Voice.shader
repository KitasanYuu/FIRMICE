Shader "Peperoncino/Voice"
{
    Properties
    {
        [HDR]_Color ("Color", color) = (1,1,1,1)
        _Alpha ("Alpha", range(0, 1)) = 1
        _Voice("Voice", range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        Blend One One
        LOD 100
        Stencil {
            Ref 128
            Comp Equal
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata v)
            {
                v2f o;

                // single pass instanced rendering
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed2 random2(fixed2 st){
                st = fixed2( dot(st,fixed2(127.1,311.7)),
                            dot(st,fixed2(269.5,183.3)) );
                return -1.0 + 2.0*frac(sin(st)*43758.5453123);
            }

            float perlinNoise(fixed2 st) 
            {
                fixed2 p = floor(st);
                fixed2 f = frac(st);
                fixed2 u = f*f*(3.0-2.0*f);

                float v00 = random2(p+fixed2(0,0));
                float v10 = random2(p+fixed2(1,0));
                float v01 = random2(p+fixed2(0,1));
                float v11 = random2(p+fixed2(1,1));

                return lerp( lerp( dot( v00, f - fixed2(0,0) ), dot( v10, f - fixed2(1,0) ), u.x ),
                            lerp( dot( v01, f - fixed2(0,1) ), dot( v11, f - fixed2(1,1) ), u.x ), 
                            u.y)+0.5f;
            }

            half4 _Color;
            fixed _Alpha;
            float _Voice;
            half4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float2 uv = i.uv;



                fixed t = (uv.y-perlinNoise(uv.x*50*_Voice+_Time.y*5*_Voice)+.5);
                t *= step(t,.5);
                t = step(.45,t);
                clip(t-.5);
                
                half4 col = t * _Color;
                col.a *= _Alpha;
                return col;
            }
            ENDCG
        }
    }
}