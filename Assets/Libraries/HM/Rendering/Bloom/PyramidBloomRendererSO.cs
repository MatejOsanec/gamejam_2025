using UnityEngine;

public class PyramidBloomRendererSO : PersistentScriptableObject {

    [SerializeField] Shader _shader = default;

    private Material _material;

    // The order of passes is important, because it matches the shader. Do not modify this.
    public enum Pass {

        Prefilter13 = 0,
        Prefilter4 = 1,
        Downsample13 = 2,
        Downsample4 = 3,
        DownsampleBilinearGamma = 4,
        UpsampleTent = 5,
        UpsampleBox = 6,
        UpsampleTentGamma = 7,
        UpsampleBoxGamma = 8,
        Bilinear = 9,
        BilinearGamma = 10,
        UpsampleTentAndReinhardToneMapping = 11,
        UpsampleTentAndACESToneMapping = 12,
        UpsampleTentAndACESToneMappingGlobalIntensity = 13,
    }

    // [down,up]
    private Level[] _pyramid;
    private const int kMaxPyramidSize = 16; // Just to make sure we handle 64k screens... Future-proof!

    private struct Level {
        internal RenderTexture down;
        internal RenderTexture up;
    }

    [DoesNotRequireDomainReloadInit]
    private static readonly int _bloomTexID = Shader.PropertyToID("_BloomTex");
    [DoesNotRequireDomainReloadInit]
    private static readonly int _globalIntensityTex = Shader.PropertyToID("_GlobalIntensityTex");
    [DoesNotRequireDomainReloadInit]
    private static readonly int _autoExposureLimitID = Shader.PropertyToID("_AutoExposureLimit");
    [DoesNotRequireDomainReloadInit]
    private static readonly int _sampleScaleID = Shader.PropertyToID("_SampleScale");
    [DoesNotRequireDomainReloadInit]
    private static readonly int _combineSrcID = Shader.PropertyToID("_CombineSrc");
    [DoesNotRequireDomainReloadInit]
    private static readonly int _combineDstID = Shader.PropertyToID("_CombineDst");
    [DoesNotRequireDomainReloadInit]
    private static readonly int _alphaWeightsID = Shader.PropertyToID("_AlphaWeights");

    private readonly string kIsScreenspaceEffectKeyword = "IS_SCREENSPACE_EFFECT";
    private readonly string kLegacyAutoExposureKeyword = "LEGACY_AUTOEXPOSURE";

    private bool _initialized;

    protected override void OnEnable() {

        base.OnEnable();

        if (_initialized) {
            return;
        }

        _initialized = true;

        _material = new Material(_shader);
        _material.hideFlags = HideFlags.HideAndDontSave;

        _pyramid = new Level[kMaxPyramidSize];
        for (int i = 0; i < kMaxPyramidSize; i++) {
            _pyramid[i] = new Level();
            _pyramid[i].down = null;
            _pyramid[i].up = null;
        }
    }

    protected void OnDisable() {

        _initialized = false;

        EssentialHelpers.SafeDestroy(_material);
        _pyramid = null;
    }

    public void RenderBloom(RenderTexture src, RenderTexture dest, float radius, bool alphaWeights, bool betterQuality, bool gammaCorrection, bool legacyAutoExposure) {

        var qualityOffset = betterQuality ? 0 : 1;
        Pass preFilterPass = (alphaWeights ? Pass.Prefilter13 : Pass.Downsample13) + qualityOffset;
        Pass downsamplePass = Pass.Downsample13 + qualityOffset;
        Pass upsamplePass = Pass.UpsampleTent + qualityOffset;
        Pass finalUpsamplePass = (gammaCorrection ? Pass.UpsampleTentGamma : Pass.UpsampleTent) + qualityOffset;

        RenderBloom(
            src,
            dest,
            radius,
            intensity: 1.0f,
            autoExposureLimit: 1000.0f,
            downIntensityOffset: 1.0f,
            uniformPyramidWeights: true,
            downsampleOnFirstPass: true,
            pyramidWeightsParam: 1.0f,
            alphaWeights: 1.0f,
            firstUpsampleBrightness: 1.0f,
            finalUpsampleBrightness: 1.0f,
            preFilterPass,
            downsamplePass,
            upsamplePass,
            finalUpsamplePass,
            legacyAutoExposure
        );
    }

