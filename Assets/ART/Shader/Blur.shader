Shader "Custom/Blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSizeX ("Blur Size X", Float) = 0.002
        _BlurSizeY ("Blur Size Y", Float) = 0.002
        _Transparency ("Transparency", Range(0,1)) = 0
        _Brightness ("Brightness", Range(0,10)) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

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

            sampler2D _MainTex;
            float _BlurSizeX;
            float _BlurSizeY;
            float _Transparency;
            float _Brightness;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 col = float4(0, 0, 0, 0);

                // Loop for horizontal and vertical blur
                for (int x = -3; x <= 3; x++)
                {
                    for (int y = -3; y <= 3; y++)
                    {
                        float2 offset = float2(x * _BlurSizeX, y * _BlurSizeY);
                        col += tex2D(_MainTex, i.uv + offset);
                    }
                }

                col /= 49.0; // Normalize by the number of samples

                // Apply brightness
                col.rgb *= _Brightness;

                // Apply transparency
                col.a *= _Transparency;

                return col;
            }
            ENDCG
        }
    }
}
