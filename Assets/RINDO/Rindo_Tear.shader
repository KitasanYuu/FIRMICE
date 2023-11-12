// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "HOD/Rindo_Tear"
{
	Properties
	{
		[Enum(UnityEngine.Rendering.CullMode)]_Culling("Culling", Int) = 2
		[HDR]_Nami("Nami", 2D) = "white" {}
		_Mask_1("Mask_1", 2D) = "white" {}
		_Mask_2("Mask_2", 2D) = "white" {}
		_Speed("Speed", Float) = 0.2
		_Tiliing("Tiliing ", Float) = 1
		_Wave("Wave", Float) = 10
		_Intensity("Intensity", Range( 0 , 1)) = 0.2
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Back
		CGPROGRAM
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf StandardCustomLighting alpha:fade keepalpha noshadow exclude_path:deferred 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldNormal;
			INTERNAL_DATA
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform int _Culling;
		uniform sampler2D _Nami;
		uniform float _Speed;
		uniform float _Tiliing;
		uniform sampler2D _Mask_1;
		uniform float4 _Mask_1_ST;
		uniform float _Wave;
		uniform sampler2D _Mask_2;
		uniform float4 _Mask_2_ST;
		uniform float _Intensity;


		//https://www.shadertoy.com/view/XdXGW8
		float2 GradientNoiseDir( float2 x )
		{
			const float2 k = float2( 0.3183099, 0.3678794 );
			x = x * k + k.yx;
			return -1.0 + 2.0 * frac( 16.0 * k * frac( x.x * x.y * ( x.x + x.y ) ) );
		}
		
		float GradientNoise( float2 UV, float Scale )
		{
			float2 p = UV * Scale;
			float2 i = floor( p );
			float2 f = frac( p );
			float2 u = f * f * ( 3.0 - 2.0 * f );
			return lerp( lerp( dot( GradientNoiseDir( i + float2( 0.0, 0.0 ) ), f - float2( 0.0, 0.0 ) ),
					dot( GradientNoiseDir( i + float2( 1.0, 0.0 ) ), f - float2( 1.0, 0.0 ) ), u.x ),
					lerp( dot( GradientNoiseDir( i + float2( 0.0, 1.0 ) ), f - float2( 0.0, 1.0 ) ),
					dot( GradientNoiseDir( i + float2( 1.0, 1.0 ) ), f - float2( 1.0, 1.0 ) ), u.x ), u.y );
		}


		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#ifdef UNITY_PASS_FORWARDBASE
			float ase_lightAtten = data.atten;
			if( _LightColor0.a == 0)
			ase_lightAtten = 0;
			#else
			float3 ase_lightAttenRGB = gi.light.color / ( ( _LightColor0.rgb ) + 0.000001 );
			float ase_lightAtten = max( max( ase_lightAttenRGB.r, ase_lightAttenRGB.g ), ase_lightAttenRGB.b );
			#endif
			#if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
			half bakedAtten = UnitySampleBakedOcclusion(data.lightmapUV.xy, data.worldPos);
			float zDist = dot(_WorldSpaceCameraPos - data.worldPos, UNITY_MATRIX_V[2].xyz);
			float fadeDist = UnityComputeShadowFadeDistance(data.worldPos, zDist);
			ase_lightAtten = UnityMixRealtimeAndBakedShadows(data.atten, bakedAtten, UnityComputeShadowFade(fadeDist));
			#endif
			float2 appendResult56 = (float2(0.0 , _Speed));
			float2 appendResult59 = (float2(1.0 , _Tiliing));
			float2 uv_TexCoord50 = i.uv_texcoord * appendResult59;
			float2 panner55 = ( _Time.y * appendResult56 + uv_TexCoord50);
			float2 Nami1_UV152 = panner55;
			float4 tex2DNode10 = tex2D( _Nami, Nami1_UV152 );
			float2 uv_Mask_1 = i.uv_texcoord * _Mask_1_ST.xy + _Mask_1_ST.zw;
			float4 tex2DNode47 = tex2D( _Mask_1, uv_Mask_1 );
			float Nami1_Mask158 = ( tex2DNode10.a * tex2DNode47.a );
			float mulTime92 = _Time.y * 0.1;
			float2 appendResult90 = (float2(_Wave , 0.0));
			float2 panner94 = ( mulTime92 * appendResult90 + i.uv_texcoord);
			float gradientNoise100 = GradientNoise(( float2( 2,2 ) * panner94 ),2.0);
			gradientNoise100 = gradientNoise100*0.5 + 0.5;
			float2 Nami2_UV154 = ( i.uv_texcoord + ( gradientNoise100 * 0.01 ) );
			float4 tex2DNode77 = tex2D( _Nami, Nami2_UV154 );
			float2 uv_Mask_2 = i.uv_texcoord * _Mask_2_ST.xy + _Mask_2_ST.zw;
			float4 tex2DNode76 = tex2D( _Mask_2, uv_Mask_2 );
			float Nami2_Mask160 = ( tex2DNode77.a * tex2DNode76.a );
			float Nami263 = ( Nami1_Mask158 + Nami2_Mask160 );
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float4 temp_cast_1 = (_Intensity).xxxx;
			float4 clampResult315 = clamp( ( ase_lightColor * ase_lightAtten ) , temp_cast_1 , float4( 1,1,1,0 ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			UnityGI gi290 = gi;
			float3 diffNorm290 = ase_worldNormal;
			gi290 = UnityGI_Base( data, 1, diffNorm290 );
			float3 indirectDiffuse290 = gi290.indirect.diffuse + diffNorm290 * 0.0001;
			c.rgb = saturate( ( clampResult315 + float4( indirectDiffuse290 , 0.0 ) ) ).rgb;
			c.a = Nami263;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
			float2 appendResult56 = (float2(0.0 , _Speed));
			float2 appendResult59 = (float2(1.0 , _Tiliing));
			float2 uv_TexCoord50 = i.uv_texcoord * appendResult59;
			float2 panner55 = ( _Time.y * appendResult56 + uv_TexCoord50);
			float2 Nami1_UV152 = panner55;
			float4 tex2DNode10 = tex2D( _Nami, Nami1_UV152 );
			float2 uv_Mask_1 = i.uv_texcoord * _Mask_1_ST.xy + _Mask_1_ST.zw;
			float4 tex2DNode47 = tex2D( _Mask_1, uv_Mask_1 );
			float Nami1_Mask158 = ( tex2DNode10.a * tex2DNode47.a );
			float mulTime92 = _Time.y * 0.1;
			float2 appendResult90 = (float2(_Wave , 0.0));
			float2 panner94 = ( mulTime92 * appendResult90 + i.uv_texcoord);
			float gradientNoise100 = GradientNoise(( float2( 2,2 ) * panner94 ),2.0);
			gradientNoise100 = gradientNoise100*0.5 + 0.5;
			float2 Nami2_UV154 = ( i.uv_texcoord + ( gradientNoise100 * 0.01 ) );
			float4 tex2DNode77 = tex2D( _Nami, Nami2_UV154 );
			float2 uv_Mask_2 = i.uv_texcoord * _Mask_2_ST.xy + _Mask_2_ST.zw;
			float4 tex2DNode76 = tex2D( _Mask_2, uv_Mask_2 );
			float Nami2_Mask160 = ( tex2DNode77.a * tex2DNode76.a );
			float Nami263 = ( Nami1_Mask158 + Nami2_Mask160 );
			float3 temp_cast_0 = (Nami263).xxx;
			o.Albedo = temp_cast_0;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
