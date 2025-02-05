using System;
using System.Collections.Generic;
using UnityEngine;

public class GlobalShaderColorLightWithIds : LightWithIds {

    [SerializeField] LightIntensitiesWithId[] _lightIntensityData = default;
    [SerializeField] bool _overrideSaturation;
    [SerializeField] [DrawIf("_overrideSaturation", true)] float _saturation = 0.5f;
#if UNITY_EDITOR
    [SerializeField] Color _editorTestColor = Color.white;
#endif

    
    private static readonly int _globalLightTintColorPropertyId = Shader.PropertyToID("_GlobalLightTintColor");

    [Serializable]
    public class LightIntensitiesWithId : LightWithId {

        [SerializeField] float _intensity = default;
        public float intensity { get => _intensity; set => _intensity = value; }

        public LightIntensitiesWithId(int lightId, float lightIntensity) : base(lightId) {

            _intensity = lightIntensity;
        }
    }

    protected override void ProcessNewColorData() {

        Color newColor = new Color();

        foreach (var lightData in _lightIntensityData) {

            var color = lightData.color;
            var intensity = lightData.intensity * color.a;
            newColor.r += color.r * intensity;
            newColor.g += color.g * intensity;
            newColor.b += color.b * intensity;
        }

        newColor /= _lightIntensityData.Length;

        Color.RGBToHSV(newColor, out float h, out float s, out float v);
        v = 1.0f;

        if (_overrideSaturation) {
            s = _saturation;
        }

        Shader.SetGlobalColor(_globalLightTintColorPropertyId, Color.HSVToRGB(h, s, v));
    }

    protected override IEnumerable<LightWithId> GetLightWithIds() => _lightIntensityData;

#if UNITY_EDITOR

    protected void OnValidate() {

        if (Application.isPlaying) {
            return;
        }

        Color.RGBToHSV(_editorTestColor, out float h, out float s, out float v);
        v = 1.0f;

        if (_overrideSaturation) {
            s = _saturation;
        }

        Shader.SetGlobalColor(_globalLightTintColorPropertyId, Color.HSVToRGB(h, s, v));
    }

#endif
}
