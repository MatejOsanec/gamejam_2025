Shader "Custom/BloomSkyboxQuad" {

    Properties {
    }

    CGINCLUDE

    #include "UnityCG.cginc"
    #include "CustomFog.cginc"
    #include "BlueNoise.cginc"
    #include "OverdrawView.cginc"

    struct appdata {

        float4 vertex : POSITION;
        float3 texcoord : TEXCOORD0;

        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct v2f {

        float4 pos : SV_POSITION;
        half4 fogScreenPos : TEXCOORD0;
        half4 noiseScreenPos : TEXCOORD1;

        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
    };

    v2f vert(appdata v) {

        v2f o;

        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_OUTPUT(v2f, o);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

        // #if SHADER_API_MOBILE
        //     o.pos = /*UnityObjectToClipPos(v.vertex) * 0.00001 + */float4(v.vertex.xy, 1.0, 1.0);
        // #else
            #if UNITY_REVERSED_Z
                o.pos = float4(v.vertex.xy, UNITY_NEAR_CLIP_VALUE - 1.0, 1.0);
            #else
                o.pos = float4(v.vertex.xy, 1.0, 1.0);
            #endif
        // #endif

        const half4 nonStereoScreenPos = ComputeOffsetNonStereoScreenPos(o.pos);
        o.fogScreenPos = ComputeFogScreenPos(nonStereoScreenPos);
        o.noiseScreenPos = ComputeNoiseScreenPos(nonStereoScreenPos);

        return o;
    }

    fixed4 frag(v2f i) : COLOR {

        #if OVERDRAW_VIEW
            return OverdrawViewOpaque();
        #endif

        float noise = BlueNoise(i.noiseScreenPos);

        fixed4 c;
        c.rgb = FogColor(i.fogScreenPos) + noise.rrr;
        c.a = 0.0;
        return c;
    }

    ENDCG

    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass {

            ZWrite Off
            Cull Off
            ZTest LEqual

            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ OVERDRAW_VIEW
            #pragma multi_compile _ ENABLE_BLOOM_FOG
            ENDCG
        }
    }
}
