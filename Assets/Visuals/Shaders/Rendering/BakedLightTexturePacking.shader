Shader "Hidden/BakedLightTexturePacking" {

    Properties {
        [HideInInspector] _Tex1 ("Tex1", 2D) = "black" {}
        [HideInInspector] _Tex2 ("Tex2", 2D) = "black" {}
        [HideInInspector] _Tex3 ("Tex3", 2D) = "black" {}
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

            sampler2D _Tex1;
            half4 _Tex1_ST;

            sampler2D _Tex2;
            half4 _Tex2_ST;

            sampler2D _Tex3;
            half4 _Tex3_ST;

            struct v2f {

                half4 pos : SV_POSITION;
                half2 uv0 : TEXCOORD0;
            };

            v2f vert(appdata_img v) {

                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv0 = v.texcoord;
                return o;
            }

            float4 frag(v2f i) : SV_Target {

                float4 col1 = tex2D(_Tex1, i.uv0);
                float4 col2 = tex2D(_Tex2, i.uv0);
                float4 col3 = tex2D(_Tex3, i.uv0);

                return float4(col1.r, col2.r, col3.r, 0.0);
            }
            ENDCG
        }
    }
}