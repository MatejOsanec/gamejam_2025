using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LightmapLightWithIds : LightWithIds {

    [SerializeField] LightConstants.BakeId _bakeId = default;
    [SerializeField] float _intensity = 1.0f;
    [SerializeField] float _probeIntensity = 1.0f;
    [SerializeField] LightIntensitiesWithId[] _lightIntensityData = default;
    [SerializeField] ColorMixAndWeightingApproach _mixType = ColorMixAndWeightingApproach.Maximum;
    [SerializeField] float _normalizerWeight = 1.0f;

    public ColorMixAndWeightingApproach mixType => _mixType;

    [Serializable]
    public class LightIntensitiesWithId : LightWithId {

        [SerializeField] float _intensity = default;
        [SerializeField] float _probeHighlightsIntensityMultiplier = 1.0f;

        public float intensity => _intensity;
        public float probeHighlightsIntensityMultiplier => _probeHighlightsIntensityMultiplier;

        public LightIntensitiesWithId(int lightId, float lightIntensity, float probeMultiplier) : base(lightId) {

            _intensity = lightIntensity;
            _probeHighlightsIntensityMultiplier = probeMultiplier;
        }
    }

    public float intensity { get => _intensity; set => _intensity = value; }
    public float normalizerWeight { get => _normalizerWeight; set => _normalizerWeight = value; }
    public Color calculatedColorPreNormalization => _calculatedColorPreNormalization;
    public LightConstants.BakeId bakeId => _bakeId;

    private BakedLightsNormalizer _bakedLightsNormalizer;
    private int _lightmapLightIdColorPropertyId;
    private int _lightProbeLightIdColorPropertyId;
    private bool _initializedPropertyIds;
    private bool _initializedNormalizer = default;
    private bool _isNormalizerInScene = false;
    private Color _calculatedColorPreNormalization;

    protected override void Awake() {

        base.Awake();

        GetBakedLightsNormalizer();
        SetShaderProperties();
        SetDataToShaders(Color.clear, Color.clear);
    }

    public void SetChannelColorDirect(Color channelColor) {

        Shader.SetGlobalColor(_lightProbeLightIdColorPropertyId, channelColor);
    }

    protected override void ProcessNewColorData() {

        if (_initializedPropertyIds == false) {
            SetShaderProperties();
        }

        if (_initializedNormalizer == false) {
            GetBakedLightsNormalizer();
        }

        //Preprocessor block below to handle different needs for runtime vs editormode
        //At runtime we only check once, null is a valid result handled for bakward comp with legacy scenes
        //In editormode we allow costly rechecks as the components may be created / deleted during scene building
#if UNITY_EDITOR
        if (!Application.isPlaying && !_isNormalizerInScene) {
            GetBakedLightsNormalizer();
        }
#endif

        Color totalLightmapColor = new Color();
        Color totalProbeColor = new Color();

        //The first LightmapLightWithIds (A-F) which requests the multiplier from BakedLightsNormalizer on a given frame
        //triggers a recalculation (using previous frame's colors). Subsequent requests to BakedLightsNormalizer on the same
        //frame return cached value for this frame.

        var globalIntensityMultiplier = _isNormalizerInScene ? _bakedLightsNormalizer.GetNormalizationMultiplier() : 1.0f;

        foreach (var lightData in _lightIntensityData) {

            var lightDataIntensity = lightData.intensity;
            var lightmapColor = lightData.color;
            var probeColor = lightmapColor;

            var lightmapIntensity = lightDataIntensity * lightmapColor.a;
            lightmapColor.r *= lightmapIntensity;
            lightmapColor.g *= lightmapIntensity;
            lightmapColor.b *= lightmapIntensity;

            var probeIntensity = Mathf.LinearToGammaSpace(probeColor.a) * lightDataIntensity;
            probeColor.r *= probeIntensity;
            probeColor.g *= probeIntensity;
            probeColor.b *= probeIntensity;
            probeColor.a *= 2.0f * lightDataIntensity * lightData.probeHighlightsIntensityMultiplier;

            switch (_mixType) {

                case ColorMixAndWeightingApproach.Maximum:
                    if (totalLightmapColor.r < lightmapColor.r) {
                        totalLightmapColor.r = lightmapColor.r;
                    }
                    if (totalLightmapColor.g < lightmapColor.g) {
                        totalLightmapColor.g = lightmapColor.g;
                    }
                    if (totalLightmapColor.b < lightmapColor.b) {
                        totalLightmapColor.b = lightmapColor.b;
                    }

                    if (totalProbeColor.r < probeColor.r) {
                        totalProbeColor.r = probeColor.r;
                    }
                    if (totalProbeColor.g < probeColor.g) {
                        totalProbeColor.g = probeColor.g;
                    }
                    if (totalProbeColor.b < probeColor.b) {
                        totalProbeColor.b = probeColor.b;
                    }
                    if (totalProbeColor.a < probeColor.a) {
                        totalProbeColor.a = probeColor.a;
                    }
                    break;

                case ColorMixAndWeightingApproach.FractionAndSum:
                    totalLightmapColor.r += lightmapColor.r;
                    totalLightmapColor.g += lightmapColor.g;
                    totalLightmapColor.b += lightmapColor.b;
                    totalLightmapColor.a += lightmapColor.a;

                    totalProbeColor.r += probeColor.r;
                    totalProbeColor.g += probeColor.g;
                    totalProbeColor.b += probeColor.b;
                    totalProbeColor.a += probeColor.a;
                    break;
            }
        }

        // Only lightmaps intensity can be changed. Reflection probes are always the same.
        totalLightmapColor *= _intensity;
        totalProbeColor *= _probeIntensity;
        _calculatedColorPreNormalization = totalProbeColor.linear;

        SetDataToShaders(totalLightmapColor.linear, globalIntensityMultiplier * totalProbeColor.linear);
    }

    protected override IEnumerable<LightWithId> GetLightWithIds() => _lightIntensityData;

    private void SetDataToShaders(Color lightmapColor, Color probeColor) {

        Shader.SetGlobalColor(_lightmapLightIdColorPropertyId, lightmapColor);
        Shader.SetGlobalColor(_lightProbeLightIdColorPropertyId, probeColor);
    }

    private void SetShaderProperties() {

        _lightmapLightIdColorPropertyId = LightConstants.GetLightmapLightBakeIdPropertyId(_bakeId);
        _lightProbeLightIdColorPropertyId = LightConstants.GetLightProbeLightBakeIdPropertyId(_bakeId);
        _initializedPropertyIds = true;
    }

    private void GetBakedLightsNormalizer() {

        _bakedLightsNormalizer = FindObjectOfType<BakedLightsNormalizer>();
        _isNormalizerInScene = _bakedLightsNormalizer != null;
        _initializedNormalizer = true;
    }

#if UNITY_EDITOR

    public void SetNewLightIntensityData(LightIntensitiesWithId[] lightIntensitiesWithIds) {

        _lightIntensityData = lightIntensitiesWithIds;
        EditorUtility.SetDirty(this);
        SetNewLightsWithIds(_lightIntensityData);
    }

    protected void OnValidate() {

        GetBakedLightsNormalizer();
    }
#endif
}
