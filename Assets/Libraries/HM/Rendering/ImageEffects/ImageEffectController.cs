using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ImageEffectController : MonoBehaviour {

    public delegate void RenderImageCallback(RenderTexture src, RenderTexture dest);
    private RenderImageCallback _renderImageCallback;

    public void SetCallback(RenderImageCallback renderImageCallback) {

        _renderImageCallback = renderImageCallback;
    }

    protected void OnRenderImage(RenderTexture src, RenderTexture dest) {

        _renderImageCallback?.Invoke(src, dest);
    }
}
