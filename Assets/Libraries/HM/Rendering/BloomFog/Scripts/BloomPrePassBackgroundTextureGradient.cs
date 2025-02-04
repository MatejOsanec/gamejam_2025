using Unity.Collections;
using UnityEngine;

[ExecuteAlways]
public abstract class BloomPrePassBackgroundTextureGradient : BloomPrePassNonLightPass {

    [SerializeField] Color _tintColor = Color.white;

    public Color tintColor { get => _tintColor; set => _tintColor = value; }

    [DoesNotRequireDomainReloadInit]
    private static readonly int _gradientTexID = Shader.PropertyToID("_GradientTex");
    [DoesNotRequireDomainReloadInit]
    private static readonly int _inverseProjectionMatrixID = Shader.PropertyToID("_InverseProjectionMatrix");
    [DoesNotRequireDomainReloadInit]
    private static readonly int _cameraToWorldMatrixID = Shader.PropertyToID("_CameraToWorldMatrix");
    [DoesNotRequireDomainReloadInit]
    private static readonly int _colorID = Shader.PropertyToID("_Color");

    private const string kUseToneMappingKeyword = "USE_TONE_MAPPING";
    private const string kSkyGradientShaderName = "Hidden/SkyGradient";

    private const int kTextureWidth = 128;

    private Texture2D _texture;
    private Material _material;

    private void InitIfNeeded() {

        if (_material != null && _texture != null) {
            return;
        }

        EssentialHelpers.SafeDestroy(_texture);
        EssentialHelpers.SafeDestroy(_material);

        _texture = new Texture2D(width:kTextureWidth, height:1, TextureFormat.RGBA32, mipChain:false, linear:false) { name = "SkyGradient", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
        _material = new Material(Shader.Find(kSkyGradientShaderName));
        _material.SetTexture(_gradientTexID, _texture);
        if (executionTimeType == ExecutionTimeType.AfterBlur) {
            _material.EnableKeyword(kUseToneMappingKeyword);
        }
        else {
            _material.DisableKeyword(kUseToneMappingKeyword);
        }
    }

    protected void Start() {

        UpdateGradientTexture();
    }

    protected void OnDestroy() {

        EssentialHelpers.SafeDestroy(_texture);
        EssentialHelpers.SafeDestroy(_material);
    }

    protected abstract void UpdatePixels(NativeArray<Color32> pixels, int numberOfPixels);

    protected override void OnValidate() {

        base.OnValidate();

        if (_material != null && _texture != null) {
            UpdateGradientTexture();
        }
    }

    public void UpdateGradientTexture() {

        InitIfNeeded();

        var rawTextureData = _texture.GetRawTextureData<Color32>();
        UpdatePixels(rawTextureData, kTextureWidth);
        _texture.Apply();
    }

    public override void Render(RenderTexture dest, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix) {

        InitIfNeeded();

        _material.SetMatrix(_inverseProjectionMatrixID, projectionMatrix.inverse);
        _material.SetMatrix(_cameraToWorldMatrixID, viewMatrix.inverse);
        _material.SetColor(_colorID, _tintColor);

        Graphics.Blit(null, dest, _material);
    }
}
