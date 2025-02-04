Shader "Hidden/BlitMaxColor" {
    Properties
    {        
        _Color ("Color", Color) = ( 1, 1, 1, 1 )
    }
    SubShader {
        Pass {
            ZTest Always Cull Off ZWrite Off
			Blend One One
            BlendOp Max
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            half4 _Color;

            struct appdata_t {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);                                    
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {          
                _Color.rgb *= _Color.a;
				return _Color;
            }
            ENDCG

        }
    }
    Fallback Off
}
