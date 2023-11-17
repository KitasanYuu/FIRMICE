Shader "Peperoncino/Circle"
{
    Properties
    {
        [HDR]_Color1 ("Color1", color) = (1,1,1,1)
        [HDR]_Color2 ("Color2", color) = (1,1,1,1)
        _Alpha ("Alpha", range(0, 1)) = 1
        _CircleRange ("Range", Range(0, 1)) = 1
        _CircleEdge ("Edge", Range(0, 1)) = 0.5
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
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            float _CircleRange,_CircleEdge;
            half4 _Color1, _Color2;
            fixed _Alpha;

            half4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				float2 uv = i.uv;
                float2 uv2 = (uv*2 - 1);
				float CircleRange = 1 - ceil(((1 - ((atan2(uv2.y,uv2.x)/(UNITY_PI*2))+0.5))-_CircleRange));
				float CircleEdge = floor((_CircleEdge+length(uv2)))*(1.0 - floor(length(uv2)));
                clip((CircleRange * CircleEdge) - 0.5);

                half4 col = lerp(_Color1,_Color2,_CircleRange);
                col.a *= _Alpha;
                return col;
            }
            ENDCG
        }
    }
}