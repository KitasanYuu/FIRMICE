Shader "Hidden/OutlineV"
{
	Properties{
		_BlurScale("BlurScale", float) = 1
	}
	HLSLINCLUDE
 
		#pragma vertex Vert
 
		#pragma target 4.5
		#pragma only_renderers d3d11 playstation xboxone vulkan metal switch
 
		#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"
	
		float _BlurScale;
 
		float2 ClampUVs(float2 uv)
		{
			uv = clamp(uv, 0, _RTHandleScale.xy - _ScreenSize.zw * 2); // clamp UV to 1 pixel to avoid bleeding
			return uv;
		}
 
		float2 GetSampleUVs(Varyings varyings)
		{
			float depth = LoadCameraDepth(varyings.positionCS.xy);
			PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
			return posInput.positionNDC.xy * _RTHandleScale.xy;
		}
 
		float4 FullScreenPass(Varyings varyings) : SV_Target
		{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);
 
			float depth = LoadCameraDepth(varyings.positionCS.xy);
			PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
			float4 color = float4(0.0, 0.0, 0.0, 0.0);			
 
 
			//纵向从CustomPass(上一个横向模糊处理后的图片采样，即CustomColor缓冲区） 使用的是uv  
// float4 SampleCustomColor(float2 uv);
// float4 LoadCustomColor(uint2 pixelCoords);
 
			//UV获取方式,varyings.positionCS.xy * _ScreenSize.zw为NDC坐标,_RTHandleScale.xy是对NDC坐标进行一个缩放获取uv
			float2 uv = ClampUVs(varyings.positionCS.xy * _ScreenSize.zw * _RTHandleScale.xy);
 
			// Load the camera color buffer at the mip 0 if we're not at the before rendering injection point
			if (_CustomPassInjectionPoint != CUSTOMPASSINJECTIONPOINT_BEFORE_RENDERING)
				color = SampleCustomColor(uv);
						 
						
 
			//纵向向模糊
			float3 sumColor = 0;
			float weight[3] = { 0.4026, 0.2442, 0.0545 };		
 
			sumColor = color.rgb * weight[0];
			for (int i = 1; i < 3; i++)
			{			
				//注意采用使用的是UV坐标，因此float2(0, _BlurScale * i) * _ScreenSize.zw * _RTHandleScale.xy获取UV坐标增量（偏移值）
				sumColor += SampleCustomColor(uv + float2(0, _BlurScale * i) * _ScreenSize.zw * _RTHandleScale.xy).rgb * weight[i];				
				sumColor += SampleCustomColor(uv + float2(0, _BlurScale * -i) * _ScreenSize.zw * _RTHandleScale.xy).rgb * weight[i];
			}
		
			return float4(sumColor, 1);
		}
 
	ENDHLSL
 
	SubShader
	{
		Pass
		{
			Name "Custom Pass 0"
 
			ZWrite On
			ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha 			
			Cull Off
 
			HLSLPROGRAM
				#pragma fragment FullScreenPass
			ENDHLSL
		}
	}
	Fallback Off
}