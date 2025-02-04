Shader "Hidden/PseudoHDREncoding" {

    Properties {
        [HideInInspector] _MainTex ("Main Texture", 2D) = "white" {}
    }

    SubShader {

        Pass {

            Cull Off
            ZWrite Off
            ZTest Always
            Blend Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            half4 _MainTex_ST;

            struct v2f {

                half4 pos : SV_POSITION;
                half2 uv0 : TEXCOORD0;
            };

            v2f vert(appdata_img v) {

                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv0 = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);
                return o;
            }

            float4 frag(v2f i) : SV_Target {
                const float4 col = tex2D(_MainTex, i.uv0);
                const float intensity = saturate(col.r * 0.5 + pow(col.a, 0.4545) * 0.5);
                return LinearToGammaSpaceExact(intensity);
            }
            ENDCG
        }
    }
}