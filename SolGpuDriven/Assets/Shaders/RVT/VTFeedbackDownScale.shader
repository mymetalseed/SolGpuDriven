Shader "VirtualTexture/VT/FeedbackDownScale"
{
    Properties
    {
        _MainTex("Texture",2D) = "white" {}
    }
    
    SubShader
    {
        Cull Off
        Zwrite Off
        ZTest Always
        Tags {"RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline"}
        
        Pass
        {
            Tags {"LightMode" = "UniversalForward"}
            
            HLSLPROGRAM
            #include "VTFeedback.cginc"
            #pragma vertex VTVert
            #pragma fragment frag

            float4 frag(VTV2f i) : SV_Target
            {
                return GetMaxFeedback(i.uv,2);
            }
            
            ENDHLSL
        }
        
    	Pass
		{
			Tags { "LightMode" = "UniversalForward" }

			HLSLPROGRAM
			#include "VTFeedback.cginc"	
			#pragma vertex VTVert
			#pragma fragment frag
			
			float4 frag(VTV2f i) : SV_Target
			{
				return GetMaxFeedback(i.uv, 4);
			}
			
			ENDHLSL
		}

		Pass
		{
			Tags { "LightMode" = "UniversalForward" }

			HLSLPROGRAM
			#include "VTFeedback.cginc"	
			#pragma vertex VTVert
			#pragma fragment frag
			
			float4 frag(VTV2f i) : SV_Target
			{
				return GetMaxFeedback(i.uv, 8);
			}
			
			ENDHLSL
		}
    }
}