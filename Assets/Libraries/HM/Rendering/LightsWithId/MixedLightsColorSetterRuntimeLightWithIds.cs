using System.Collections;
using UnityEngine;

public class MixedLightsColorSetterRuntimeLightWithIds : RuntimeLightWithIds {

    [SerializeField] MaterialPropertyBlockColorSetter _materialPropertyBlockColorSetter = default;
    [SerializeField] float _lightMultiplier = 1.0f;

    protected override void ColorWasSet(Color color) {
        
        _materialPropertyBlockColorSetter.SetColor(_lightMultiplier * color);
    }
}
