using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class BlueNoiseDitheringUpdater : MonoBehaviour {

    [SerializeField] BlueNoiseDithering _blueNoiseDithering = default;
    [SerializeField] RandomValueToShader _randomValueToShader = default;

    protected void OnEnable() {
        _randomValueToShader.SetRandomSeed(0);

        Camera.onPreRender -= HandleCameraPreRender;
        Camera.onPreRender += HandleCameraPreRender;
    }

    protected void OnDisable() {

        Camera.onPreRender -= HandleCameraPreRender;
    }

    public void HandleCameraPreRender(Camera camera) {

        _randomValueToShader.SetRandomValueToShaders();
        _blueNoiseDithering.SetBlueNoiseShaderParams(camera.pixelWidth, camera.pixelHeight);
    }

#if UNITY_INCLUDE_TESTS
    internal void SetBlueNoiseDithering(BlueNoiseDithering blueNoiseDithering) {
        _blueNoiseDithering = blueNoiseDithering;
    }
#endif
}
