Shader "Hidden/BlitBlendColor" {
    Properties
    {        
        _Color ("Color", Color) = ( 1, 1, 1, 1 )
    }
    SubShader {
        Pass {
            ZTest Always Cull Off ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            half4 _Color;

            struct appdata_t {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID //BS CHANGE for single pass instanced rendering
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO //BS CHANGE for single pass instanced rendering
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                
                UNITY_SETUP_INSTANCE_ID(v); //BS CHANGE for single pass instanced rendering
                UNITY_INITIALIZE_OUTPUT(v2f, o); //BS CHANGE for single pass instanced rendering
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //BS CHANGE for single pass instanced rendering
                
                o.vertex = UnityObjectToClipPos(v.vertex);                                    
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {                
				return _Color;
            }
            ENDCG

        }
    }
    Fallback Off
}
