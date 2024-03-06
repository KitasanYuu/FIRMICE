Shader "HRP/Effect/TVDistortion2"
{
    Properties
    {

        [Enum(UnityEngine.Rendering.BlendMode)]_BlendSrc("混合源乘子",int) = 0
        [Enum(UnityEngine.Rendering.BlendMode)]_BlendDst("混合目标乘子",int) = 0
        [Enum(UnityEngine.Rendering.BlendOp)]_BlendOp("混合运算方式",int) = 0

        [HDR]_AdjustColor ("调整用颜色", Color) = (0.5,0.5,0.5,0.5)
        _MainTex("MainTex", 2D) = "white" { }
        _DistortionTex("Distortion Tex (RG)", 2D) = "gray" { }
        _DistortionFrequency("Distortion Frequency", Float) = 1
        _DistortionAmplitude("Distortion Amplitude", Range(0, 1)) = 1
        _DistortionAnmSpeed("Distortion Animation Speed", Float) = 1
        _ColorScatterStrength("Color Scatter Strength", Range(-0.1, 0.1)) = 0.01

        _PulseFrequency ("脉冲频率", Range (0, 10)) = 2
        _PulsePercent ("脉冲时长", Range (0, 1)) = 0.5
    }


    SubShader

    {
        Tags
        {
            "RenderType" = "Transparent" "Queue" = "Transparent"
        }

        Pass
        {

            Cull Off
            ZWrite Off
            BlendOp [_BlendOp]
            Blend [_BlendSrc] [_BlendDst]
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Assets/Arts/TA/Shaders/Features/FOE_Function.hlsl"
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal
            #pragma target 2.0

            struct appdata_particle
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f_particle
            {
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float4 viewPos : TEXCOORD2;
                float2 uv : TEXCOORD3;
                float4 viewDir : TEXCOORD4;
            };

            int _BlendSrc, _BlendDst, _BlendOp;

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _AdjustColor;
            sampler2D _DistortionTex;
            float _DistortionFrequency;
            float _DistortionAmplitude;
            float _DistortionAnmSpeed;
            float _ColorScatterStrength;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            float _NoiseAnmSpeed;
            float _NoiseStrength;

            float _PulseFrequency;
            float _PulsePercent;

            v2f_particle vert(appdata_particle v)
            {
                v2f_particle o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv.xy = v.uv.xy;

                return o;
            }

            float RandomX(float x){
                return frac(sin(x*1000)*1000);
            }

            float4 frag(v2f_particle i) : SV_Target
            {

                float2 distortUV;
                distortUV.x = (_Time.y * _DistortionAnmSpeed);
                distortUV.y = i.uv.y*_DistortionFrequency;
                half4 distortionCol = tex2D(_DistortionTex, distortUV.xy);

                float2 mainUV = i.uv.xy;
                float randY = RandomX(floor(i.uv.y*100));
                //幅度增加节奏变化
                half yArea = smoothstep(0, 0.5, i.uv.y)-smoothstep(0.5, 1, i.uv.y);
                float rhythm = step(frac(_Time.y*_PulseFrequency), _PulsePercent);


                half disturbanceAmplitude = _DistortionAmplitude*yArea*0.2;
                disturbanceAmplitude = lerp(disturbanceAmplitude, _DistortionAmplitude, rhythm);

                mainUV.x += (randY-0.5)*disturbanceAmplitude*distortionCol.r;

                float4 color = float4(0, 0, 0, 0);

                float2 ColorStrength = distortionCol.y*float2(_ColorScatterStrength, 0.0);
                ColorStrength.x = lerp(0, ColorStrength, rhythm);
                
                float4 redOffset = tex2D(_MainTex, (mainUV + ColorStrength));
                color.xw = redOffset.xw;
                float4 greenOffset = tex2D(_MainTex, mainUV);
                color.yw = (color.yw + greenOffset.yw);
                float4 blueOffset = tex2D(_MainTex, mainUV - ColorStrength);
                color.zw = (color.zw + blueOffset.zw);

                color.w = saturate(color.w);
                color.rgb = lerp(color.xyz, color.xyz * _AdjustColor.rgb, disturbanceAmplitude);

                return color;












                /*

                float4 distortUV;
                distortUV.x = (_Time.y * _DistortionAnmSpeed);
                distortUV.y = (i.uv.y * _DistortionFrequency);
                float4 color = float4(0, 0, 0, 0);

                half4 distortionCol = tex2D(_DistortionTex, distortUV.xy);
                
                half disturbanceAmplitude = _DistortionAmplitude;
                half2 offset = (distortionCol - 0.498).xy * disturbanceAmplitude;
                offset.y = 0;

                float2 ColorStrength = 2*distortionCol.y*float2(_ColorScatterStrength, 0.0);

                float4 redOffset = tex2D(_MainTex, ((i.uv.xy + offset) + ColorStrength));
                color.xw = redOffset.xw;
                float4 greenOffset = tex2D(_MainTex, i.uv.xy + offset);
                color.yw = (color.yw + greenOffset.yw);
                float4 blueOffset = tex2D(_MainTex, (i.uv.xy + offset) - ColorStrength);
                color.zw = (color.zw + blueOffset.zw);
                
                color.w = saturate(color.w);

                color.xyz = color.xyz * _AdjustColor;
                
                return color;
                */
            }
            ENDHLSL
        }
    }
}