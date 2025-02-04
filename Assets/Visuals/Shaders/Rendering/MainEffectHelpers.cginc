#ifndef MAIN_EFFECT_HELPERS_INCLUDED
#define MAIN_EFFECT_HELPERS_INCLUDED

half _BaseColorBoost;
half _BaseColorBoostThreshold;

inline half3 WhiteBoost(half3 color, half alpha) {

    half b = alpha * alpha * _BaseColorBoost - _BaseColorBoostThreshold;
    return saturate(color + half3(b, b, b));
}

inline half3 ForwardWhiteBoost(half3 color, half alpha) {

    #if MAIN_EFFECT_ENABLED // Bloom Postprocess
        return color;
    #else
        color.rgb = WhiteBoost(color, alpha);
        return color;
    #endif
}

#endif
