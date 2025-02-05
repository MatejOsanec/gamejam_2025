using UnityEngine;

public class BloomFogSO : PersistentScriptableObject {

    public float transition {
        set {
            if (value == _transition) {
                return;
            }
            _transition = value;
            UpdateShaderParams();
        }
        get => _transition;
    }

    public BloomFogEnvironmentParams defaultForParams {
        get => _defaultFogParams;
        set {
            if (value == _defaultFogParams) {
                return;
            }
            _defaultFogParams = value;
            if (_transition < 1.0f) {
                UpdateShaderParams();
            }
        }
    }

    public BloomFogEnvironmentParams transitionFogParams {
        get => _transitionFogParams;
        set {
            if (value == _transitionFogParams) {
                return;
            }
            _transitionFogParams = value;
            if (_transition > 0.0f) {
                UpdateShaderParams();
            }
        }
    }

    public bool bloomFogEnabled {
        set {
            if (value == _bloomFogEnabled) {
                return;
            }

            if (value) {
                Shader.EnableKeyword(kBloomFogEnabledKeyword);
            }
            else {
                Shader.DisableKeyword(kBloomFogEnabledKeyword);
            }
            _bloomFogEnabled = value;
        }
        get => _bloomFogEnabled;
    }
    
    public bool legacyAutoExposureEnabled {
        set {
            if (value == _legacyAutoExposureEnabled) {
                return;
            }

            _legacyAutoExposureEnabled = value;
            UpdateShaderParams();

        }
        get => _legacyAutoExposureEnabled;
    }

    public float autoExposureLimit => _autoExposureLimit;
    public float noteSpawnIntensity => _noteSpawnIntensity;

    private bool _bloomFogEnabled = true;
    private bool _legacyAutoExposureEnabled;

    private float _transition;
    private float _autoExposureLimit;
    private float _noteSpawnIntensity;

    private const string kBloomFogEnabledKeyword = "ENABLE_BLOOM_FOG";

    
    private static readonly int _customFogAttenuationID = Shader.PropertyToID("_CustomFogAttenuation");
    
    private static readonly int _customFogOffsetID = Shader.PropertyToID("_CustomFogOffset");
    
    private static readonly int _customFogHeightFogStartYID = Shader.PropertyToID("_CustomFogHeightFogStartY");
    
    private static readonly int _customFogHeightFogHeightID = Shader.PropertyToID("_CustomFogHeightFogHeight");

    private BloomFogEnvironmentParams _defaultFogParams;
    private BloomFogEnvironmentParams _transitionFogParams;

    public void Setup(BloomFogEnvironmentParams defaultFogParams) {

        _defaultFogParams = _transitionFogParams = defaultFogParams;
    }

    protected override void OnEnable() {

        base.OnEnable();
        UpdateShaderParams();
    }

    public void UpdateShaderParams() {

        if (_defaultFogParams == null) {
            return;
        }

        if (_transitionFogParams == null || _transition <= Mathf.Epsilon) {
            SetParams(_defaultFogParams.attenuation, _defaultFogParams.offset, _defaultFogParams.heightFogStartY, _defaultFogParams.heightFogHeight, _defaultFogParams.autoExposureLimit, _defaultFogParams.noteSpawnIntensity);
        }
        else if (_transition == 1.0f) {
            SetParams(_transitionFogParams.attenuation, _transitionFogParams.offset, _transitionFogParams.heightFogStartY, _transitionFogParams.heightFogHeight, _transitionFogParams.autoExposureLimit, _transitionFogParams.noteSpawnIntensity);
        }
        else {
            float attenuation = Mathf.Lerp(_defaultFogParams.attenuation, _transitionFogParams.attenuation, _transition);
            float offset = Mathf.Lerp(_defaultFogParams.offset, _transitionFogParams.offset, _transition);
            float heightFogStartY = Mathf.Lerp(_defaultFogParams.heightFogStartY, _transitionFogParams.heightFogStartY, _transition);
            float heightFogHeight = Mathf.Lerp(_defaultFogParams.heightFogHeight, _transitionFogParams.heightFogHeight, _transition);
            float autoExposureLimit = Mathf.Lerp(_defaultFogParams.autoExposureLimit, _transitionFogParams.autoExposureLimit, _transition);
            float noteSpawnIntensity = Mathf.Lerp(_defaultFogParams.noteSpawnIntensity, _transitionFogParams.noteSpawnIntensity, _transition);
            SetParams(attenuation, offset, heightFogStartY, heightFogHeight, autoExposureLimit, noteSpawnIntensity);
        }

    }

    private void SetParams(float attenuation, float offset, float heightFogStartY, float heightFogHeight, float autoExposureLimit, float noteSpawnIntensity) {

        Shader.SetGlobalFloat(_customFogAttenuationID, attenuation);
        Shader.SetGlobalFloat(_customFogOffsetID, offset);
        Shader.SetGlobalFloat(_customFogHeightFogStartYID, heightFogStartY);
        Shader.SetGlobalFloat(_customFogHeightFogHeightID, heightFogHeight);
        _autoExposureLimit = autoExposureLimit;
        _noteSpawnIntensity = noteSpawnIntensity;
        legacyAutoExposureEnabled = _transition < 0.5f ? _defaultFogParams.legacyAutoExposure : _transitionFogParams.legacyAutoExposure;

    }
}
