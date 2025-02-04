using System;
using UnityEngine;

public class BloomPrePassBackgroundColorsGradientTintColorWithLightIds : LightWithIdMonoBehaviour {

    [SerializeField] BloomPrePassBackgroundColorsGradient _bloomPrePassBackgroundColorsGradient = default;

    public override void ColorWasSet(Color color) {

        _bloomPrePassBackgroundColorsGradient.tintColor = color;
    }
}
