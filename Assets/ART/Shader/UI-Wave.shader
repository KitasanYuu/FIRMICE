Shader "Akari/UI/WAVE" {
    Properties {
        [Toggle(_UseScriptPass)] _UseScriptPass("Use Script Pass", Float) = 0 // 0 表示未选中, 1 表示选中
        _Color ("Color", Color) = (1,1,1,1)
        _TargetColor("TOColor", Color) = (1,1,1,1)
        _ControlParameter ("Control Parameter", Range(0, 1.34)) = 0
        _Angel ("Angle", Range(45, 135)) = 45
        _ShapeKey ("ShapeKey", Range(0, 20)) = 2
        _Tilling ("Tilling", Range(1, 5)) = 1
        _WaveSpacing ("Wave Spacing", Range(0, 1)) = 1
        _CanvasAspectRatio("Canvas Aspect Ratio", Float) = 1.0
    }
    SubShader {
        Tags {"QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="true"}
        Pass {
            Name"MAIN"
            Tags {"QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="true"}
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
#include "UnityCG.cginc"

struct appdata_t
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct v2f
{
    float2 uv : TEXCOORD0;
    UNITY_FOG_COORDS(1)
    float4 vertex : SV_POSITION;
};

float _UseScriptPass;
float4 _Color;
float4 _TargetColor;
float _ControlParameter;
float _Angel;
float _Tilling;
float _WaveSpacing;
float _ShapeKey;
float _CanvasAspectRatio;

v2f vert(appdata_t v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    UNITY_TRANSFER_FOG(o, o.vertex);
    return o;
}

fixed4 frag(v2f i) : SV_Target {
    float UseScriptPass =  _UseScriptPass;
    float aspect;
    if(_UseScriptPass > 0.5)
        aspect = _CanvasAspectRatio; 
    else
        aspect = _ScreenParams.y / _ScreenParams.x; 
    float Angle = _Angel;
    float value;
    float ShapeKey = _ShapeKey;
    float Tilling = _Tilling;
    float4 Color = _Color; 
    float4 TargetColor = _TargetColor;
    float2 uv = i.uv;
    float ControlParameter = _ControlParameter;
    float rot = radians(Angle);
    // 缩放操作
    float2 scaledUV = float2((uv.x - 0.5), (uv.y - 0.5) * aspect);
    
    // 旋转操作
    float2x2 m = float2x2(cos(rot), -sin(rot), sin(rot), cos(rot));
    float2 rotatedUV = mul(m, scaledUV);
    
    // 计算位置
    float2 pos = 10.0 * rotatedUV + float2(0.5, 0.5);
    
    float2 rep = frac(pos * Tilling); // 使用Tilling参数调整网格单元数量
    float dist = 2.0 * min(min(rep.x, 1.0-rep.x), min(rep.y, 1.0-rep.y));
    float squareDist = length((floor(pos) + 0.5) - 0.5);
    float edge = sin(ControlParameter*3 - squareDist * 0.5 * Tilling) * 0.5 + 0.5;
    edge = (ControlParameter*3 - squareDist * 0.5 * Tilling) * 0.5;
    edge = ShapeKey * frac(edge * 0.5);
    value = frac(dist * 2.0);
    value = lerp(value, 1.0 - value, step(1.0, edge));
    edge = pow(abs(1.0 - edge), 2.0);
    value = smoothstep(edge - 0.05, edge, 0.95 * value);
    value += squareDist * 0.1;
    // 添加波浪之间的间隔
    float waveSpacing = _WaveSpacing * Tilling;
    value = step(waveSpacing, value);
    
    return lerp(TargetColor, Color, value);
}

            ENDCG
        }
    }
}
