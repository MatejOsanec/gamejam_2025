using UnityEngine;
using UnityEngine.Playables;

public class TextureProcessorMixerBehaviour : PlayableBehaviour
{

    private TextureProcessor _trackBinding;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {

        _trackBinding = playerData as TextureProcessor;
        float blendedComputeKernelA = 0f;
        float blendedComputeKernelB = 0f;
        float blendedComputeKernelC = 0f;
        float blendedInputTextureIndexA = 0f;
        float blendedInputTextureIndexB = 0f;
        float blendedInputTextureIndexC = 0f;
        //float blendedAmplitudeA = 0f;
        //float blendedAmplitudeB = 0f;
        //float blendedAmplitudeC = 0f;
        float blendedPhaseA = 0f;
        float blendedPhaseB = 0f;
        float blendedPhaseC = 0f;
        float blendedParam2A = 0f;
        float blendedParam2B = 0f;
        float blendedParam2C = 0f;
        float blendedParam1A = 0f;
        float blendedParam1B = 0f;
        float blendedParam1C = 0f;
        float blendedSpeedA = 0f;
        float blendedSpeedB = 0f;
        float blendedSpeedC = 0f;
        float blendedSpatialScaleA = 0f;
        float blendedSpatialScaleB = 0f;
        float blendedSpatialScaleC = 0f;
        //float blendedRowSize = 0f;
        //float blendedColumnSize = 0f;

        if (!_trackBinding) {
            return;
        }

        int inputCount = playable.GetInputCount ();

        float maxInputWeight = 0.0f;

        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            ScriptPlayable<TextureProcessorBehaviour> inputPlayable = (ScriptPlayable<TextureProcessorBehaviour>)playable.GetInput(i);
            TextureProcessorBehaviour input = inputPlayable.GetBehaviour();

            bool useCurrent = false;
            if (inputWeight > maxInputWeight) {
                useCurrent = true;
                maxInputWeight = inputWeight;
            }

            //blendedAmplitudeA += input.amplitudeA * inputWeight;
            //blendedAmplitudeB += input.amplitudeB * inputWeight;
            //blendedAmplitudeC += input.amplitudeC * inputWeight;
            blendedPhaseA += input.phaseA * inputWeight;
            blendedPhaseB += input.phaseB * inputWeight;
            blendedPhaseC += input.phaseC * inputWeight;
            blendedParam2A += input.param2A * inputWeight;
            blendedParam2B += input.param2B * inputWeight;
            blendedParam2C += input.param2C * inputWeight;
            blendedParam1A += input.param1A * inputWeight;
            blendedParam1B += input.param1B * inputWeight;
            blendedParam1C += input.param1C * inputWeight;
            blendedSpatialScaleA += input.spatialScaleA * inputWeight;
            blendedSpatialScaleB += input.spatialScaleB * inputWeight;
            blendedSpatialScaleC += input.spatialScaleC * inputWeight;

            if (useCurrent) {
                blendedInputTextureIndexA = input.inputTextureIndexA;
                blendedInputTextureIndexB = input.inputTextureIndexB;
                blendedInputTextureIndexC = input.inputTextureIndexC;
                //blendedRowSize = input.rowSize;
                //blendedColumnSize = input.columnSize;
                blendedComputeKernelA = input.computeKernelA;
                blendedComputeKernelB = input.computeKernelB;
                blendedComputeKernelC = input.computeKernelC;
                blendedSpeedA = input.speedA;
                blendedSpeedB = input.speedB;
                blendedSpeedC = input.speedC;
            }
        }

        if (_trackBinding is null) {
            return;
        }
        _trackBinding.computeKernelA = (TextureProcessor.ComputeKernel)Mathf.Floor(blendedComputeKernelA);
        _trackBinding.computeKernelB = (TextureProcessor.ComputeKernel)Mathf.Floor(blendedComputeKernelB);
        _trackBinding.computeKernelC = (TextureProcessor.ComputeKernel)Mathf.Floor(blendedComputeKernelC);
        _trackBinding.inputTextureIndexA = (int)blendedInputTextureIndexA;
        _trackBinding.inputTextureIndexB = (int)blendedInputTextureIndexB;
        _trackBinding.inputTextureIndexC = (int)blendedInputTextureIndexC;
        //_trackBinding.amplitudeA = blendedAmplitudeA;
        //_trackBinding.amplitudeB = blendedAmplitudeB;
        //_trackBinding.amplitudeC = blendedAmplitudeC;
        _trackBinding.phaseA = blendedPhaseA;
        _trackBinding.phaseB = blendedPhaseB;
        _trackBinding.phaseC = blendedPhaseC;
        _trackBinding.param2A = blendedParam2A;
        _trackBinding.param2B = blendedParam2B;
        _trackBinding.param2C = blendedParam2C;
        _trackBinding.param1A = blendedParam1A;
        _trackBinding.param1B = blendedParam1B;
        _trackBinding.param1C = blendedParam1C;
        _trackBinding.speedA = blendedSpeedA;
        _trackBinding.speedB = blendedSpeedB;
        _trackBinding.speedC = blendedSpeedC;
        _trackBinding.spatialScaleA = blendedSpatialScaleA;
        _trackBinding.spatialScaleB = blendedSpatialScaleB;
        _trackBinding.spatialScaleC = blendedSpatialScaleC;
        //_trackBinding.rowSize = (int)blendedRowSize;
        //_trackBinding.columnSize = (int)blendedColumnSize;
    }
}
