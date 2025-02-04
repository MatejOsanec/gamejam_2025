using UnityEngine;

public class DirectionalLightWithLightGroupIds : RuntimeLightWithLightGroupIds {

    [SerializeField] DirectionalLight _directionalLight = default;

    protected override void ColorWasSet(Color color) {

        _directionalLight.color = color;
    }
}
