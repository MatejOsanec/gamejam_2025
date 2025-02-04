using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class BloomFogEnvironment : MonoBehaviour {

    [SerializeField] BloomFogSO _bloomFog = default;

    [Space]
    [FormerlySerializedAs("_fog0Params")]
    [SerializeField] BloomFogEnvironmentParams _fogParams = default;

    public BloomFogEnvironmentParams fogParams => _fogParams;

    protected void OnEnable() {

        _bloomFog.transition = 0.0f;
        _bloomFog.Setup(_fogParams);

    }

    protected void OnValidate() {

        if (Application.isPlaying) {
            return;
        }

        _bloomFog.Setup(_fogParams);
        _bloomFog.bloomFogEnabled = true;
        _bloomFog.legacyAutoExposureEnabled = false;
        _bloomFog.UpdateShaderParams();
    }
}
