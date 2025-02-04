
Shader "Hidden/PostProcessing/Bloom" {

    Properties {
        [HideInInspector] _MainTex ("Main Texture", 2D) = "white" {}
    }

    CGINCLUDE

        #include "BloomSampling.cginc"
        #include "Assets/Visuals/Shaders/Rendering/Tonemapping.cginc"
        
        #if IS_SCREENSPACE_EFFECT
            UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
            UNITY_DECLARE_SCREENSPACE_TEXTURE(_BloomTex);
            UNITY_DECLARE_SCREENSPACE_TEXTURE(_GlobalIntensityTex);
        #else
            sampler2D _MainTex;
            sampler2D _BloomTex;
            sampler2D _GlobalIntensityTex;
        #endif
    
        float _AutoExposureLimit;
        float4 _MainTex_TexelSize;
        half4 _MainTex_ST;
        float _SampleScale;
        float _CombineSrc;
        float _CombineDst;
        float _AlphaWeights;

        // Vert

        struct appdata {

            float4 vertex : POSITION;
            float2 texcoord : TEXCOORD;
            #if IS_SCREENSPACE_EFFECT
                UNITY_VERTEX_INPUT_INSTANCE_ID
            #endif
        };

        struct v2f {
            
            float4 pos : SV_POSITION;
            float2 texcoord : TEXCOORD0;
            #if IS_SCREENSPACE_EFFECT
                UNITY_VERTEX_OUTPUT_STEREO
            #endif
        };

        v2f VertDefault(appdata v) {

            v2f o;

            #if IS_SCREENSPACE_EFFECT
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            #endif
            
            o.pos = UnityObjectToClipPos(v.vertex);
            o.texcoord = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);
            return o;
        }

        // ----------------------------------------------------------------------------------------
        // Prefilter

        inline half4 Prefilter(half4 color) {

            color.rgb *= saturate(color.a * _AlphaWeights);
            return color;
        }

        half4 FragPrefilter13(v2f i) : SV_Target {

            #if IS_SCREENSPACE_EFFECT
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
            #endif
            
            half4 color = DownsampleBox13Tap(PASS_TEXTURE_SAMPLER(_MainTex), i.texcoord, _MainTex_TexelSize.xy);
            return Prefilter(color);
        }

        half4 FragPrefilter4(v2f i) : SV_Target {

            #if IS_SCREENSPACE_EFFECT
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
            #endif
            
            half4 color = DownsampleBox4Tap(PASS_TEXTURE_SAMPLER(_MainTex), i.texcoord, _MainTex_TexelSize.xy);
            return Prefilter(color);
        }

        // ----------------------------------------------------------------------------------------
        // Downsample

        half4 FragDownsample13(v2f i) : SV_Target {

            #if IS_SCREENSPACE_EFFECT
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
            #endif
            
            half4 color = DownsampleBox13Tap(PASS_TEXTURE_SAMPLER(_MainTex), i.texcoord, _MainTex_TexelSize.xy);
            return color;
        }

        half4 FragDownsample4(v2f i) : SV_Target {

            #if IS_SCREENSPACE_EFFECT
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
            #endif
            
            half4 color = DownsampleBox4Tap(PASS_TEXTURE_SAMPLER(_MainTex), i.texcoord, _MainTex_TexelSize.xy);
            return color;
        }

        half4 FragDownsampleBilinearGamma(v2f i) : SV_Target {

            #if IS_SCREENSPACE_EFFECT
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
            #endif
            
            half4 color = DownsampleBox4Tap(PASS_TEXTURE_SAMPLER(_MainTex), i.texcoord, _MainTex_TexelSize.xy);
            color.rgb = GammaToLinearSpace(color.rgb);
            return color;
        }

        // ----------------------------------------------------------------------------------------
        // Upsample & combine

        half4 Combine(half4 bloom, float2 uv) {

            half4 color = SAMPLE_TEXTURE(_BloomTex, uv);
            return bloom * _CombineDst + color * _CombineSrc;
        }

        half4 FragUpsampleTent(v2f i) : SV_Target {

            #if IS_SCREENSPACE_EFFECT
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
            #endif
            
            half4 bloom = UpsampleTent(PASS_TEXTURE_SAMPLER(_MainTex), i.texcoord, _MainTex_TexelSize.xy, _SampleScale);
            return Combine(bloom, i.texcoord);
        }

        half4 FragUpsampleBox(v2f i) : SV_Target {

            #if IS_SCREENSPACE_EFFECT
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
            #endif
            
            half4 bloom = UpsampleBox(PASS_TEXTURE_SAMPLER(_MainTex), i.texcoord, _MainTex_TexelSize.xy, _SampleScale);
            return Combine(bloom, i.texcoord);
        }

        half4 FragUpsampleBilinear(v2f i) : SV_Target {

            #if IS_SCREENSPACE_EFFECT
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
            #endif
            
            half4 bloom = SAMPLE_TEXTURE(_MainTex, i.texcoord);
            return Combine(bloom, i.texcoord);
        }

        half4 FragUpsampleTentGamma(v2f i) : SV_Target {

            #if IS_SCREENSPACE_EFFECT
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
            #endif
            
            half4 bloom = UpsampleTent(PASS_TEXTURE_SAMPLER(_MainTex), i.texcoord, _MainTex_TexelSize.xy, _SampleScale);
            half4 col = Combine(bloom, i.texcoord);
            col.rgb = GammaToLinearSpace(col.rgb);
            return col;
        }

        half4 FragUpsampleBoxGamma(v2f i) : SV_Target {

            #if IS_SCREENSPACE_EFFECT
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
            #endif
            
            half4 bloom = UpsampleBox(PASS_TEXTURE_SAMPLER(_MainTex), i.texcoord, _MainTex_TexelSize.xy, _SampleScale);
            half4 col = Combine(bloom, i.texcoord);
            col.rgb = GammaToLinearSpace(col.rgb);
            return col;
        }

        half4 FragUpsampleBilinearGamma(v2f i) : SV_Target {

            #if IS_SCREENSPACE_EFFECT
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
            #endif
            
            half4 bloom = SAMPLE_TEXTURE(_MainTex, i.texcoord);
            half4 col = Combine(bloom, i.texcoord);
            col.rgb = GammaToLinearSpace(col.rgb);
            return col;
        }

        half4 FragUpsampleTentAndReinhard(v2f i) : SV_Target {

            #if IS_SCREENSPACE_EFFECT
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
            #endif
            
            half4 bloom = UpsampleTent(PASS_TEXTURE_SAMPLER(_MainTex), i.texcoord, _MainTex_TexelSize.xy, _SampleScale);
            half4 col = Combine(bloom, i.texcoord);
            col.rgb = ReinhardTonemapping(col.rgb);

            return col;
        }

        half4 FragUpsampleTentAndACES(v2f i) : SV_Target {

            #if IS_SCREENSPACE_EFFECT
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
            #endif
            
            half4 bloom = UpsampleTent(PASS_TEXTURE_SAMPLER(_MainTex), i.texcoord, _MainTex_TexelSize.xy, _SampleScale);
            half4 col = Combine(bloom, i.texcoord);
            col.rgb = ACESFilmTonemapping(col.rgb);

            return col;
        }

        half4 FragUpsampleTentAndACESAndGlobalIntensity(v2f i) : SV_Target {

            #if IS_SCREENSPACE_EFFECT
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
            #endif
            
            const half4 bloom = UpsampleTent(PASS_TEXTURE_SAMPLER(_MainTex), i.texcoord, _MainTex_TexelSize.xy, _SampleScale);
            const float intensity = dot(SAMPLE_TEXTURE(_GlobalIntensityTex, half2(0.5, 0.5)).rgb, float3(0.3, 0.59, 0.11));
            
            // Lower _AutoExposureLimit means more supression at lower intensities, values between 300 and 10 000 work best
            // Originally only the 0.1 / sqrt(intensity) calculation was used, which resulted in infinitely multiplied Bloom appearing sooner than its source
            // Later, a now-legacy solution solved it while introducing an issue of inverse slope, resulting in bloom A becoming stronger when bloom B is turned on in low light situations
            // Current solution has linear slope, hopefully bringing an ideal middle ground into the auto exposure behavior
            // Comparison graph for autoexposureCurve » https://www.desmos.com/calculator/bd5ywtxa7e where (c) is the original, (b) is legacy and (d) is current
            // Comparison graph for resulting intensity » https://www.desmos.com/calculator/0v11m4kede where (o) is the original, (b) is legacy and (d) is current
            
            float autoexposureCurve = 0.1 / sqrt(intensity);
            #if LEGACY_AUTOEXPOSURE
                autoexposureCurve = min(autoexposureCurve, intensity * _AutoExposureLimit);
            #else
                autoexposureCurve = min(autoexposureCurve, 0.004 * _AutoExposureLimit);
            #endif
            
            half4 col = Combine(bloom, i.texcoord) * autoexposureCurve;

            col.rgb = ACESFilmTonemapping(col.rgb);

            return col;
        }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        // 0: Prefilter 13 taps
        Pass {

            CGPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragPrefilter13
                #pragma multi_compile _ IS_SCREENSPACE_EFFECT
            ENDCG
        }

        // 1: Prefilter 4 taps
        Pass {

            CGPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragPrefilter4
                #pragma multi_compile _ IS_SCREENSPACE_EFFECT
            ENDCG
        }

        // 2: Downsample 13 taps
        Pass {

            CGPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragDownsample13
                #pragma multi_compile _ IS_SCREENSPACE_EFFECT
            ENDCG
        }

        // 3: Downsample 4 taps
        Pass {

            CGPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragDownsample4
                #pragma multi_compile _ IS_SCREENSPACE_EFFECT
            ENDCG
        }

        // 4 Downsample Bilinear Gamma
        Pass {

            CGPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragDownsampleBilinearGamma
                #pragma multi_compile _ IS_SCREENSPACE_EFFECT
            ENDCG
        }

        // 5: Upsample tent filter
        Pass {

            CGPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragUpsampleTent
                #pragma multi_compile _ IS_SCREENSPACE_EFFECT
            ENDCG
        }

        // 6: Upsample box filter
        Pass {

            CGPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragUpsampleBox
                #pragma multi_compile _ IS_SCREENSPACE_EFFECT
            ENDCG
        }

        // 7: Upsample tent filter gamma
        Pass {

            CGPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragUpsampleTentGamma
                #pragma multi_compile _ IS_SCREENSPACE_EFFECT
            ENDCG
        }

        // 8: Upsample box filter gamma
        Pass {

            CGPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragUpsampleBoxGamma
                #pragma multi_compile _ IS_SCREENSPACE_EFFECT
            ENDCG
        }

        // 9: Upsample bilinear filter
        Pass {

            CGPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragUpsampleBilinear
                #pragma multi_compile _ IS_SCREENSPACE_EFFECT
            ENDCG
        }

        // 10: Upsample bilinear filter gamma
        Pass {

            CGPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragUpsampleBilinearGamma
                #pragma multi_compile _ IS_SCREENSPACE_EFFECT
            ENDCG
        }

        // 11: Upsample tent filter and Reinhard tonemapping
        Pass {

            CGPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragUpsampleTentAndReinhard
                #pragma multi_compile _ IS_SCREENSPACE_EFFECT
            ENDCG
        }

        // 12: Upsample tent filter and ACES tonemapping
        Pass {

            CGPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragUpsampleTentAndACES
                #pragma multi_compile _ IS_SCREENSPACE_EFFECT            
            ENDCG
        }

        // 13: Upsample tent filter and ACES tonemapping with global intensity
        Pass {

            CGPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragUpsampleTentAndACESAndGlobalIntensity
                #pragma multi_compile _ IS_SCREENSPACE_EFFECT
                #pragma multi_compile _ LEGACY_AUTOEXPOSURE
            ENDCG
        }
    }
}
