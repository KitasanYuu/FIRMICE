Shader "Peperoncino/Cam"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+1"}
        LOD 100

        Stencil {  
            Ref 128
            Comp always
            Pass replace  
        }
        Cull off
        ColorMask 0
        ZWrite Off
        ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : WORLD_POS;
            };


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                float d = length(_WorldSpaceCameraPos - i.worldPos);
                if(d>3)discard;
                return 0;
            }
            ENDCG
        }
    }
}