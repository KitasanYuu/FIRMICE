Shader "Peperoncino/InfoMoni"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Alpha ("Alpha", range(0, 1)) = 1

        _Num ("Num", float) = 0
        [HDR]_Color1("Color1", color) = (1,1,1,1)
        [HDR]_Color2("Color2", color) = (1,1,1,1)

        [IntRange]_MaxNum ("最大桁", range(1,15))=4 
        [IntRange]_FloatN ("小数第何位まで", range(0,5))=0 
        [KeywordEnum(Default, World, FPS)] _Calc("Calc", float) = 0

        _Diff ("_Diff", float) = 0

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
            // make fog work
            #pragma multi_compile_fog
            #pragma shader_feature_local _CALC_DEFAULT _CALC_WORLD _CALC_FPS

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
                float3 worldPos : WORLD_POS;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            float2 fixNum(float2 uv, float num){
                float n = frac(num/10)*10;
                uv.y -= n+1;
                uv.y = uv.y/11;
                return uv;
            }

            float _Num;
            half4 _Color1, _Color2;
            float _Diff;
            fixed _Alpha;

            int _FloatN, _MaxNum;
            half4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                half4 col = 0;
                half4 Color = _Color1;

                float DefNum = _Num;
                float CalcNum = DefNum;

                #ifdef _CALC_WORLD
                    DefNum = i.worldPos.y + _Diff;
                #elif _CALC_FPS
                    DefNum = unity_DeltaTime.w;
                #endif

                if(DefNum<0){
                    DefNum = 0-DefNum;
                    Color = _Color2;
                }

                float2 uv;
                fixed temp = 0;

                int num = CalcNum;

                float n = _FloatN+1;
                int s = _MaxNum;
                int flag =0; 
                for(int j=n-s;j<n;j++){
                    num = (DefNum*pow(10,j))%10;
                    if(num!=0|| j>-1)flag+=1;
                    if(flag>0){
                        uv = fixNum(i.uv, num);
                        uv.x*=n;
                        temp = tex2D(_MainTex, uv);
                        temp *= step((1/n)*(j+1), i.uv.x);
                        temp *= step(i.uv.x, (1/n)*(j+2));
                        col += temp;
                    }
                }

                col *= step(i.uv.y,1);
                clip(col.r-.1);
                col *= Color;

                col.a *= _Alpha;
                return col;
            }
            ENDCG
        }
    }
}