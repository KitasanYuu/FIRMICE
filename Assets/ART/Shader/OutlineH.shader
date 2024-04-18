Shader "Hidden/OutlineH"
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
 
			// Load the camera color buffer at the mip 0 if we're not at the before rendering injection point
			if (_CustomPassInjectionPoint != CUSTOMPASSINJECTIONPOINT_BEFORE_RENDERING)
				color = float4(CustomPassSampleCameraColor(posInput.positionNDC.xy, 0), 1);		
 
			//UV获取方式,varyings.positionCS.xy * _ScreenSize.zw为NDC坐标,_RTHandleScale.xy是对NDC坐标进行一个缩放获取uv
			//float2 uv = ClampUVs(varyings.positionCS.xy * _ScreenSize.zw * _RTHandleScale.xy);
 
			//横向模糊（需要起另一个CustomPass处理纵向模糊）
			float3 sumColor = 0;
			float weight[3] = { 0.4026, 0.2442, 0.0545 };		
 
			sumColor = color.rgb * weight[0];
			for (int i = 1; i < 3; i++)
			{			
				//注意因为采样坐标是用NDC坐标,故float2(_BlurScale * i, 0) * _ScreenSize.zw是获取一个NDC坐标系上的增量(偏移值) 进行对周围横向或纵向像素采样!
				sumColor += CustomPassSampleCameraColor(posInput.positionNDC.xy + float2(_BlurScale * i, 0) * _ScreenSize.zw, 0).rgb * weight[i];
				sumColor += CustomPassSampleCameraColor(posInput.positionNDC.xy + float2(_BlurScale * -i, 0) * _ScreenSize.zw, 0).rgb * weight[i];
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