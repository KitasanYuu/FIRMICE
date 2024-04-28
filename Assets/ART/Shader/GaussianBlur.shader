Shader"Custom/GaussianBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSizeX ("Blur Size X", Float) = 1.0
        _BlurSizeY ("Blur Size Y", Float) = 1.0
        _Transparency ("Transparency", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
Blend SrcAlpha OneMinusSrcAlpha

        // This shader is for Gaussian blur.
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
    float weightSum = 0.0;

                // Gaussian weights for 7x7 kernel
    float weights[7] = { 0.0044, 0.0540, 0.2420, 0.3989, 0.2420, 0.0540, 0.0044 };

                // Horizontal blur
    for (int x = -3; x <= 3; x++)
    {
        float2 offset = float2(x * _BlurSizeX, 0);
        col += tex2D(_MainTex, i.uv + offset) * weights[x + 3];
        weightSum += weights[x + 3];
    }

                // Normalize by the sum of weights
    col /= weightSum;

                // Vertical blur
    for (int y = -3; y <= 3; y++)
    {
        float2 offset = float2(0, y * _BlurSizeY);
        col += tex2D(_MainTex, i.uv + offset) * weights[y + 3];
                    // No need to accumulate weightSum here, we already did it for horizontal blur
    }

                // Normalize by the sum of weights
    col /= weightSum;

                // Apply transparency
    col.a *= _Transparency;

    return col;
}
            ENDCG
        }
    }
}
