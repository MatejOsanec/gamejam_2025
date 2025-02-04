#ifndef CUSTOMFOG_INCLUDED
#define CUSTOMFOG_INCLUDED

half _CustomFogAttenuation;
half _CustomFogOffset;
half _StereoCameraEyeOffset;
half2 _CustomFogTextureToScreenRatio;
half _CustomFogHeightFogStartY;
half _CustomFogHeightFogHeight;

#if ENABLE_BLOOM_FOG
    sampler2D _BloomPrePassTexture;
#endif

// Fog Scale for Lights
inline half FogScaleMultiplierForLight(half lightIntensity) {

    return 1.0 / max(1.0, lightIntensity);
}

// Screen Positions for Fog
inline half4 OffsetNonStereoScreenPos(float4 nonStereoScreenPos) {

    #if !UNITY_UV_STARTS_AT_TOP
        nonStereoScreenPos.y = nonStereoScreenPos.w - nonStereoScreenPos.y;
    #endif

    nonStereoScreenPos.x += nonStereoScreenPos.w * lerp(-_StereoCameraEyeOffset, _StereoCameraEyeOffset, unity_StereoEyeIndex);

    return nonStereoScreenPos;
}

inline half4 ComputeOffsetNonStereoScreenPos(float4 vertex) {

    half4 nonStereoScreenPos = ComputeNonStereoScreenPos(vertex);
    return OffsetNonStereoScreenPos(nonStereoScreenPos);
}

inline half4 ComputeFogScreenPos(half4 nonStereoScreenPos) {

    nonStereoScreenPos.xy = (nonStereoScreenPos.xy - half2(0.5, 0.5) * nonStereoScreenPos.w) * _CustomFogTextureToScreenRatio + half2(0.5, 0.5) * nonStereoScreenPos.w;
    return nonStereoScreenPos;
}

// Fog Strength
inline half FogStrength(half sqrDistance) {

    #if ENABLE_BLOOM_FOG
        half d = max(sqrDistance - _CustomFogOffset, 0.0);
        return 1.0 - 1.0 / (1.0 + d * _CustomFogAttenuation);
    #else
        return 0.0;
    #endif
}

inline half FogStrength(half sqrDistance, half positionY) {

    half fogStrength = FogStrength(sqrDistance);
    half heightFogValue = saturate((positionY - (_CustomFogHeightFogStartY + _CustomFogHeightFogHeight)) / _CustomFogHeightFogHeight);
    heightFogValue = heightFogValue * heightFogValue * (3.0 - 2.0 * heightFogValue);
    fogStrength = lerp(1.0, fogStrength, heightFogValue);

    return fogStrength;
}

inline half FogStrength(half sqrDistance, half sqrStartOffset, half scale) {

    return FogStrength(max(0.0, sqrDistance - sqrStartOffset) * scale);
}

inline half FogStrength(half sqrDistance, half positionY, half sqrStartOffset, half scale) {

    return FogStrength(max(0.0, sqrDistance - sqrStartOffset) * scale, positionY);
}

// Fog Color
inline half4 FogColorUV(half2 uv) {

    #if ENABLE_BLOOM_FOG
        half4 c = tex2D(_BloomPrePassTexture, uv);
        c.a = 0.0;
        return c;
    #else
        return fixed4(0.1, 0.1, 0.1, 0.0);
    #endif
}

inline half4 FogColor(half4 fogScreenPos) {

    #if ENABLE_BLOOM_FOG
        half4 c = tex2Dproj(_BloomPrePassTexture, fogScreenPos);
        c.a = 0.0;
        return c;
    #else
        return fixed4(0.1, 0.1, 0.1, 0.0);
    #endif
}

// Lerp To Fog
inline half3 LerpToFog(half3 color, half4 fogScreenPos, half fogStrength) {

    half3 c = FogColor(fogScreenPos);
    return lerp(color, c, fogStrength);
}

inline half3 LerpToFog(half3 color, half4 fogScreenPos, half sqrCameraDistance, half sqrStartOffset, half scale) {

    half3 c = FogColor(fogScreenPos);
    half fogStrength = FogStrength(sqrCameraDistance, sqrStartOffset, scale);
    return lerp(color, c, fogStrength);
}

