using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

[System.Serializable]
public class TextureProcessorBehaviour : PlayableBehaviour {

    public float computeKernelA;
    public float computeKernelB;
    public float computeKernelC;
    public float inputTextureIndexA;
    public float inputTextureIndexB;
    public float inputTextureIndexC;
    //public float amplitudeA;
    //public float amplitudeB;
    //public float amplitudeC;
    public float phaseA;
    public float phaseB;
    public float phaseC;
    public float param2A;
    public float param2B;
    public float param2C;
    public float param1A;
    public float param1B;
    public float param1C;
    public float speedA;
    public float speedB;
    public float speedC;
    public float spatialScaleA;
    public float spatialScaleB;
    public float spatialScaleC;
    //public float rowSize;
    //public float columnSize;

    private bool _initialized;
    private float _originalComputeKernelA;
    private float _originalComputeKernelB;
    private float _originalComputeKernelC;
    private float _originalInputTextureIndexA;
    private float _originalInputTextureIndexB;
    private float _originalInputTextureIndexC;
    //private float _amplitudeA;
    //private float _amplitudeB;
    //private float _amplitudeC;
    private float _phaseA;
    private float _phaseB;
    private float _phaseC;
    private float _param2A;
    private float _param2B;
    private float _param2C;
    private float _param1A;
    private float _param1B;
    private float _param1C;
    private float _originalSpeedA;
    private float _originalSpeedB;
    private float _originalSpeedC;
    private float _originalSpatialScaleA;
    private float _originalSpatialScaleB;
    private float _originalSpatialScaleC;
    //private float _originalRowSize;
    //private float _originalColumnSize;

    private TextureProcessor _textureProcessor;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData) {

        var time = playable.GetTime();
        if (time <= 0.0f) {
            return;
        }

        _textureProcessor = playerData as TextureProcessor;

        if (_textureProcessor == null) {
            return;
        }

        ScriptPlayable<TextureProcessorBehaviour> inputPlayable = (ScriptPlayable<TextureProcessorBehaviour>)playable;
        inputPlayable.GetBehaviour();

        if (!_initialized) {
            _originalComputeKernelA = (float)_textureProcessor.computeKernelA;
            _originalComputeKernelB = (float)_textureProcessor.computeKernelB;
            _originalComputeKernelC = (float)_textureProcessor.computeKernelC;
            _originalInputTextureIndexA = _textureProcessor.inputTextureIndexA;
            _originalInputTextureIndexB = _textureProcessor.inputTextureIndexB;
            _originalInputTextureIndexC = _textureProcessor.inputTextureIndexC;
            //_amplitudeA = _textureProcessor.amplitudeA;
            //_amplitudeB = _textureProcessor.amplitudeB;
            //_amplitudeC = _textureProcessor.amplitudeC;
            _phaseA = _textureProcessor.phaseA;
            _phaseB = _textureProcessor.phaseB;
            _phaseC = _textureProcessor.phaseC;
            _param2A = _textureProcessor.param2A;
            _param2B = _textureProcessor.param2B;
            _param2C = _textureProcessor.param2C;
            _param1A = _textureProcessor.param1A;
            _param1B = _textureProcessor.param1B;
            _param1C = _textureProcessor.param1C;
            _originalSpeedA = _textureProcessor.speedA;
            _originalSpeedB = _textureProcessor.speedB;
            _originalSpeedC = _textureProcessor.speedC;
            _originalSpatialScaleA = _textureProcessor.spatialScaleA;
            _originalSpatialScaleB = _textureProcessor.spatialScaleB;
            _originalSpatialScaleC = _textureProcessor.spatialScaleC;
            //_originalRowSize = _textureProcessor.rowSize;
            //_originalColumnSize = _textureProcessor.columnSize;
            _initialized = true;
        }
    }

    public override void OnPlayableDestroy(Playable playable) {

        if (!_initialized || _textureProcessor != null) {
            return;
        }
        _textureProcessor.computeKernelA = (TextureProcessor.ComputeKernel)Mathf.Floor(_originalComputeKernelA);
        _textureProcessor.computeKernelB = (TextureProcessor.ComputeKernel)Mathf.Floor(_originalComputeKernelB);
        _textureProcessor.computeKernelC = (TextureProcessor.ComputeKernel)Mathf.Floor(_originalComputeKernelC);
        _textureProcessor.inputTextureIndexA = (int)_originalInputTextureIndexA;
        _textureProcessor.inputTextureIndexB = (int)_originalInputTextureIndexB;
        _textureProcessor.inputTextureIndexC = (int)_originalInputTextureIndexC;
        //_textureProcessor.amplitudeA = _amplitudeA;
        //_textureProcessor.amplitudeB = _amplitudeB;
        //_textureProcessor.amplitudeC = _amplitudeC;
        _textureProcessor.phaseA = _phaseA;
        _textureProcessor.phaseB = _phaseB;
        _textureProcessor.phaseC = _phaseC;
        _textureProcessor.param2A = _param2A;
        _textureProcessor.param2B = _param2B;
        _textureProcessor.param2C = _param2C;
        _textureProcessor.param1A = _param1A;
        _textureProcessor.param1B = _param1B;
        _textureProcessor.param1C = _param1C;
        _textureProcessor.speedA = _originalSpeedA;
        _textureProcessor.speedB = _originalSpeedB;
        _textureProcessor.speedC = _originalSpeedC;
        _textureProcessor.spatialScaleA = _originalSpatialScaleA;
        _textureProcessor.spatialScaleB = _originalSpatialScaleB;
        _textureProcessor.spatialScaleC = _originalSpatialScaleC;
        //_textureProcessor.rowSize = (int)_originalRowSize;
        //_textureProcessor.columnSize = (int)_originalColumnSize;
    }
}
