using System;
using System.Collections.Generic;
using UnityEngine;

public class LightmapLightsWithIds : LightWithIds {

    [SerializeField] float _maxTotalIntensity = 1.0f;
    [SerializeField] LightIntensitiesWithId[] _lightIntensityData = default;

    public float maxTotalIntensity { get => _maxTotalIntensity; set => _maxTotalIntensity = value; }

    [Serializable]
    public class LightIntensitiesWithId : LightWithId {

        [SerializeField] LightConstants.BakeId _bakeId = default;
        [SerializeField] float _intensity = default;
        [SerializeField] float _weight = default;

        public LightConstants.BakeId bakeId => _bakeId;
        public float intensity { get => _intensity; set => _intensity = value; }
        public float weight { get => _weight; set => _weight = value; }

        private int _lightmapLightIdColorPropertyId;
        private int _lightProbeLightIdColorPropertyId;
        private bool _initializedPropertyIds;

        public void SetDataToShaders(Color lightmapColor, Color probeColor) {

            if (_initializedPropertyIds == false) {
                _lightmapLightIdColorPropertyId = LightConstants.GetLightmapLightBakeIdPropertyId(_bakeId);
                _lightProbeLightIdColorPropertyId = LightConstants.GetLightProbeLightBakeIdPropertyId(_bakeId);
                _initializedPropertyIds = true;
            }

            Shader.SetGlobalColor(_lightmapLightIdColorPropertyId, lightmapColor);
            Shader.SetGlobalColor(_lightProbeLightIdColorPropertyId, probeColor);
        }
    }

    protected override void ProcessNewColorData() {

        float grayscaleSum = 0.0f;
        foreach (var lightData in _lightIntensityData) {
            grayscaleSum += lightData.color.grayscale * lightData.weight;
        }

        float globalIntensity = 1.0f;
        if (grayscaleSum > _maxTotalIntensity) {
            globalIntensity = _maxTotalIntensity / grayscaleSum;
        }

        globalIntensity = Mathf.LinearToGammaSpace(globalIntensity);

        foreach (var lightData in _lightIntensityData) {

            var lightmapColor = lightData.color;
            var lightmapIntensity = (lightData.intensity * globalIntensity * lightmapColor.a);
            lightmapColor.r *= lightmapIntensity;
            lightmapColor.g *= lightmapIntensity;
            lightmapColor.b *= lightmapIntensity;

            var probeColor = lightData.color;
            var probeIntensity = Mathf.LinearToGammaSpace(probeColor.a) * globalIntensity;
            probeColor.r *= probeIntensity;
            probeColor.g *= probeIntensity;
            probeColor.b *= probeIntensity;
            probeColor.a *= 2.0f * globalIntensity;

            lightData.SetDataToShaders(lightmapColor.linear, probeColor.linear);
        }
    }

    protected override IEnumerable<LightWithId> GetLightWithIds() => _lightIntensityData;

#if UNITY_EDITOR

    public LightIntensitiesWithId __GetLightData(int lightId) {

        foreach (var lightData in _lightIntensityData) {
            if (lightData.lightId == lightId) {
                return lightData;
            }
        }
        return null;
    }
#endif
}
