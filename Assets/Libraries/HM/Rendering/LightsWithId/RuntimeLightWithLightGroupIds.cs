using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class RuntimeLightWithLightGroupIds : LightWithIds {

    [SerializeField] LightGroup[] _lightGroupList;

    [Space]
    [SerializeField] float _intensity = 1.0f;
    [SerializeField] float _maxIntensity = 1.0f;
    [SerializeField] bool _multiplyColorByAlpha = true;

    [Serializable]
    public class LightIntensitiesWithId : LightWithId {

        [SerializeField] float _intensity = default;
        public float intensity { get => _intensity; set => _intensity = value; }

        public LightIntensitiesWithId(int lightId, float intensity) : base(lightId) {
            _intensity = intensity;
        }
    }

    private LightIntensitiesWithId[] _lightIntensityData;

    protected abstract void ColorWasSet(Color color);

    protected override void Awake() {

        var totalNumberOfElements = 0;
        foreach (var lightGroup in _lightGroupList) {
            totalNumberOfElements += lightGroup.lightGroupSO.numberOfElements;
        }
        _lightIntensityData = new LightIntensitiesWithId[totalNumberOfElements];
        int j = 0;
        foreach (var lightGroup in _lightGroupList) {
            for (int i = 0; i < lightGroup.lightGroupSO.numberOfElements; i++) {
                _lightIntensityData[j] = new LightIntensitiesWithId(lightId:i + lightGroup.lightGroupSO.startLightId, intensity:1.0f);
                j++;
            }
        }

        base.Awake();

    }

    protected override void ProcessNewColorData() {

        Color newColor = new Color();

        for (int i = 0; i < _lightIntensityData.Length; i++) {

            var lightIntensityData = _lightIntensityData[i];
            var color = ProcessColor(lightIntensityData.color, lightIntensityData.intensity);

            if (newColor.r < color.r) {
                newColor.r = color.r;
            }
            if (newColor.g < color.g) {
                newColor.g = color.g;
            }
            if (newColor.b < color.b) {
                newColor.b = color.b;
            }
            if (newColor.a < color.a) {
                newColor.a = color.a;
            }
        }

        if (_multiplyColorByAlpha) {

            newColor *= _intensity;
            float gray = newColor.grayscale;

            if (gray > _maxIntensity) {
                newColor /= (gray / _maxIntensity);
            }
        }
        else {

            newColor.a *= _intensity;
            newColor.a = Mathf.Min(_maxIntensity, newColor.a);
        }

        ColorWasSet(newColor);
    }

    protected override IEnumerable<LightWithId> GetLightWithIds() => _lightIntensityData;

    private Color ProcessColor(Color color, float intensity) {

        color.a *= intensity;
        if (_multiplyColorByAlpha) {
            color.a = Mathf.Sqrt(color.a);
            color.r *= color.a;
            color.g *= color.a;
            color.b *= color.a;
        }

        return color;
    }
}
