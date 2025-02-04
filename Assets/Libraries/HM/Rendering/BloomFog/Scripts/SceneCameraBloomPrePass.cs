using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

[ExecuteInEditMode]
public class SceneCameraBloomPrePass : MonoBehaviour {

#pragma warning disable 414
    [SerializeField] BloomPrePassRendererSO _bloomPrepassRenderer = default;
    [SerializeField] BloomPrePassEffectContainerSO _bloomPrePassEffectContainer = default;
#pragma warning restore 414

    private RenderTexture _bloomPrepassRenderTexture;

#if UNITY_EDITOR

    protected void OnEnable() {

        _bloomPrepassRenderer.Init();
        Camera.onPreRender += OnCameraPreRender;
        Camera.onPostRender += OnCameraPostRender;
    }

    protected void OnDisable() {

        Camera.onPreRender -= OnCameraPreRender;
        Camera.onPostRender -= OnCameraPostRender;
    }

    protected void OnDestroy() {

        Camera.onPreRender -= OnCameraPreRender;
        Camera.onPostRender -= OnCameraPostRender;

        if (_bloomPrepassRenderTexture != null) {
            _bloomPrepassRenderTexture.Release();
            EssentialHelpers.SafeDestroy(_bloomPrepassRenderTexture);
        }
    }

    private void OnCameraPreRender(Camera camera) {

        if (SceneView.currentDrawingSceneView == null || camera != SceneView.currentDrawingSceneView.camera) {
            return;
        }

        if (FogEnabledForCamera(camera)) {

            _bloomPrepassRenderer.GetCameraParams(camera, out var projectionMatrix, out var viewMatrix, out float stereoCameraEyeOffset);
            _bloomPrepassRenderTexture = _bloomPrepassRenderer.CreateBloomPrePassRenderTextureIfNeeded(_bloomPrepassRenderTexture, _bloomPrePassEffectContainer.bloomPrePassEffect);
            _bloomPrepassRenderer.EnableBloomFog();
            _bloomPrepassRenderer.RenderAndSetData(
                camera.transform.position,
                projectionMatrix,
                viewMatrix,
                stereoCameraEyeOffset,
                _bloomPrePassEffectContainer.bloomPrePassEffect,
                _bloomPrepassRenderTexture,
                out Vector2 textureToScreenRatio,
                out ToneMapping toneMapping
            );
            BloomPrePassRendererSO.SetDataToShaders(stereoCameraEyeOffset, textureToScreenRatio, _bloomPrepassRenderTexture, toneMapping);
        }
        else {
            _bloomPrepassRenderer.DisableBloomFog();
        }
    }

    private void OnCameraPostRender(Camera camera) {

        if (SceneView.currentDrawingSceneView == null || camera != SceneView.currentDrawingSceneView.camera) {
            return;
        }

        if (FogEnabledForCamera(camera)) {
            _bloomPrepassRenderTexture.DiscardContents();
            _bloomPrepassRenderer.DisableBloomFog();
        }
    }

    private bool FogEnabledForCamera(Camera camera) {

        // Find SceneView for camera
        foreach (SceneView sceneView in SceneView.sceneViews) {
            if (sceneView.camera == camera) {
                return sceneView.sceneViewState.showFog;
            }
        }

        return true;
    }

#endif

}
