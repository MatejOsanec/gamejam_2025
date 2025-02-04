#ifndef HOT_COLOR_GRADIENT_INCLUDED
#define HOT_COLOR_GRADIENT_INCLUDED


half4 HotColorGradient(half alpha, half3 edgeColor, half midColorBoost, half edgeColorCenter, half midColorCenter, half edgeBias, half coreBias) {
    half3 midColor = saturate(edgeColor * midColorBoost);
    half gradientEdgeToMid = pow(saturate((alpha - edgeColorCenter) / (midColorCenter - edgeColorCenter)), edgeBias);
    half3 edgeToMid = lerp(edgeColor, midColor, gradientEdgeToMid);
    half gradientMidToCore = saturate((1.0 - pow((1.0 - (alpha - midColorCenter)), coreBias)));
    half3 outRGB = lerp(edgeToMid, half3(1, 1, 1), gradientMidToCore);
    return half4(outRGB.rgb, (alpha / edgeColorCenter));
}

#endif //HOT_COLOR_GRADIENT_INCLUDED