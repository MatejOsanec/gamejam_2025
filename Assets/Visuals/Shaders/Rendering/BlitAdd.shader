Shader "Hidden/BlitAdd" {
    Properties
    {
        _MainTex ("Texture", any) = "" {}
        _Alpha("Alpha", Float) = 1.0
    }
    SubShader {
        Pass {
            ZTest Always Cull Off ZWrite Off
			Blend SrcAlpha One

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            uniform float4 _MainTex_ST;
            uniform float _Alpha;

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);                                    
                o.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 c = tex2D(_MainTex, i.texcoord);          
				c.a = _Alpha;
				return c;
            }
            ENDCG

        }
    }
    Fallback Off
}
