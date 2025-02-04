#ifndef TONEMAPPING_INCLUDED
#define TONEMAPPING_INCLUDED

float3 ReinhardTonemapping(const float3 x) {

    return x * (1 + x * 0.25) / (1.0 + x);
}

float3 ACESFilmTonemapping(float3 x) {

    // Note this remaps 1.0 to ~0.8 which can easily cause issues for whiteboosted emissive surfaces (so consider moving ACES before emissive)
    // Uses simplified curve mentioned in the following article https://knarkowicz.wordpress.com/2016/01/06/aces-filmic-tone-mapping-curve/

    const float a = 2.51f;
    const float b = 0.03f;
    const float c = 2.43f;
    const float d = 0.59f;
    const float e = 0.14f;
    return saturate((x*(a*x+b))/(x*(c*x+d)+e));
}

#endif
