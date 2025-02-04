Shader "Hidden/SkyGradient" {

    Properties {
        _GradientTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
    }

    SubShader {
        Pass {
            ZTest Always
            Cull Off
            ZWrite Off
            Blend One One

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ USE_TONE_MAPPING
            #pragma multi_compile _ ACES_TONE_MAPPING

            #include "UnityCG.cginc"

            #if ACES_TONE_MAPPING && USE_TONE_MAPPING
                #include "Tonemapping.cginc"
            #endif

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float3 eyeRay : TEXCOORD0;
            };

            float4x4 _InverseProjectionMatrix;
            float4x4 _CameraToWorldMatrix;
            half4 _Color;

            sampler2D _GradientTex;

            v2f vert(appdata v) {

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                float4 clipSpacePosition = float4(v.vertex.xy * 2.0 - 1.0, 1.0, 1.0);
                float4 viewSpacePosition = mul(_InverseProjectionMatrix, clipSpacePosition);

                o.eyeRay = mul((float3x3)_CameraToWorldMatrix, viewSpacePosition);

                return o;
            }

            float4 frag(v2f i) : SV_Target {

                float y = normalize(i.eyeRay).y;
                y = y * 0.5 + 0.5;
                fixed4 c = tex2D(_GradientTex, half2(y, 0.0)) * _Color;

                #if ACES_TONE_MAPPING && USE_TONE_MAPPING
                    c.rgb = ACESFilmTonemapping(c.rgb);
                #endif

                return c;
            }
            ENDCG
        }
    }
}