inline half3 LerpToFog(half3 color, half4 fogScreenPos, half sqrCameraDistance, half positionY, half sqrStartOffset, half scale) {

    half3 c = FogColor(fogScreenPos);
    half fogStrength = FogStrength(sqrCameraDistance, positionY, sqrStartOffset, scale);
    return lerp(color, c, fogStrength);
}

inline half4 LerpToFog(half4 color, half4 fogScreenPos, half fogStrength) {

    half4 c = FogColor(fogScreenPos);
    return lerp(color, c, fogStrength);
}

inline half4 LerpToFog(half4 color, half4 fogScreenPos, half sqrCameraDistance, half sqrStartOffset, half scale) {

    half4 c = FogColor(fogScreenPos);

    half fogStrength = FogStrength(sqrCameraDistance, sqrStartOffset, scale);
    return lerp(color, c, fogStrength);
}

inline half4 LerpToFog(half4 color, half4 fogScreenPos, half sqrCameraDistance, half positionY, half sqrStartOffset, half scale) {

    half4 c = FogColor(fogScreenPos);
    half fogStrength = FogStrength(sqrCameraDistance, positionY, sqrStartOffset, scale);
    return lerp(color, c, fogStrength);
}

inline half4 LerpToCustomFogColor(half4 color, half4 fogColor, half sqrCameraDistance, half sqrStartOffset = 0.0, half scale = 1.0) {

    return lerp(color, fogColor, FogStrength(sqrCameraDistance, sqrStartOffset, scale));
}

inline half4 LerpToCustomFogColor(half4 color, half4 fogColor, half sqrCameraDistance, half positionY, half sqrStartOffset = 0.0, half scale = 1.0) {

    return lerp(color, fogColor, FogStrength(sqrCameraDistance, positionY, sqrStartOffset, scale));
}

// Fog for Alpha
inline half FogForAlpha(half lightIntensity, half sqrCameraDistance, half sqrStartOffset, half scale) {

    half fogStrength = FogStrength(sqrCameraDistance, sqrStartOffset, scale);
    return lightIntensity * (1.0 - fogStrength);
}

inline half FogForAlpha(half lightIntensity, half sqrCameraDistance, half positionY, half sqrStartOffset, half scale) {

    half fogStrength = FogStrength(sqrCameraDistance, positionY, sqrStartOffset, scale);
    return lightIntensity * (1.0 - fogStrength);
}

// Fog for Light Intensity
inline half GetFogForLightIntensityMultiplier(half lightIntensity, half sqrCameraDistance, half sqrStartOffset, half scale) {

    half fogStrength = FogStrength(sqrCameraDistance, sqrStartOffset, scale * FogScaleMultiplierForLight(lightIntensity));
    return 1.0 - fogStrength;
}

inline half GetFogForLightIntensityMultiplier(half lightIntensity, half sqrCameraDistance, half positionY, half sqrStartOffset, half scale) {

    half fogStrength = FogStrength(sqrCameraDistance, positionY, sqrStartOffset, scale * FogScaleMultiplierForLight(lightIntensity));
    return 1.0 - fogStrength;
}

inline half FogForLightIntensity(half lightIntensity, half sqrCameraDistance, half sqrStartOffset, half scale) {

    return lightIntensity * GetFogForLightIntensityMultiplier(lightIntensity, sqrCameraDistance, sqrStartOffset, scale);
}

inline half FogForLightIntensity(half lightIntensity, half sqrCameraDistance, half positionY, half sqrStartOffset, half scale) {

    return lightIntensity * GetFogForLightIntensityMultiplier(lightIntensity, sqrCameraDistance, positionY, sqrStartOffset, scale);
}

// Lerp Light to Fog
inline half4 LerpLightToFog(half4 color, half4 fogScreenPos, half sqrCameraDistance, half positionY, half sqrStartOffset, half scale) {

    return LerpToFog(color, fogScreenPos, sqrCameraDistance, positionY, sqrStartOffset, scale * FogScaleMultiplierForLight(color.a));
}

inline half4 LerpLightToFog(half4 color, half4 fogScreenPos, half sqrCameraDistance, half sqrStartOffset, half scale) {

    return LerpToFog(color, fogScreenPos, sqrCameraDistance, sqrStartOffset, scale * FogScaleMultiplierForLight(color.a));
}

#endif
