using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityLightWithId : LightWithIdMonoBehaviour {

    [SerializeField] Light _light = default;
    [SerializeField] float _intensity = 1.0f;
    [SerializeField] float _minAlpha = 0.0f;

    public Color color => _light.color;

    public override void ColorWasSet(Color color) {

        _light.color = color;
        _light.intensity = Mathf.Max(color.a, _minAlpha) * _intensity;
    }
}
