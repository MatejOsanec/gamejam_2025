using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BakedLightsNormalizer : MonoBehaviour {

    [SerializeField] float _maxTotalIntensity = 1.0f;

    public Dictionary<LightConstants.BakeId, LightmapLightWithIds> lightmapLightDict => _lightmapLightDict;

    public float maxTotalIntensity {
        get => _maxTotalIntensity;
#if UNITY_EDITOR
        set {
            _maxTotalIntensity = value;
            MarkUnsavedChanges();
            UpdateMaxIntensity();
        }
#endif
    }

    private readonly Dictionary<LightConstants.BakeId, LightmapLightWithIds> _lightmapLightDict = new Dictionary<LightConstants.BakeId, LightmapLightWithIds>();
    private bool _lightmapDictInitialized = false;
    private float _grayscaleTotal = 0.0f;
    private int _lastCalculatedOnFrame = default;
    private bool _grayscaleCalculatedOnce = false;
    private bool _newUpdates = true;

#if UNITY_EDITOR
    private float _prevMaxTotalIntensity = -1.0f;
    private LightWithIdManager _lightManager = default;
#endif

    private const int kMaxFramesWithoutUpdate = 5;

    protected void LateUpdate() {

        if (_newUpdates && (Time.frameCount - _lastCalculatedOnFrame) > kMaxFramesWithoutUpdate) {
            UpdateGrayscaleTotal();
            _newUpdates = false;
        }
    }

#if UNITY_EDITOR
    protected void OnValidate() {

        if (!Mathf.Approximately(_prevMaxTotalIntensity, _maxTotalIntensity)) {
            UpdateMaxIntensity();
        }
    }

    private void UpdateMaxIntensity() {

        GetLightmapLights();
        UpdateGrayscaleTotal();

        if (_lightManager == null) {
            _lightManager = FindObjectOfType<LightWithIdManager>();
            if (_lightManager == null) {
                return;
            }
        }
        if (_lightmapLightDict.Count == 0 && !_lightmapDictInitialized) {
            GetLightmapLights();
        }

        foreach (var lightmapLight in _lightmapLightDict.Values) {
            lightmapLight.MarkChildrenColorAsSet();
        }

        _prevMaxTotalIntensity = _maxTotalIntensity;
        _lightManager.RequestUpdate();
    }

    public void MarkUnsavedChanges() {

        EditorUtility.SetDirty(this);
        PrefabUtility.RecordPrefabInstancePropertyModifications(this);
    }

#endif

    private void GetLightmapLights() {

        _lightmapLightDict.Clear();
        var bakedLights = FindObjectsOfType<LightmapLightWithIds>();
        foreach (var lightmapLight in bakedLights) {
            var bakeId = lightmapLight.bakeId;
            _lightmapLightDict[bakeId] = lightmapLight;
        }
        _lightmapDictInitialized = true;
    }

    private void UpdateGrayscaleTotal() {

        if (_lightmapLightDict.Count == 0 && !_lightmapDictInitialized) {
            GetLightmapLights();
        }

        if (Time.frameCount == _lastCalculatedOnFrame && _grayscaleCalculatedOnce) {
            return;
        }

        _grayscaleTotal = 0.0f;
        foreach (var lightmapLight in _lightmapLightDict.Values) {
            _grayscaleTotal += lightmapLight.calculatedColorPreNormalization.grayscale * lightmapLight.normalizerWeight;
        }
        _lastCalculatedOnFrame = Time.frameCount;
        _grayscaleCalculatedOnce = true;
    }

    public float GetNormalizationMultiplier() {

        UpdateGrayscaleTotal();
        _newUpdates = true;
        var globalIntensityMultiplier = _lightmapDictInitialized && _grayscaleTotal > _maxTotalIntensity ? Mathf.LinearToGammaSpace(_maxTotalIntensity / _grayscaleTotal) : 1.0f;
        return globalIntensityMultiplier;
    }
}
