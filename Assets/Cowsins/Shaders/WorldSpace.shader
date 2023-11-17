
Shader "Unlit/WorldSpace" {
    Properties{

        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Base Color", 2D) = "white" {}
        _UVs("UV Scale", float) = 1.0

    }

        SubShader{
            Tags { "RenderType" = "Opaque" }
            LOD 300

            CGPROGRAM
            #pragma surface surf StandardSpecular fullforwardshadows vertex:vert

            #pragma target 3.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            float _UVs;

                    struct Input {

                            float2 uv_MainTex;
                            float3 worldPos;
                            float3 worldNormal;

                    };

                    void vert(inout appdata_full v) { }


                         UNITY_INSTANCING_BUFFER_START(Props)
                         UNITY_INSTANCING_BUFFER_END(Props)

                         void surf(Input IN, inout SurfaceOutputStandardSpecular o) {


                             //---- World Space -----

                                 float3 Pos = IN.worldPos / (-1.0 * abs(_UVs));

                                 float3 c00 = tex2D(_MainTex, IN.worldPos / 10);

                                 float3 c1 = tex2D(_MainTex, Pos.yz).rgb;
                                 float3 c2 = tex2D(_MainTex, Pos.xz).rgb;
                                 float3 c3 = tex2D(_MainTex, Pos.xy).rgb;

                                 float alpha21 = abs(IN.worldNormal.x);
                                 float alpha23 = abs(IN.worldNormal.z);

                                 float3 c21 = lerp(c2, c1, alpha21).rgb;
                                 float3 c23 = lerp(c21, c3, alpha23).rgb;

                                 //---- Base Color Adjustment -----

                                 fixed3 c = c23 * _Color;
                                 o.Albedo = c23;


                             }
                             ENDCG
        }
            FallBack "Diffuse"
}

