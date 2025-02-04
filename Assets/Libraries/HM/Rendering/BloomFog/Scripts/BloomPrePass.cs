using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class BloomPrePass : MonoBehaviour {

    [SerializeField] BloomPrePassRendererSO _bloomPrepassRenderer = default;
    [SerializeField] BloomPrePassEffectContainerSO _bloomPrePassEffectContainer = default;
    [Tooltip("This is used to share same render data with two BloomPrePass objects. We need this for efficient implementation of mixed reality background and foreground camera. Null is allowed.")]
    [SerializeField] [NullAllowed] BloomPrePassRenderDataSO _bloomPrePassRenderData = default;
    [SerializeField] Mode _mode = Mode.RenderAndSetData;

    public enum Mode {
        RenderAndSetData,
        SetDataOnly
    }

    private BloomPrePassRenderDataSO.Data _renderData;

    protected void Awake() {

        LazyInit();
    }

    private void LazyInit() {

        if (_renderData != null) {
            return;
        }

        _renderData = _bloomPrePassRenderData == null ? new BloomPrePassRenderDataSO.Data() : _bloomPrePassRenderData.data;
        _bloomPrepassRenderer.Init();
    }

    protected void OnDestroy() {

        if (_renderData == null || _renderData.bloomPrePassRenderTexture == null) {
            return;
        }

        _renderData.bloomPrePassRenderTexture.Release();
        EssentialHelpers.SafeDestroy(_renderData.bloomPrePassRenderTexture);
        _renderData.bloomPrePassRenderTexture = null;
    }

    protected void OnPreRender() {

#if UNITY_EDITOR
        if (!Application.isPlaying) {
            LazyInit();
        }
#endif

        if (_mode == Mode.RenderAndSetData) {

            var camera = Camera.current;

            _bloomPrepassRenderer.GetCameraParams(camera, out _renderData.projectionMatrix, out _renderData.viewMatrix, out _renderData.stereoCameraEyeOffset);
            _renderData.bloomPrePassRenderTexture = _bloomPrepassRenderer.CreateBloomPrePassRenderTextureIfNeeded(_renderData.bloomPrePassRenderTexture, _bloomPrePassEffectContainer.bloomPrePassEffect);
            _bloomPrepassRenderer.RenderAndSetData(
                cameraPos: camera.transform.position,
                projectionMatrix: _renderData.projectionMatrix,
                viewMatrix: _renderData.viewMatrix,
                stereoCameraEyeOffset: _renderData.stereoCameraEyeOffset,
                bloomPrePassParams: _bloomPrePassEffectContainer.bloomPrePassEffect,
                dest: _renderData.bloomPrePassRenderTexture,
                textureToScreenRatio: out _renderData.textureToScreenRatio,
                toneMapping: out _renderData.toneMapping
            );
            _bloomPrepassRenderer.EnableBloomFog();;
            BloomPrePassRendererSO.SetDataToShaders(_renderData.stereoCameraEyeOffset, _renderData.textureToScreenRatio, _renderData.bloomPrePassRenderTexture, _renderData.toneMapping);
        }
        // Mode.SetDataOnly works only if bloomPrePassRenderTexture was rendered before.
        else if (_renderData.bloomPrePassRenderTexture != null) {

            _bloomPrepassRenderer.EnableBloomFog();
            BloomPrePassRendererSO.SetDataToShaders(_renderData.stereoCameraEyeOffset, _renderData.textureToScreenRatio, _renderData.bloomPrePassRenderTexture, _renderData.toneMapping);
        }
    }

    protected void OnPostRender() {

#if UNITY_EDITOR
        if (!Application.isPlaying) {
            LazyInit();
        }
#endif

        if (_renderData.bloomPrePassRenderTexture != null) {
            _renderData.bloomPrePassRenderTexture.DiscardContents();
        }
        _bloomPrepassRenderer.DisableBloomFog();
    }

    public void SetMode(Mode mode) {

        _mode = mode;
    }
}
