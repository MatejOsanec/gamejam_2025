#ifndef LIGHTMAPPING_INCLUDED
#define LIGHTMAPPING_INCLUDED

    half3 _LightmapLightBakeIdA;
    half3 _LightmapLightBakeIdB;
    half3 _LightmapLightBakeIdC;
    half3 _LightmapLightBakeIdD;
    half3 _LightmapLightBakeIdE;
    half3 _LightmapLightBakeIdF;

    half4 _LightProbeLightBakeIdA;
    half4 _LightProbeLightBakeIdB;
    half4 _LightProbeLightBakeIdC;
    half4 _LightProbeLightBakeIdD;
    half4 _LightProbeLightBakeIdE;
    half4 _LightProbeLightBakeIdF;

    #if REFLECTION_PROBE
        UNITY_DECLARE_TEXCUBE(_ReflectionProbeTexture1);
        UNITY_DECLARE_TEXCUBE(_ReflectionProbeTexture2);
        float3 _ReflectionProbePosition;
        float3 _ReflectionProbeBoundsMin;
        float3 _ReflectionProbeBoundsMax;
        #if REFLECTION_PROBE_BOX_PROJECTION_OFFSET
            float3 _ReflectionProbeBoxProjectionSizeOffset;
            float3 _ReflectionProbeBoxProjectionPositionOffset;
        #endif
    #endif

    #if LIGHTMAP
        sampler2D _LightMap1;
        sampler2D _LightMap2;
    #endif

    #if LIGHTMAP
        half3 GetLightmap(float2 lightmapUv) {

            half3 lightmap1 = tex2D(_LightMap1, lightmapUv);
            half3 lightmap2 = tex2D(_LightMap2, lightmapUv);

            half3 light1 = lightmap1.r * _LightmapLightBakeIdA.rgb;
            half3 light2 = lightmap1.g * _LightmapLightBakeIdB.rgb;
            half3 light3 = lightmap1.b * _LightmapLightBakeIdC.rgb;
            half3 light4 = lightmap2.r * _LightmapLightBakeIdD.rgb;
            half3 light5 = lightmap2.g * _LightmapLightBakeIdE.rgb;
            half3 light6 = lightmap2.b * _LightmapLightBakeIdF.rgb;

            return (light1 + light2 + light3 + light4 + light5 + light6) * 4.59479342;
        }
    #endif

    #if REFLECTION_PROBE

        float3 BoxProjection (float3 direction, float3 position, float3 cubemapPosition, float3 boxMin, float3 boxMax) {

            float3 factors = ((direction > 0 ? boxMax : boxMin) - position) / direction;
            float scalar = min(min(factors.x, factors.y), factors.z);
            return direction * scalar + (position - cubemapPosition);
        }

        half3 GetReflectionProbeMixedColor(float3 reflUVW, half mip) {

            half3 probe1Color = UNITY_SAMPLE_TEXCUBE_LOD(_ReflectionProbeTexture1, reflUVW, mip);
            half3 probe2Color = UNITY_SAMPLE_TEXCUBE_LOD(_ReflectionProbeTexture2, reflUVW, mip);

            half3 probe1ColorC = min(probe1Color, 0.5);
            half3 probe2ColorC = min(probe2Color, 0.5);

            #if !REFLECTION_PROBE_DISABLED_WHITEBOOST
                half3 probe1ColorA = max(0.0, probe1Color - 0.5);
                half3 probe2ColorA = max(0.0, probe2Color - 0.5);
            
                probe1ColorA *= half3(_LightProbeLightBakeIdA.a, _LightProbeLightBakeIdB.a, _LightProbeLightBakeIdC.a);
                probe1ColorA *= probe1ColorA;
                probe2ColorA *= half3(_LightProbeLightBakeIdD.a, _LightProbeLightBakeIdE.a, _LightProbeLightBakeIdF.a);
                probe2ColorA *= probe2ColorA;

                half3 probe1 = probe1ColorC.r * _LightProbeLightBakeIdA.rgb + probe1ColorA.r;
                half3 probe2 = probe1ColorC.g * _LightProbeLightBakeIdB.rgb + probe1ColorA.g;
                half3 probe3 = probe1ColorC.b * _LightProbeLightBakeIdC.rgb + probe1ColorA.b;
                half3 probe4 = probe2ColorC.r * _LightProbeLightBakeIdD.rgb + probe2ColorA.r;
                half3 probe5 = probe2ColorC.g * _LightProbeLightBakeIdE.rgb + probe2ColorA.g;
                half3 probe6 = probe2ColorC.b * _LightProbeLightBakeIdF.rgb + probe2ColorA.b;
            #else
                half3 probe1 = probe1ColorC.r * _LightProbeLightBakeIdA.rgb;
                half3 probe2 = probe1ColorC.g * _LightProbeLightBakeIdB.rgb;
                half3 probe3 = probe1ColorC.b * _LightProbeLightBakeIdC.rgb;
                half3 probe4 = probe2ColorC.r * _LightProbeLightBakeIdD.rgb;
                half3 probe5 = probe2ColorC.g * _LightProbeLightBakeIdE.rgb;
                half3 probe6 = probe2ColorC.b * _LightProbeLightBakeIdF.rgb;
            #endif

            return saturate((probe1 + probe2 + probe3 + probe4 + probe5 + probe6) * 2.0);
        }

        half3 GetReflectionProbe(float3 worldPos, float3 reflectedDir, half mip) {

            #if REFLECTION_PROBE_BOX_PROJECTION
                #if REFLECTION_PROBE_BOX_PROJECTION_OFFSET
                    float3 reflUVW = BoxProjection(
                        reflectedDir,
                        worldPos,
                        _ReflectionProbePosition + _ReflectionProbeBoxProjectionPositionOffset,
                        _ReflectionProbeBoundsMin - _ReflectionProbeBoxProjectionSizeOffset,
                        _ReflectionProbeBoundsMax + _ReflectionProbeBoxProjectionSizeOffset
                    );
                #else
                    float3 reflUVW = BoxProjection(
                        reflectedDir,
                        worldPos,
                        _ReflectionProbePosition,
                        _ReflectionProbeBoundsMin,
                        _ReflectionProbeBoundsMax
                    );
                #endif
            #else
                float3 reflUVW = reflectedDir;
            #endif

            return GetReflectionProbeMixedColor(reflUVW, mip);
        }
    #endif
#endif
