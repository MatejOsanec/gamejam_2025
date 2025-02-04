using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class RuntimeLightWithIds : LightWithIds {

    [SerializeField] LightIntensitiesWithId[] _lightIntensityData = default;

    [Space]
    [SerializeField] float _intensity = 1.0f;
    [SerializeField] float _maxIntensity = 1.0f;
    [SerializeField] bool _multiplyColorByAlpha = true;
    [SerializeField] ColorMixAndWeightingApproach _mixType = ColorMixAndWeightingApproach.Maximum;

    public ColorMixAndWeightingApproach mixType => mixType;

    [Serializable]
    public class LightIntensitiesWithId : LightWithId {

        [SerializeField] float _intensity = default;
        public float intensity { get => _intensity; set => _intensity = value; }

        public LightIntensitiesWithId(int lightId, float lightIntensity) : base(lightId) {

            _intensity = lightIntensity;
        }
    }

    protected abstract void ColorWasSet(Color color);

    protected override void ProcessNewColorData() {

        Color newColor = new Color();

        foreach (var lightData in _lightIntensityData) {

            var color = ProcessColor(lightData.color, lightData.intensity);

            switch (_mixType) {

                case ColorMixAndWeightingApproach.Maximum:
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
                        break;

                case ColorMixAndWeightingApproach.FractionAndSum:
                    newColor.r += color.r;
                    newColor.g += color.g;
                    newColor.b += color.b;
                    break;
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

        switch (_mixType) {

            case ColorMixAndWeightingApproach.Maximum:
                color.a *= intensity;
                color.a = Mathf.Sqrt(color.a);
                break;
            case ColorMixAndWeightingApproach.FractionAndSum:
                color.a *= intensity;
                break;
        }
        if (_multiplyColorByAlpha) {
            color.r *= color.a;
            color.g *= color.a;
            color.b *= color.a;
        }

        return color;
    }

#if UNITY_EDITOR

    public void SetNewLightIntensityData(LightIntensitiesWithId[] lightIntensitiesWithIds) {

        _lightIntensityData = lightIntensitiesWithIds;
        EditorUtility.SetDirty(this);
        SetNewLightsWithIds(_lightIntensityData);
    }
#endif
}
