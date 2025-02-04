using System;
using Unity.Collections;
using UnityEngine;

public class BloomPrePassBackgroundColorsGradient : BloomPrePassBackgroundTextureGradient {

    [SerializeField] Element[] _elements = default;

    [Serializable]
    public class Element {
        public Color color;
        public float startT;
        public float exp;
    }
    
    public Element[] elements => _elements;

    protected override void UpdatePixels(NativeArray<Color32> pixels, int numberOfPixels) {
        
        for (int i = 0; i < numberOfPixels; i++) {
            pixels[i] = EvaluateColor((float)i / (numberOfPixels - 1));
        }
    }

    private Color EvaluateColor(float t) {

        for (int i = _elements.Length - 2; i >= 0 ; i--) {
            var element = _elements[i];
            if (t >= element.startT) {
                var nextElement = _elements[i + 1];
                return Color.LerpUnclamped(element.color, nextElement.color, Mathf.Pow((t - element.startT) / (nextElement.startT - element.startT), element.exp));
            }
        }
        return _elements[_elements.Length - 1].color;
    }
}
