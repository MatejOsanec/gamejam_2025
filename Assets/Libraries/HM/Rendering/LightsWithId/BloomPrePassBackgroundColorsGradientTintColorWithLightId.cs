using System;
using UnityEngine;

public class BloomPrePassBackgroundColorsGradientTintColorWithLightId : RuntimeLightWithIds {

    [SerializeField] BloomPrePassBackgroundColorsGradient _bloomPrePassBackgroundColorsGradient = default;
    [SerializeField] bool _useGrayscale;
    [SerializeField, DrawIf("_useGrayscale", true)] float grayscaleFactor = 0.0f;

    protected override void ColorWasSet(Color color) {

        if (_useGrayscale) {
            color = Color.Lerp(color, Color.white * color.maxColorComponent, Mathf.Clamp01(grayscaleFactor));
        }
        
        _bloomPrePassBackgroundColorsGradient.tintColor = color;
    }
}
