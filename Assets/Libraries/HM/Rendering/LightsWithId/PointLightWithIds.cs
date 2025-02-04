using UnityEngine;

public class PointLightWithIds : RuntimeLightWithIds {

    [SerializeField] PointLight _pointLight = default;

    protected override void ColorWasSet(Color color) {

        _pointLight.color = color;
    }
}