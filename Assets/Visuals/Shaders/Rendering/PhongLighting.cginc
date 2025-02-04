#ifndef DIRECTIONALLIGHTS_INCLUDED
#define DIRECTIONALLIGHTS_INCLUDED

    static const int kMaxNumberOfDirectionalLights = 5;
    half3 _DirectionalLightDirections[kMaxNumberOfDirectionalLights];
    half3 _DirectionalLightPositions[kMaxNumberOfDirectionalLights];
    half _DirectionalLightRadii[kMaxNumberOfDirectionalLights];
    half3 _DirectionalLightColors[kMaxNumberOfDirectionalLights];

    #include "UnityStandardBRDF.cginc"
    #include "UnityStandardUtils.cginc"
    #include "UnityPBSLighting.cginc"
    #include "Assets/Visuals/Shaders/Rendering/MainEffectHelpers.cginc"

    // static const int kMaxNumberOfPointLights = 1;
    // float3 _PointLightPositions[kMaxNumberOfPointLights];
    // half3 _PointLightColors[kMaxNumberOfPointLights];

    inline float LightFalloff(half3 lightVec, half radius) {

        return 1.0 / (1.0 + 25.0 * (dot(lightVec, lightVec) / (radius * radius)));
    }

    inline half3 PhongLightingDiffuse(half3 worldPos, half3 normal) {

        half3 diffuse = half3(0.0, 0.0, 0.0);

        int i = 0;

        // Directional Light
        [unroll]
        for (i = 0; i < kMaxNumberOfDirectionalLights; i++) {

            #if LIGHT_FALLOFF
                const float falloff = LightFalloff(worldPos - _DirectionalLightPositions[i], _DirectionalLightRadii[i]);
                #if BOTH_SIDES_DIFFUSE
                    diffuse += abs(dot(normal, _DirectionalLightDirections[i])) * _DirectionalLightColors[i] * falloff;
                #else
                    diffuse += max(0.0, dot(normal, _DirectionalLightDirections[i])) * _DirectionalLightColors[i] * falloff;
                #endif
            #else
                #if BOTH_SIDES_DIFFUSE
                    diffuse += abs(dot(normal, _DirectionalLightDirections[i])) * _DirectionalLightColors[i];
                #else
                    diffuse += max(0.0, dot(normal, _DirectionalLightDirections[i])) * _DirectionalLightColors[i];
                #endif
            #endif

        }

        return diffuse;
    }

    inline void PhongLightingDiffuseSplitFrontBack(half3 worldPos, half3 normal, out half3 outDiffuseFront, out half3 outDiffuseBack) {

        outDiffuseFront = half3(0.0, 0.0, 0.0);
        outDiffuseBack = half3(0.0, 0.0, 0.0);

        int i = 0;

        // Directional Light
        [unroll]
        for (i = 0; i < kMaxNumberOfDirectionalLights; i++) {

            half3 lightColor = _DirectionalLightColors[i];
            #if LIGHT_FALLOFF
                lightColor *= LightFalloff(worldPos - _DirectionalLightPositions[i], _DirectionalLightRadii[i]);
            #endif

            half dotLightFront = dot(normal, _DirectionalLightDirections[i]);
            const half dotLightBack = max(0.0, -dotLightFront);
            dotLightFront = max(0.0, dotLightFront);
            outDiffuseFront += dotLightFront * lightColor;
            outDiffuseBack += dotLightBack * lightColor;
        }
    }

    inline half3 PhongLighting(half3 worldPos, half3 normal, half3 viewDir, fixed3 albedo, half smoothness, half metallic = 0.0,
        half specularIntensity = 1.0, half3 diffuse = half3(0.0, 0.0, 0.0), half3 specular = half3(0.0, 0.0, 0.0), half bothSidesDiffuseMultiplier = 1.0) {

        #if DIFFUSE || SPECULAR || LIGHTMAP

            #if SPECULAR
                const float smoothness4 = smoothness * smoothness * smoothness * smoothness;
                const half3 refl = reflect(viewDir, normal);
                const half b = 500 * smoothness4;
            #endif

            int i = 0;

            #if (DIFFUSE || SPECULAR) && !LIGHT_FALLOFF
                float falloff = 1.0f;
            #endif

            #if DIFFUSE || SPECULAR
                // Directional Light
                [unroll]
                for (i = 0; i < kMaxNumberOfDirectionalLights; i++) {

                    #if (DIFFUSE || SPECULAR) && LIGHT_FALLOFF
                        float falloff = LightFalloff(worldPos - _DirectionalLightPositions[i], _DirectionalLightRadii[i]);
                    #endif

                    #if DIFFUSE
                        #if BOTH_SIDES_DIFFUSE
                            half ray = dot(normal, _DirectionalLightDirections[i]);
                            ray *= ray < 0 ? -1.0 * bothSidesDiffuseMultiplier : 1.0;
                            diffuse += ray * _DirectionalLightColors[i] * falloff;
                        #else
                            diffuse += max(0.0, dot(normal, _DirectionalLightDirections[i])) * _DirectionalLightColors[i] * falloff;
                        #endif
                    #endif

                    #if SPECULAR
                        half3 rv = _DirectionalLightDirections[i] - refl;
                        half l = dot(rv, rv) * 0.5;
                        half s = max(0, (1.0 - b * l));
                        s = min(1.0, s);
                        s *= s;
                        s *= s;
                        s *= s;

                        specular += s * _DirectionalLightColors[i] * falloff * smoothness4 * 500;
                    #endif
                }
            #endif

        #endif

        #if SPECULAR
            float3 specularTint;
            float oneMinusReflectivity;

            #if DIFFUSE
                diffuse = DiffuseAndSpecularFromMetallic(diffuse * albedo, metallic, specularTint, oneMinusReflectivity);
            #else
                specularTint = lerp(unity_ColorSpaceDielectricSpec.rgb, albedo, metallic);
            #endif

            specular *= specularTint * specularIntensity;
            //specular = WhiteBoost(specular.rgb, max(specular.r ,max(specular.g, specular.b))); //Doesn't work for some use-cases
        #endif

        #if !(DIFFUSE && SPECULAR)
            diffuse *= albedo * (1 - metallic);
        #endif
        
        return diffuse + specular;
    }

    inline half3 PhongPointLightDiffuse(half3 worldPos, half3 lightPos, half3 normal, half3 color) {

        #if POINT_LIGHT_IS_LOCAL
            half3 lightDir = mul(unity_ObjectToWorld, half4(lightPos.xyz, 0.0)).xyz + half3(unity_ObjectToWorld[0][3], unity_ObjectToWorld[1][3], unity_ObjectToWorld[2][3]) - worldPos;
        #else
            half3 lightDir = lightPos - worldPos;
        #endif

        const half distanceSqr = max(dot(lightDir, lightDir), 0.00001);
        lightDir /= sqrt(distanceSqr);

        const half attenuation = 1.0 / distanceSqr;
        #if BOTH_SIDES_DIFFUSE
            return (abs(dot(normal, lightDir)) * attenuation) * color;
        #else
            return (max(0.0, dot(normal, lightDir)) * attenuation) * color;
        #endif
    }

    inline half3 PhongPointLight2Diffuse(half3 worldPos, half3 lightPos, half3 normal, half3 color) {

        #if POINT_LIGHT_2_IS_LOCAL
            half3 lightDir = mul(unity_ObjectToWorld, lightPos);
        #else
            half3 lightDir = lightPos - worldPos;
        #endif

        half distanceSqr = max(dot(lightDir, lightDir), 0.00001);
        lightDir /= sqrt(distanceSqr);

        half attenuation = 1.0 / distanceSqr;
        #if BOTH_SIDES_DIFFUSE
            return (abs(dot(normal, lightDir)) * attenuation) * color;
        #else
            return (max(0.0, dot(normal, lightDir)) * attenuation) * color;
        #endif
    }

    // inline half3 PhongPointLightDiffuse(half3 worldPos, half3 normal, half3 viewDir, fixed3 albedo, half specularHardness, half specularIntensity)
    // {
    //
    //     #if ENABLE_DIFFUSE
    //     half3 diffuse = unity_AmbientSky.rgb;
    //     #else
    //     half3 diffuse = half3(1.0, 1.0, 1.0);
    //     #endif
    //
    //     half3 specular = half3(0.0, 0.0, 0.0);
    //
    //     half3 lightDir = _PointLightPositions[i] - worldPos;
    //     half lightDirLength = length(lightDir);
    //     lightDir /= lightDirLength;
    //
    //     half attenuation = 1.0 / (1.0 + lightDirLength * 0.5);
    //
    //     diffuse += (max(0.0, dot(normal, lightDir)) * attenuation) * _PointLightColors[i];
    //     specular += (pow(max(0.0, dot(refl, lightDir)), specularHardness) * attenuation) * _PointLightColors[i];
    //
    //
    //     return diffuse * albedo + specular * specularIntensity;
    // }
#endif
