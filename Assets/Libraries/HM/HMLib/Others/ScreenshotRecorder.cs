using UnityEngine;
using System.IO;

public class ScreenshotRecorder : MonoBehaviour {

    public enum RecordingType {
        Sequence,
        Stereo360Sequence,
        Mono360Sequence,
        F10ForScreenshot,
        Interval,
        ScreenshotOnPause,
    }

    [SerializeField] string _directory = "Screenshots";
    [SerializeField] Camera _camera = default;
    [SerializeField] int _frameRate = 60;
    [SerializeField] bool _forceFixedFramerate = default;
    [SerializeField] int _interval = 20;
    [SerializeField] RecordingType _recordingType = RecordingType.F10ForScreenshot;
    [SerializeField] bool _pauseWithPButton = true;
    [SerializeField] int _antiAlias = 8;
    [SerializeField] int _screenshotWidth = 1920;
    [SerializeField] int _screenshotHeight = 1080;

    public string directory {
        get => _directory;
        set => _directory = value;
    }

    private int _counter;
    private float _originalTimeScale;
    private bool _paused;
    private int _frameNum;

    private RenderTexture _cubemapLeftEye;
    private RenderTexture _cubemapRighEye;
    private RenderTexture _equirectTexture;
    private RenderTexture _cameraRenderTexture;

    protected void OnEnable() {

        if (_recordingType == RecordingType.Sequence || _recordingType == RecordingType.Stereo360Sequence || _recordingType == RecordingType.Mono360Sequence || _forceFixedFramerate) {
            Time.captureFramerate = _frameRate;
        }
        Directory.CreateDirectory(_directory);

        _counter = _interval;

        _cubemapLeftEye = new RenderTexture(1024, 1024, 24);
        _cubemapLeftEye.dimension = UnityEngine.Rendering.TextureDimension.Cube;
        _cubemapRighEye = new RenderTexture(1024, 1024, 24);
        _cubemapRighEye.dimension = UnityEngine.Rendering.TextureDimension.Cube;
        _equirectTexture = new RenderTexture(1920, 2160, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);

        _cameraRenderTexture = new RenderTexture(_screenshotWidth, _screenshotHeight, depth: 24, RenderTextureFormat.ARGB32);
        _cameraRenderTexture.antiAliasing = _antiAlias;
        _camera.targetTexture = _cameraRenderTexture;
    }

    protected void OnDisable() {

        _cubemapLeftEye.Release();
        _cubemapRighEye.Release();
        _equirectTexture.Release();
        _cameraRenderTexture.Release();

        Destroy(_cubemapLeftEye);
        Destroy(_cubemapRighEye);
        Destroy(_equirectTexture);
        Destroy(_cameraRenderTexture);
    }

    protected void LateUpdate() {

        if (_recordingType == RecordingType.Sequence) {
            SaveCameraScreenshot();
        }
        else if (_recordingType == RecordingType.Stereo360Sequence) {

            // _camera.stereoSeparation = 0.064f;
            // _camera.RenderToCubemap(_cubemapLeftEye, 63, Camera.MonoOrStereoscopicEye.Left);
            // _camera.RenderToCubemap(_cubemapRighEye, 63, Camera.MonoOrStereoscopicEye.Right);
            // _cubemapLeftEye.ConvertToEquirect(_equirectTexture, Camera.MonoOrStereoscopicEye.Left);
            // _cubemapRighEye.ConvertToEquirect(_equirectTexture, Camera.MonoOrStereoscopicEye.Right);

            // SaveTextureScreenshot(ConvertRenderTexture(_equirectTexture));
        }
        else if (_recordingType == RecordingType.Mono360Sequence) {

            // _camera.RenderToCubemap(_cubemapLeftEye, 63, Camera.MonoOrStereoscopicEye.Mono);
            // _cubemapLeftEye.ConvertToEquirect(_equirectTexture, Camera.MonoOrStereoscopicEye.Mono);

            // SaveTextureScreenshot(ConvertRenderTexture(_equirectTexture));
        }
        else if (_recordingType == RecordingType.Interval && _counter == 0) {
            SaveCameraScreenshot();
            _counter = _interval;
        }
        else if (_recordingType == RecordingType.F10ForScreenshot && Input.GetKeyDown(KeyCode.F10)) {
            SaveCameraScreenshot();
        }

        if (_counter > 0) {
            _counter--;
        }

        if (_pauseWithPButton && Input.GetKeyDown(KeyCode.P)) {
            _paused = !_paused;

            if (_paused) {
                _originalTimeScale = Time.timeScale;
                Time.timeScale = 0.0f;
            }
            else {
                Time.timeScale = _originalTimeScale;
            }
        }
    }

    private void OnApplicationFocus(bool hasFocus) {

        if (_recordingType == RecordingType.ScreenshotOnPause && hasFocus) {
            SaveCameraScreenshot();
        }
    }

    private void SaveCameraScreenshot() {

        // Convert to texture
        Texture2D texture = ConvertRenderTexture(_camera.targetTexture);

        //Save
        SaveTextureScreenshot(texture);
        Destroy(texture);
    }

    private void SaveTextureScreenshot(Texture2D tex) {

        string name = string.Format("{0}/{1:D05}.png", _directory, _frameNum);
        byte[] bytes = ImageConversion.EncodeToPNG(tex);
        File.WriteAllBytes(name, bytes);
        Debug.Log("Screenshot saved to \"" + name + "\"");

        _frameNum++;
    }

    private Texture2D ConvertRenderTexture(RenderTexture renderTexture) {

        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        RenderTexture.active = renderTexture;
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        return tex;
    }
}
