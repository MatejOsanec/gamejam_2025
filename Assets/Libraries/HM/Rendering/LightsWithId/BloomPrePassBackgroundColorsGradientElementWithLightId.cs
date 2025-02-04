using System;
using UnityEngine;

public class BloomPrePassBackgroundColorsGradientElementWithLightId : LightWithIdMonoBehaviour {

    [SerializeField] BloomPrePassBackgroundColorsGradient _bloomPrePassBackgroundColorsGradient = default;
    [SerializeField] Elements[] _elements = default;

    [Serializable]
    public class Elements {
        public int elementNumber = 0;
        public float intensity = 1.0f;
        public float minIntensity = 0.0f;
    }
    
    public override void ColorWasSet(Color color) {

        foreach (var element in _elements) {
            _bloomPrePassBackgroundColorsGradient.elements[element.elementNumber].color = color * Mathf.Max(color.a * element.intensity, element.minIntensity);    
        }
        _bloomPrePassBackgroundColorsGradient.UpdateGradientTexture();
    }
}
