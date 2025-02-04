using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalLightWithId : LightWithIdMonoBehaviour {

    [SerializeField] DirectionalLight _light = default;
    [SerializeField] float _intensity = 1.0f;
    [SerializeField] float _minIntensity = 0.0f;

    public override void ColorWasSet(Color color) {

        _light.intensity = Mathf.Max(color.a * _intensity, _minIntensity);        
        _light.color = color;
    }
}
