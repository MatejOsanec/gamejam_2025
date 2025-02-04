using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class TextureProcessor : MonoBehaviour {

    [Header("General Setup")]
    [SerializeField] Material[] _testCubeMaterials;
    [SerializeField] ComputeShader _textureGenCompute;
    [SerializeField] ComputeShader _writeTexturesCompute;
    [SerializeField] Texture2D[] _inputTextures;
    [SerializeField] Texture2D[] _variantTextures;

    [Header("Grid Size/Spacing")]
    [SerializeField] int _rowSize;
    [SerializeField] int _columnSize;

    [Header("Channel A")]
    [SerializeField] ComputeKernel _computeKernelA;
    [SerializeField] int _inputTextureIndexA;
    [SerializeField] [Range(-1.0f, 1.0f)] float _amplitudeA;
    [SerializeField] [Range(0.0f, 1.0f)] float _phaseA;
    [SerializeField] [Range(0.0f, 1.0f)] float _param2A;
    [SerializeField] [Range(-1.0f, 1.0f)] float _param1A;
    [SerializeField] [Range(-10.0f, 10.0f)] float _speedA = 1.0f;
    [SerializeField] [Range(-10.0f, 10.0f)] float _spatialScaleA = 1.0f;

    [Header("Channel B")]
    [SerializeField] ComputeKernel _computeKernelB;
    [SerializeField] int _inputTextureIndexB;
    [SerializeField] [Range(-1.0f, 1.0f)] float _amplitudeB;
    [SerializeField] [Range(0.0f, 1.0f)] float _phaseB;
    [SerializeField] [Range(0.0f, 1.0f)] float _param2B;
    [SerializeField] [Range(-1.0f, 1.0f)] float _param1B;
    [SerializeField] [Range(-10.0f, 10.0f)] float _speedB = 1.0f;
    [SerializeField] [Range(-10.0f, 10.0f)] float _spatialScaleB = 1.0f;

    [Header("Channel C")]
    [SerializeField] ComputeKernel _computeKernelC;
    [SerializeField] int _inputTextureIndexC;
    [SerializeField] [Range(-1.0f, 1.0f)] float _amplitudeC;
    [SerializeField] [Range(0.0f, 1.0f)] float _phaseC;
    [SerializeField] [Range(0.0f, 1.0f)] float _param2C;
    [SerializeField] [Range(-1.0f, 1.0f)] float _param1C;
    [SerializeField] [Range(-10.0f, 10.0f)] float _speedC = 1.0f;
    [SerializeField] [Range(-10.0f, 10.0f)] float _spatialScaleC = 1.0f;

    [Header("Debugging")]
    [SerializeField] [NullAllowed] Material _generatedTextureDebugMaterialA;
    [SerializeField] [NullAllowed] Material _generatedTextureDebugMaterialB;
    [SerializeField] [NullAllowed] Material _generatedTextureDebugMaterialC;
    [SerializeField] [NullAllowed] Material _generatedTextureDebugMaterialOut;

    public enum ComputeKernel {
        Neutral,
        Texture,
        WaveU,
        WaveV,
        Ripple,
        TextureScroll,
        LinearRampShift,
        InvLinearRampShift,
        SmoothedRampShift,
        InvSmoothedRampShift,
        WaveRipple,
        RandomEjectEffect
    }

    public enum DispatchMode {
        NoInterleave,
        TextureWrites_BufferWrite,
        Texture1Write_Texture2WriteBufferWrite,
        Texture1Write_Texture2Write_BufferWrite
    }

    public DispatchMode dispatchMode { get => _dispatchMode; set => SetDispatchMode(value); }

    public ComputeKernel computeKernelA { get => _computeKernelA; set => _computeKernelA = value; }

    public ComputeKernel computeKernelB { get => _computeKernelB; set => _computeKernelB = value; }

    public ComputeKernel computeKernelC { get => _computeKernelC; set => _computeKernelC = value; }

    public int inputTextureIndexA { get => _inputTextureIndexA; set => _inputTextureIndexA = value; }
    public int inputTextureIndexB { get => _inputTextureIndexB; set => _inputTextureIndexB = value; }
    public int inputTextureIndexC { get => _inputTextureIndexC; set => _inputTextureIndexC = value; }

    public float amplitudeA { get => _amplitudeA; set => _amplitudeA = value; }

    public float amplitudeB { get => _amplitudeB; set => _amplitudeB = value; }

    public float amplitudeC { get => _amplitudeC; set => _amplitudeC = value; }

    public float phaseA { get => _phaseA; set => _phaseA = value; }

    public float phaseB { get => _phaseB; set => _phaseB = value; }

    public float phaseC { get => _phaseC; set => _phaseC = value; }

    public float param2A { get => _param2A; set => _param2A = value; }

    public float param2B { get => _param2B; set => _param2B = value; }

    public float param2C { get => _param2C; set => _param2C = value; }

    public float param1A { get => _param1A; set => _param1A = value; }
    
    public float param1B { get => _param1B; set => _param1B = value; }

    public float param1C { get => _param1C; set => _param1C = value; }

    public float speedA { get => _speedA; set => _speedA = value; }

    public float speedB { get => _speedB; set => _speedB = value; }

    public float speedC { get => _speedC; set => _speedC = value; }

    public float spatialScaleA { get => _spatialScaleA; set => _spatialScaleA = value; }
    public float spatialScaleB { get => _spatialScaleB; set => _spatialScaleB = value; }
    public float spatialScaleC { get => _spatialScaleC; set => _spatialScaleC = value; }

    public int rowSize {
        get => _rowSize;
        set
        {
               _rowSize = value;
        }
    }

    public int columnSize {
        get => _columnSize;
        set
        {
            _columnSize = value;
        }
    }


    private DispatchMode _dispatchMode = DispatchMode.NoInterleave;
    private int _dispatchStepCounter;
    private int _numberOfDispatchSteps = 2;

    private int _cachedInstanceCount = -1;
    private Matrix4x4[] _matrices;
    private int _numInstances;
    private RenderTexture _animationTextureA;
    private RenderTexture _animationTextureB;
    private RenderTexture _animationTextureC;
    private RenderTexture _animationTextureOut;
    private int _textureArrayLength;
    private int _variantTextureArrayLength;
    private int _ejectEffectKernelCount;
    private int _testMaterialArrayCount;
    private int _textureVariantRandomIndex = 0;
    private int _ejectEffectKernelRandomIndex = 0;

    [DoesNotRequireDomainReloadInit]
    private static readonly string[] _kernelStrings = {
        "Neutral",
        "Texture",
        "WaveU",
        "WaveV",
        "Ripple",
        "TextureScroll",
        "LinearRampShift",
        "InvLinearRampShift",
        "SmoothedRampShift",
        "InvSmoothedRampShift",
        "WaveRipple",
        "RandomEjectEffect",
        "EjectEffect_SpiralMaskRipple"
    };

    [DoesNotRequireDomainReloadInit]
    private static readonly string[] _ejectEffectKernelStrings = {
        "EjectEffect_WaveRipple",
        "EjectEffect_RectRipple",
        "EjectEffect_TextureMaskRipple",
        "EjectEffect_NoiseMaskRipple",
        "EjectEffect_RotationMaskRipple",
        "EjectEffect_SpiralMaskRipple",
        "EjectEffect_MixedRipple"
    };

    private const int kRandomEjectEffectIndex = 11;

    protected void Awake() {

        UnityEngine.Random.InitState(DateTime.Now.Millisecond);
        _variantTextureArrayLength = _variantTextures.Length;
        _ejectEffectKernelCount = _ejectEffectKernelStrings.Length;
        _textureVariantRandomIndex = UnityEngine.Random.Range(0, _variantTextureArrayLength);
        _numInstances = _columnSize * _rowSize;
        UpdateBuffers();
    }

    protected void Update() {

        _dispatchStepCounter = (_dispatchStepCounter + 1) % _numberOfDispatchSteps;

        _numInstances = _columnSize * _rowSize;
        AnimateTextures();

    }

    protected void OnEnable() {

        UpdateBuffers();
    }

    protected void OnValidate() {

        UpdateBuffers();
    }

    private RenderTexture CreateTexture(int sizeX, int sizeY) {

        var texture = new RenderTexture(sizeX, sizeY, 32) { format = RenderTextureFormat.ARGBFloat, enableRandomWrite = true, filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
        texture.Create();

        return texture;
    }


    private void SetDispatchMode(DispatchMode dispatchMode) {

        _dispatchMode = dispatchMode;
        switch (dispatchMode) {
            case DispatchMode.NoInterleave:
                _numberOfDispatchSteps = 1;
                break;
            case DispatchMode.TextureWrites_BufferWrite:
                _numberOfDispatchSteps = 2;
                break;
            case DispatchMode.Texture1Write_Texture2WriteBufferWrite:
                _numberOfDispatchSteps = 2;
                break;
            case DispatchMode.Texture1Write_Texture2Write_BufferWrite:
                _numberOfDispatchSteps = 3;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dispatchMode), dispatchMode, null);
        }
    }


    private void ReleaseTextures() {

        if (_animationTextureA) {
            _animationTextureA.Release();
            _animationTextureA = null;
        }

        if (_animationTextureB) {
            _animationTextureB.Release();
            _animationTextureB = null;
        }

        if (_animationTextureC) {
            _animationTextureC.Release();
            _animationTextureC = null;
        }

        if (_animationTextureOut){
            _animationTextureOut.Release();
            _animationTextureOut = null;
        }
    }

    private void UpdateBuffers() {

        if (_rowSize <= 0 || _columnSize <= 0) {
            return;
        }

        _numInstances = _columnSize * _rowSize;

        _animationTextureA = CreateTexture(_rowSize, _columnSize);
        _animationTextureB = CreateTexture(_rowSize, _columnSize);
        _animationTextureC = CreateTexture(_rowSize, _columnSize);
        _animationTextureOut = CreateTexture(_rowSize, _columnSize);

        _generatedTextureDebugMaterialA.SetTexture("_VertexDisplacementMask", _animationTextureA);
        _generatedTextureDebugMaterialA.SetTexture("_SecondaryEmissionMask", _animationTextureA);

        _generatedTextureDebugMaterialB.SetTexture("_VertexDisplacementMask", _animationTextureB);
        _generatedTextureDebugMaterialB.SetTexture("_SecondaryEmissionMask", _animationTextureB);

        _generatedTextureDebugMaterialC.SetTexture("_VertexDisplacementMask", _animationTextureC);
        _generatedTextureDebugMaterialC.SetTexture("_SecondaryEmissionMask", _animationTextureC);

        _generatedTextureDebugMaterialOut.SetTexture("_VertexDisplacementMask", _animationTextureOut);
        _generatedTextureDebugMaterialOut.SetTexture("_SecondaryEmissionMask", _animationTextureOut);

        _cachedInstanceCount = _numInstances;
    }

    private void AnimateTextures() {

        _textureArrayLength = _inputTextures.Length;
        if (_rowSize <= 0 || _columnSize <= 0 || _textureArrayLength == 0) {
            return;
        }

        var currentKernelName = GetKernelName(_computeKernelA);
        var currentKernel = _textureGenCompute.FindKernel(currentKernelName);

        if (_dispatchStepCounter == 0) {
            _textureGenCompute.SetInt("_columnSize", _columnSize);
            _textureGenCompute.SetInt("_rowSize", _rowSize);
            _textureGenCompute.SetFloat("_amplitude", _amplitudeA);
            _textureGenCompute.SetFloat("_phase", _phaseA);
            _textureGenCompute.SetFloat("_param2", _param2A);
            _textureGenCompute.SetFloat("_param1", _param1A);
            _textureGenCompute.SetFloat("_speed", _speedA);
            _textureGenCompute.SetFloat("_spatialScale", _spatialScaleA);
            _textureGenCompute.SetTexture(currentKernel, "_inputTexture", _inputTextureIndexA == 0 ? _variantTextures[_textureVariantRandomIndex % _textureArrayLength] : _inputTextures[_inputTextureIndexA % _textureArrayLength]);
            _textureGenCompute.SetTexture(currentKernel, "_outputTexture", _animationTextureA);
            _textureGenCompute.Dispatch(currentKernel, _rowSize, _columnSize, 1);
        }

        if ((_dispatchMode == DispatchMode.NoInterleave || _dispatchMode == DispatchMode.TextureWrites_BufferWrite) && _dispatchStepCounter == 0 ||
            (_dispatchMode == DispatchMode.Texture1Write_Texture2WriteBufferWrite || _dispatchMode == DispatchMode.Texture1Write_Texture2Write_BufferWrite) && _dispatchStepCounter == 1) {

            currentKernelName = GetKernelName(_computeKernelB);
            currentKernel = _textureGenCompute.FindKernel(currentKernelName);
            _textureGenCompute.SetInt("_columnSize", _columnSize);
            _textureGenCompute.SetInt("_rowSize", _rowSize);
            _textureGenCompute.SetFloat("_amplitude", _amplitudeB);
            _textureGenCompute.SetFloat("_phase", _phaseB);
            _textureGenCompute.SetFloat("_param2", _param2B);
            _textureGenCompute.SetFloat("_param1", _param1B);
            _textureGenCompute.SetFloat("_speed", _speedB);
            _textureGenCompute.SetFloat("_spatialScale", _spatialScaleB);
            _textureGenCompute.SetTexture(currentKernel, "_inputTexture", _inputTextureIndexB == 0 ? _variantTextures[_textureVariantRandomIndex % _textureArrayLength] : _inputTextures[_inputTextureIndexB % _textureArrayLength]);
            _textureGenCompute.SetTexture(currentKernel, "_outputTexture", _animationTextureB);
            _textureGenCompute.Dispatch(currentKernel, _rowSize, _columnSize, 1);
        }

        if ((_dispatchMode == DispatchMode.NoInterleave || _dispatchMode == DispatchMode.TextureWrites_BufferWrite) && _dispatchStepCounter == 0 ||
            (_dispatchMode == DispatchMode.Texture1Write_Texture2WriteBufferWrite || _dispatchMode == DispatchMode.Texture1Write_Texture2Write_BufferWrite) && _dispatchStepCounter == 1) {
            currentKernelName = GetKernelName(_computeKernelC);
            currentKernel = _textureGenCompute.FindKernel(currentKernelName);
            _textureGenCompute.SetInt("_columnSize", _columnSize);
            _textureGenCompute.SetInt("_rowSize", _rowSize);
            _textureGenCompute.SetFloat("_amplitude", _amplitudeC);
            _textureGenCompute.SetFloat("_phase", _phaseC);
            _textureGenCompute.SetFloat("_param2", _param2C);
            _textureGenCompute.SetFloat("_param1", _param1C);
            _textureGenCompute.SetFloat("_speed", _speedC);
            _textureGenCompute.SetFloat("_spatialScale", _spatialScaleC);
            _textureGenCompute.SetTexture(currentKernel, "_inputTexture", _inputTextureIndexC == 0 ? _variantTextures[_textureVariantRandomIndex % _textureArrayLength] : _inputTextures[_inputTextureIndexC % _textureArrayLength]);
            _textureGenCompute.SetTexture(currentKernel, "_outputTexture", _animationTextureC);
            _textureGenCompute.Dispatch(currentKernel, _rowSize, _columnSize, 1);
        }

        if ((_dispatchMode == DispatchMode.NoInterleave || _dispatchMode == DispatchMode.TextureWrites_BufferWrite) && _dispatchStepCounter == 0 ||
            (_dispatchMode == DispatchMode.Texture1Write_Texture2WriteBufferWrite || _dispatchMode == DispatchMode.Texture1Write_Texture2Write_BufferWrite) && _dispatchStepCounter == 1)
        {
            currentKernel = _writeTexturesCompute.FindKernel("WriteTextures");
            _writeTexturesCompute.SetInt("_columnSize", _columnSize);
            _writeTexturesCompute.SetInt("_rowSize", _rowSize);
            _writeTexturesCompute.SetTexture(currentKernel, "_inputTextureA", _animationTextureA);
            _writeTexturesCompute.SetTexture(currentKernel, "_inputTextureB", _animationTextureB);
            _writeTexturesCompute.SetTexture(currentKernel, "_inputTextureC", _animationTextureC);
            _writeTexturesCompute.SetTexture(currentKernel, "_outputMask", _animationTextureOut);
            _writeTexturesCompute.Dispatch(currentKernel, _rowSize, _columnSize, 1);
        }
    }

    protected void OnDisable() {

        ReleaseTextures();
    }

    protected void OnDestroy() {

        ReleaseTextures();
    }

    public void ModifyGridSize(int rowSizeDelta, int columnSizeDelta) {

        _rowSize = Math.Max(_rowSize + rowSizeDelta, 1);
        _columnSize = Math.Max(_columnSize + columnSizeDelta, 1);
    }


    public void Step() {

        UpdateBuffers();
    }

    public void SetRandomEjectEffect() {

        _ejectEffectKernelRandomIndex = UnityEngine.Random.Range(0, _ejectEffectKernelCount);
    }

    private string GetKernelName(ComputeKernel kernel) {

        var kernelIndex = (int)kernel;
        return kernelIndex != kRandomEjectEffectIndex ? _kernelStrings[(int)kernel] : _ejectEffectKernelStrings[_ejectEffectKernelRandomIndex];
    }
}
