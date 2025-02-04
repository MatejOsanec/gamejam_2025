Shader "Hidden/HM/KawaseBlur" {

	Properties {
        _MainTex ("Main Texture", 2D) = "" {}
	}

	SubShader {

		// Pass 0
        Pass {
        
            name "AlphaWeights"
            Cull Off
            ZWrite Off
            ZTest Always
        
            CGPROGRAM            
                #include "KawaseBlur.cginc"
                #pragma vertex vertSimple
                #pragma fragment fragAlphaWeights
            ENDCG   
        }	               

       	// Pass 1
       	Pass {
        
            name "KawaseBlur"
            Cull Off
            ZWrite Off
            ZTest Always
        
            CGPROGRAM  
                #include "KawaseBlur.cginc"          
            	#pragma vertex vertOffsets
                #pragma fragment fragKawaseBlur
            ENDCG   
        }	

       	// Pass 2
       	Pass {
        
            name "KawaseBlurAdd"
            Cull Off
            ZWrite Off
            ZTest Always
			Blend SrcAlpha One
        
            CGPROGRAM  
                #include "KawaseBlur.cginc"          
            	#pragma vertex vertOffsets
                #pragma fragment fragKawaseBlur
            ENDCG   
        }
        
        // Pass 3
       	Pass {
        
            name "KawaseBlurWithAlphaWeights"
            Cull Off
            ZWrite Off
            ZTest Always
        
            CGPROGRAM  
                #include "KawaseBlur.cginc"          
            	#pragma vertex vertOffsets
                #pragma fragment fragKawaseBlurWithAlphaWeights
            ENDCG
        }

        // Pass 4
       	Pass {
        
            name "AlphaAndDepthWeights"
            Cull Off
            ZWrite Off
            ZTest Always                        
        
            CGPROGRAM  
                #include "KawaseBlur.cginc"
            	#pragma vertex vertSimple
                #pragma fragment fragAlphaAndDepthWeights
            ENDCG   
        }

       	// Pass 5
       	Pass {
        
            name "KawaseBlurGamma"
            Cull Off
            ZWrite Off
            ZTest Always
        
            CGPROGRAM  
                #include "KawaseBlur.cginc"          
            	#pragma vertex vertOffsets
                #pragma fragment fragKawaseBlurGamma
            ENDCG   
        }	  

       	// Pass 6
       	Pass {
        
            name "KawaseBlurAddGamma"
            Cull Off
            ZWrite Off
            ZTest Always
			Blend SrcAlpha One
        
            CGPROGRAM  
                #include "KawaseBlur.cginc"          
            	#pragma vertex vertOffsets
                #pragma fragment fragKawaseBlurGamma
            ENDCG   
        }              
	}

    Fallback Off
}