    public void RenderBloom(
        RenderTexture src,
        RenderTexture dest,
        float radius,
        float intensity,
        float autoExposureLimit,
        float downIntensityOffset,
        bool uniformPyramidWeights,
        bool downsampleOnFirstPass,
        float pyramidWeightsParam,
        float alphaWeights,
        float firstUpsampleBrightness,
        float finalUpsampleBrightness,
        Pass preFilterPass,
        Pass downsamplePass,
        Pass upsamplePass,
        Pass finalUpsamplePass,
        bool legacyAutoExposure,
        bool isScreenspaceEffect = false
    ) {

        var descriptor = dest.descriptor;
        descriptor.depthBufferBits = 0;
        descriptor.msaaSamples = 1;
        //descriptor.vrUsage = VRTextureUsage.None;
        if (downsampleOnFirstPass) {
            descriptor.width /= 2;
            descriptor.height /= 2;
        }
        else {
            descriptor.width = descriptor.width;
            descriptor.height = descriptor.height;
        }

        if (isScreenspaceEffect) {
            _material.EnableKeyword(kIsScreenspaceEffectKeyword);
        }
        else {
            _material.DisableKeyword(kIsScreenspaceEffectKeyword);
        }

        if (legacyAutoExposure) {
            Shader.EnableKeyword(kLegacyAutoExposureKeyword);
        }
        else {
            Shader.DisableKeyword(kLegacyAutoExposureKeyword);
        }

        // Determine the iteration count
        int s = Mathf.Max(descriptor.width, descriptor.height);
        float logs = Mathf.Log(s, 2.0f) + Mathf.Min(radius, 10.0f) - 10.0f;
        int logs_i = Mathf.FloorToInt(logs);
        int iterations = Mathf.Clamp(logs_i, 1, kMaxPyramidSize);
        float sampleScale = 0.5f + logs - logs_i;
        _material.SetFloat(_sampleScaleID, sampleScale);
        _material.SetFloat(_alphaWeightsID, alphaWeights);

        _material.SetFloat(_combineDstID, 1.0f);
        _material.SetFloat(_combineSrcID, intensity);

        // Downsample
        var lastDown = src;
        for (int i = 0; i < iterations; i++) {
            var pass = (i == 0) ? preFilterPass : downsamplePass;
            _pyramid[i].down = RenderTexture.GetTemporary(descriptor);
            if (i > 0) {
                _pyramid[i].up = RenderTexture.GetTemporary(descriptor);
            }
            var mipDown = _pyramid[i].down;
            Graphics.Blit(lastDown, mipDown, _material, (int)pass);
            lastDown = mipDown;
            descriptor.width = Mathf.Max(descriptor.width / 2, 1);
            descriptor.height = Mathf.Max(descriptor.height / 2, 1);
        }

        _material.SetTexture(_globalIntensityTex, lastDown);
        _material.SetFloat(_autoExposureLimitID, autoExposureLimit);

        if (uniformPyramidWeights) {
            _material.SetFloat(_combineDstID, 1.0f);
            _material.SetFloat(_combineSrcID, intensity);
        }

        // Upsample
        var lastUp = _pyramid[iterations - 1].down;
        for (int i = iterations - 2; i >= 0; i--) {
            var mipDown = _pyramid[i].down;
            var mipUp = i == 0 ? dest : _pyramid[i].up;
            if (!uniformPyramidWeights) {
                float combineSrc = Mathf.Min(1.0f, Mathf.Pow(intensity * (float)(i + 1) / (iterations - 1), pyramidWeightsParam));
                float combineDst = Mathf.Min(1.0f, 1.0f + downIntensityOffset - combineSrc);
                float brightness = 1.0f;
                if (i == 0) {
                    brightness = finalUpsampleBrightness;
                }
                else if (i == iterations - 2) {
                    brightness = firstUpsampleBrightness;
                }
                _material.SetFloat(_combineSrcID, combineSrc * brightness);
                _material.SetFloat(_combineDstID, combineDst * brightness);
            }
            _material.SetTexture(_bloomTexID, mipDown);
            var pass = i == 0 ? finalUpsamplePass : upsamplePass;
            Graphics.Blit(lastUp, mipUp, _material, (int)pass);
            lastUp = mipUp;
        }

        // Unbind temporary texture from material
        _material.SetTexture(_bloomTexID, null);
        _material.SetTexture(_globalIntensityTex, null);

        // Cleanup
        for (int i = 0; i < iterations; i++) {

            if (_pyramid[i].down != null) {
                RenderTexture.ReleaseTemporary(_pyramid[i].down);
                _pyramid[i].down = null;
            }

            if (_pyramid[i].up != null) {
                RenderTexture.ReleaseTemporary(_pyramid[i].up);
                _pyramid[i].up = null;
            }
        }
    }
}
