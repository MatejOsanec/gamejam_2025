#ifndef CUBEMAPPING_INCLUDED
#define CUBEMAPPING_INCLUDED

    #include "UnityGlobalIllumination.cginc"

    half3 GlossyEnvironment(samplerCUBE tex, half roughness, half3 refl) {

        half perceptualRoughness = roughness;
        perceptualRoughness = perceptualRoughness * (1.7 - 0.7 * perceptualRoughness);

        half mip = perceptualRoughnessToMipmapLevel(perceptualRoughness);

        return texCUBElod(tex, half4(refl, mip)).rgb;
    }

    half3 GlossyEnvironmentMip(samplerCUBE tex, half mip, half3 refl) {

        return texCUBElod(tex, half4(refl, mip)).rgb;
    }

    half GetMipmapLevel(half smoothness) {
        half perceptualRoughness = 1 - smoothness;
        perceptualRoughness = perceptualRoughness * (1.7 - 0.7 * perceptualRoughness);
        return perceptualRoughnessToMipmapLevel(perceptualRoughness);
    }

#endif