using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using System;

public class GaussianSeparableBlurRendererSO : PersistentScriptableObject {

/*
    [SerializeField] Shader _separableBlurShader = default;

    public class WeightsAndOffsets {
        public float[] weights;
        public float[] offsets;
    }

    private Material _separableBlurMaterial;

    private Dictionary<int, WeightsAndOffsets> _weightsAndOffsets;

    public enum KernelSize {
        Kernel15 = 3,
    }

    public void GetBlurWeightsAndOffsets(KernelSize kernelSize, out WeightsAndOffsets weightsAndOffsets) {

        if (_weightsAndOffsets == null) {
            _weightsAndOffsets = new Dictionary<int, WeightsAndOffsets>();
        }

        if (_weightsAndOffsets.TryGetValue((int)kernelSize, out weightsAndOffsets)) {
            return;
        }

        switch (kernelSize) {

            case KernelSize.Kernel15:
                ComputeWeightsAndOffsets(7, out weightsAndOffsets);
                break;
        }
    }

    private int _offsetsID;
    private int _weightsID;

    private Dictionary<string, RenderTexture> _commandBufferRenderTextures;

    protected void OnEnable() {

        _offsetsID = Shader.PropertyToID("_Offsets");
        _weightsID = Shader.PropertyToID("_Weights");

        _separableBlurMaterial = new Material(_separableBlurShader);
        _separableBlurMaterial.hideFlags = HideFlags.HideAndDontSave;
    }

    public CommandBuffer CreateBlurCommandBuffer(string globalTextureName, KernelSize kernelSize, int downsample) {

        return null;

        // if (_commandBufferRenderTextures == null) {
        //     _commandBufferRenderTextures = new Dictionary<string, RenderTexture>();
        // }

        // RenderTexture targetRenderTexture;
        
        // if (!_commandBufferRenderTextures.TryGetValue(globalTextureName, out targetRenderTexture)) {
        //     targetRenderTexture = new RenderTexture()
        // }   

        // WeightsAndOffsets weightsAndOffsets;

        // GetBlurWeightsAndOffsets(kernelSize, out weightsAndOffsets);

        // CommandBuffer buf = new CommandBuffer();

        // Camera currentCamera = Camera.current;

        // int width = currentCamera.stereoEnabled && currentCamera.stereoTargetEye == StereoTargetEyeMask.Both ? currentCamera.pixelWidth * 2 : currentCamera.pixelWidth;
        // int height = currentCamera.pixelHeight;

        // int downsampledWidth = width >> downsample;
        // int downsampledHeight = height >> downsample;

        // int tempTextureID = Shader.PropertyToID("_TempTexture0");
        // buf.GetTemporaryRT(tempTextureID, downsampledWidth, downsampledHeight, 0, FilterMode.Bilinear);

        // if (downsample > 0) {
        //     buf.Blit(BuiltinRenderTextureType.CurrentActive, tempTextureID);
        // }
        // else {
        //     srcTextureID = (int)BuiltinRenderTextureType.CurrentActive;
        // }

        // Vector2 pixelSize = new Vector2(1.0f / downsampledWidth, 1.0f / downsampledHeight);

        // buf.SetGlobalFloatArray(_offsetsID, weightsAndOffsets.offsets);
        // buf.SetGlobalFloatArray(_weightsID, weightsAndOffsets.weights);
        // buf.Blit(srcTextureID, destTextureID, _commandBuffersMaterial, kBlurPassNum);

        // // Separable Blur - Horizontal
        // for (int i = 0; i < weightsAndOffsets.Length; i++) {

        //     // Offset for Kawase blur.
        //     float offset = kernel[i] + 0.5f;
        //     buf.SetGlobalFloat(_boostID, boost);

        //     // Do the Kawase blur step.
        //     buf.Blit(srcTextureID, destTextureID, _commandBuffersMaterial, kBlurPassNum);

        //     int t = destTextureID;
        //     destTextureID = srcTextureID;
        //     srcTextureID = t;
        // }

        // buf.SetGlobalTexture(globalTextureName, destTextureID);
        // buf.ReleaseTemporaryRT(srcTextureID);
        // buf.ReleaseTemporaryRT(destTextureID);

        // return buf;
    }

    public static void ComputeWeightsAndOffsets(int size, out WeightsAndOffsets weightsAndOffsets) {

        weightsAndOffsets = new WeightsAndOffsets();

        if (size % 2 == 0 || size <= 0) {
            return;
        }

        float[] coefs = new float[size * 2 + 1];

        float acc = 0.0f;

        // Compute basic coeficients.
        for (int i = -size; i <= size; i++) {
            float x = Mathf.PI * i / (float)(size);
            float v = (1.0f / Mathf.Sqrt(2 * Mathf.PI)) * Mathf.Exp(-(Mathf.Pow(x, 2)) / 2.0f);
            acc += v;
            coefs[i + size] = v;
        }

        // Normalize coeficients.
        for (int i = -size; i <= size; i++) {
            coefs[i + size] /= acc;
            // Debug.Log(coefs[i + size]);
        }

        // Central coeficients is used two times in the shader, so it needs to be divided by 2.
        coefs[size] *= 0.5f;

        weightsAndOffsets.offsets = new float[(size - 1) / 2];
        weightsAndOffsets.weights = new float[(size - 1) / 2];

        for (int i = 0; i <= (size - 1) / 2; i++) {

            float v0 = coefs[i * 2 + size];
            float v1 = coefs[i * 2 + size + 1];

            float w = v0 + v1;
            float c = v0 / v1;
            float f = (c - 1.0f) / (c + 1.0f);

            weightsAndOffsets.weights[i] = w;
            weightsAndOffsets.offsets[i] = f;

            // Debug.Log(v0 + " " + v1);
            // Debug.Log("weight: " + w + " offset: " + f);
        }
    }
    */
}