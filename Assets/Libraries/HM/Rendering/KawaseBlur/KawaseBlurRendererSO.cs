using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using System;

public class KawaseBlurRendererSO : PersistentScriptableObject {

    [SerializeField] Shader _kawaseBlurShader = default;
    [SerializeField] Shader _additiveShader = default;
    [SerializeField] Shader _tintShader = default;

    private Material _kawaseBlurMaterial;
    private Material _additiveMaterial;
    private Material _tintMaterial;
    private Material _commandBuffersMaterial;

    private int[][] _kernels;

    public enum KernelSize {
        Kernel7 = 0,
        Kernel15 = 1,
        Kernel23 = 2,
        Kernel35 = 3,
        Kernel63 = 4,
        Kernel127 = 5,
        Kernel135 = 6,
        Kernel143 = 7,
    }

    public int[] GetBlurKernel(KernelSize kernelSize) {

        int[] kernel;

        int idx = (int)kernelSize;

        if (_kernels[idx] != null) {
            return _kernels[idx];
        }

        switch (kernelSize) {
            case KernelSize.Kernel7:
                kernel = new int[] { 0, 0 }; break;
            case KernelSize.Kernel15:
                kernel = new int[] { 0, 1, 1 }; break;
            case KernelSize.Kernel23:
                kernel = new int[] { 0, 1, 1, 2 }; break;
            case KernelSize.Kernel35:
                kernel = new int[] { 0, 1, 2, 2, 3 }; break;
            case KernelSize.Kernel63:
                kernel = new int[] { 0, 1, 2, 3, 4, 4, 5 }; break;
            case KernelSize.Kernel127:
                kernel = new int[] { 0, 1, 2, 3, 4, 5, 7, 8, 9, 10 }; break;
            case KernelSize.Kernel135:
                kernel = new int[] { 0, 1, 2, 3, 4, 5, 7, 8, 9, 10, 11 }; break;
            case KernelSize.Kernel143:
                kernel = new int[] { 0, 1, 2, 3, 4, 5, 7, 8, 9, 10, 11, 12 }; break;
            default:
                kernel = new int[] { 0, 0 }; break;
        }

        _kernels[idx] = kernel;

        return kernel;
    }

    public enum WeightsType {
        None,
        AlphaWeights,
        AlphaAndDepthWeights,
    }

    private class BloomKernel {
        public KernelSize kernelSize;
        public int sharedPartWithNext;
    }
    private BloomKernel[] _bloomKernels;
    private RenderTexture[] _blurTextures;

    private const int kMaxBloomIterations = 5;

    
    private static readonly float[][] kBloomIterationWeights = new float[][] {
        new float[] { 1.0f },
        new float[] { 0.3875f, 0.61125f },
        new float[] { 0.5f, 0.3f, 0.3f },
        new float[] { 0.5f, 0.3f, 0.3f, 0.5f },
        new float[] { 0.5f, 0.3f, 0.3f, 0.3f, 0.5f },
    };

    private enum Pass {
        AlphaWeights = 0,
        Blur = 1,
        BlurAndAdd = 2,
        BlurWithAlphaWeights = 3,
        AlphaAndDepthWeights = 4,
        BlurGamma = 5,
        BlurGammaAndAdd = 6
    }
    
    
    private static readonly int _offsetID = Shader.PropertyToID("_Offset");
    
    
    private static readonly int _boostID = Shader.PropertyToID("_Boost");
    
    
    private static readonly int _additiveAlphaID = Shader.PropertyToID("_AdditiveAlpha");
    
    
    private static readonly int _alphaID = Shader.PropertyToID("_Alpha");
    
    
    private static readonly int _tintColorID = Shader.PropertyToID("_TintColor");
    
    
    private static readonly int _alphaWeightsID = Shader.PropertyToID("_AlphaWeights");
    
    
    private static readonly int _tempTexture0ID = Shader.PropertyToID("_TempTexture0");
    
    
    private static readonly int _tempTexture1ID = Shader.PropertyToID("_TempTexture1");

    protected override void OnEnable() {

        base.OnEnable();

        _kernels = new int[Enum.GetNames(typeof(KernelSize)).Length][];

        _bloomKernels = new BloomKernel[] {
            new BloomKernel { kernelSize = KernelSize.Kernel7, sharedPartWithNext = 1 },
            new BloomKernel { kernelSize = KernelSize.Kernel15, sharedPartWithNext = 2 },
            new BloomKernel { kernelSize = KernelSize.Kernel35, sharedPartWithNext = 3 },
            new BloomKernel { kernelSize = KernelSize.Kernel63, sharedPartWithNext = 5 },
            new BloomKernel { kernelSize = KernelSize.Kernel127, sharedPartWithNext = 9 }
        };

        _blurTextures = new RenderTexture[kMaxBloomIterations];

        _kawaseBlurMaterial = new Material(_kawaseBlurShader);
        _kawaseBlurMaterial.hideFlags = HideFlags.HideAndDontSave;

        _commandBuffersMaterial = new Material(_kawaseBlurShader);
        _commandBuffersMaterial.hideFlags = HideFlags.HideAndDontSave;

        _additiveMaterial = new Material(_additiveShader);
        _additiveMaterial.hideFlags = HideFlags.HideAndDontSave;

        _tintMaterial = new Material(_tintShader);
        _tintMaterial.hideFlags = HideFlags.HideAndDontSave;
    }

    protected void OnDisable() {

        EssentialHelpers.SafeDestroy(_kawaseBlurMaterial);
        EssentialHelpers.SafeDestroy(_commandBuffersMaterial);
        EssentialHelpers.SafeDestroy(_additiveMaterial);
        EssentialHelpers.SafeDestroy(_tintMaterial);
    }

    public void Bloom(RenderTexture src, RenderTexture dest, int iterationsStart, int iterations, float boost, float alphaWeights, WeightsType blurStartWeightsType, float[] bloomIterationWeights) {

        Assert.IsTrue(bloomIterationWeights == null || bloomIterationWeights.Length == iterations);

        iterations = Mathf.Clamp(iterations, 1, Mathf.Min(_bloomKernels.Length, kMaxBloomIterations));

        var texDesc = dest.descriptor;
        texDesc.depthBufferBits = 0;
        texDesc.msaaSamples = 1;

        var rootTexture = src;
        int rootTextureIterationsLength = 0;

        for (int i = iterationsStart; i < iterationsStart + iterations; i++) {

            var bloomKernel = _bloomKernels[i];
            var kernel = GetBlurKernel(bloomKernel.kernelSize);

            var newRootTexture = RenderTexture.GetTemporary(texDesc);

            // Use the root texture to save some work.
            Blur(
                rootTexture,
                newRootTexture,
                kernel,
                boost,
                downsample: 0,
                startIdx: rootTextureIterationsLength,
                length: bloomKernel.sharedPartWithNext - rootTextureIterationsLength,
                alphaWeights: alphaWeights,
                additiveAlpha: 1.0f,
                additivelyBlendToDest: false,
                gammaCorrection: false,
                blurStartWeightsType: i == iterationsStart ? blurStartWeightsType : WeightsType.None
            );

            rootTextureIterationsLength = bloomKernel.sharedPartWithNext;

            _blurTextures[i - iterationsStart] = RenderTexture.GetTemporary(texDesc);
            Blur(
                newRootTexture,
                _blurTextures[i - iterationsStart],
                kernel,
                boost,
                downsample: 0,
                startIdx: bloomKernel.sharedPartWithNext,
                length: kernel.Length - bloomKernel.sharedPartWithNext,
                alphaWeights: 0.0f,
                additiveAlpha: 1.0f,
                additivelyBlendToDest: false,
                gammaCorrection: false,
                blurStartWeightsType: WeightsType.None
            );

            // Prepare root texture for next iteration and release old one if it wasn't src.
            if (i > iterationsStart) {
                RenderTexture.ReleaseTemporary(rootTexture);
            }
            rootTexture = newRootTexture;
        }

        RenderTexture.ReleaseTemporary(rootTexture);

        if (bloomIterationWeights == null) {
            bloomIterationWeights = kBloomIterationWeights[iterations - 1];
        }

        // Blit all the blurred textures into the final one.    
        for (int i = iterationsStart; i < iterationsStart + iterations; i++) {
            float alpha = bloomIterationWeights[i - iterationsStart];
            if (i == iterationsStart) {
                _tintMaterial.SetColor(_tintColorID, new Color(alpha, alpha, alpha, alpha));
                Graphics.Blit(_blurTextures[i - iterationsStart], dest, _tintMaterial);
            }
            else {
                _additiveMaterial.SetFloat(_alphaID, alpha);
                Graphics.Blit(_blurTextures[i - iterationsStart], dest, _additiveMaterial);
            }
            RenderTexture.ReleaseTemporary(_blurTextures[i - iterationsStart]);
        }

    }

    public void DoubleBlur(RenderTexture src, RenderTexture dest, KernelSize kernelSize0, float boost0, KernelSize kernelSize1, float boost1, float secondBlurAlpha, int downsample, bool gammaCorrection) {

        var kernel0 = GetBlurKernel(kernelSize0);
        var kernel1 = GetBlurKernel(kernelSize1);

        // Common kernel
        int sameKernelLength = 0;
        while (sameKernelLength < kernel0.Length && sameKernelLength < kernel1.Length && kernel0[sameKernelLength] == kernel1[sameKernelLength]) {
            sameKernelLength++;
        }

        var downsampledWidth = src.width >> downsample;
        var downsampledHeight = src.height >> downsample;

        var texDesc = src.descriptor;
        texDesc.depthBufferBits = 0;
        texDesc.msaaSamples = 1;
        texDesc.width = downsampledWidth;
        texDesc.height = downsampledHeight;

        var commonKernelTex = RenderTexture.GetTemporary(texDesc);

        Blur(src,
            commonKernelTex,
            kernel0,
            0.0f,
            downsample,
            0,
            sameKernelLength,
            alphaWeights: 0.0f,
            additiveAlpha: 1.0f,
            additivelyBlendToDest: false,
            gammaCorrection: false,
            blurStartWeightsType: WeightsType.None
        );
        Blur(commonKernelTex,
            dest,
            kernel0,
            boost0,
            0,
            sameKernelLength,
            kernel0.Length - sameKernelLength,
            alphaWeights: 0.0f,
            additiveAlpha: 1.0f,
            additivelyBlendToDest: false,
            gammaCorrection: gammaCorrection,
            blurStartWeightsType: WeightsType.None
        );
        Blur(commonKernelTex,
            dest,
            kernel1,
            boost1,
            0,
            sameKernelLength,
            kernel1.Length - sameKernelLength,
            alphaWeights: 0.0f,
            additiveAlpha: secondBlurAlpha,
            additivelyBlendToDest: true,
            gammaCorrection: gammaCorrection,
            blurStartWeightsType: WeightsType.None
        );

        RenderTexture.ReleaseTemporary(commonKernelTex);
    }    

    public Texture2D Blur(Texture src, KernelSize kernelSize, int downsample = 0) {

        var renderTexture = RenderTexture.GetTemporary(src.width >> downsample, src.height >> downsample);
        renderTexture.wrapMode = TextureWrapMode.Clamp;
        Blur(src, renderTexture, kernelSize, boost: 0.0f, downsample: downsample);
        var resultTexture = renderTexture.GetTexture2D();
        RenderTexture.ReleaseTemporary(renderTexture);
        return resultTexture;
    }    

    public void Blur(Texture src, RenderTexture dest, KernelSize kernelSize, float boost, int downsample) {

        var kernel = GetBlurKernel(kernelSize);
        Blur(src, dest, kernel, boost, downsample, 0, kernel.Length, alphaWeights: 0.0f, additiveAlpha: 1.0f, additivelyBlendToDest: false, gammaCorrection: false, blurStartWeightsType: WeightsType.None);
    }

    private void Blur(Texture src, RenderTexture dest, int[] kernel, float boost, int downsample, int startIdx, int length, float alphaWeights, float additiveAlpha, bool additivelyBlendToDest, bool gammaCorrection, WeightsType blurStartWeightsType) {

        if (length == 0) {
            Graphics.Blit(src, dest);
            return;
        }

        var downsampledWidth = src.width >> downsample;
        var downsampledHeight = src.height >> downsample;

        if (downsample == 0) {
            downsampledWidth = dest.width;
            downsampledHeight = dest.height;
        }

        var texDesc = dest.descriptor;
        texDesc.depthBufferBits = 0;
        texDesc.width = downsampledWidth;
        texDesc.height = downsampledHeight;
        texDesc.msaaSamples = 1;

        Texture tempTexture0 = null;

        _kawaseBlurMaterial.SetFloat(_alphaWeightsID, alphaWeights);

        // If starting with alpha and depth weights we need to do first pass outside of the blur cycle.
        if (blurStartWeightsType == WeightsType.AlphaAndDepthWeights) {
            tempTexture0 = RenderTexture.GetTemporary(texDesc);
            Graphics.Blit(src, (RenderTexture)tempTexture0, _kawaseBlurMaterial, (int)Pass.AlphaAndDepthWeights);
        }
        else {
            tempTexture0 = src;
        }

        int endIdx = startIdx + length;

        // Kawase Blur
        for (int i = startIdx; i < endIdx; i++) {

            RenderTexture tempTexture1 = null;

            int passNum = 0;
            if (i == 0 && blurStartWeightsType == WeightsType.AlphaWeights) {
                passNum = (int)Pass.BlurWithAlphaWeights;
            }
            else {
                passNum = (int)Pass.Blur;
            }

            if (i == endIdx - 1) {
                tempTexture1 = dest;
                if (additivelyBlendToDest) {
                    passNum = gammaCorrection ? (int)Pass.BlurGammaAndAdd : (int)Pass.BlurAndAdd;
                }
                else if (gammaCorrection) {
                    passNum = (int)Pass.BlurGamma;
                }
            }
            else {
                tempTexture1 = RenderTexture.GetTemporary(texDesc);
            }

            // Offset for Kawase blur.
            float offset = kernel[i] + 0.5f;
            _kawaseBlurMaterial.SetVector(_offsetID, new Vector4(offset, offset, -offset, offset));
            _kawaseBlurMaterial.SetFloat(_boostID, boost);
            _kawaseBlurMaterial.SetFloat(_additiveAlphaID, additiveAlpha);

            // Do the Kawase blur step.
            Graphics.Blit(tempTexture0, tempTexture1, _kawaseBlurMaterial, passNum);

            if (tempTexture0 != src) {
                RenderTexture.ReleaseTemporary((RenderTexture)tempTexture0);
            }

            tempTexture0 = tempTexture1;
        }
    }

    public void AlphaWeights(RenderTexture src, RenderTexture dest) {

        float offset = 0.5f;
        _kawaseBlurMaterial.SetVector(_offsetID, new Vector4(offset, offset, -offset, offset));
        Graphics.Blit(src, dest, _kawaseBlurMaterial, (int)Pass.AlphaWeights);
    }

    public CommandBuffer CreateBlurCommandBuffer(int width, int height, string globalTextureName, KernelSize kernelSize, float boost) {

        var kernel = GetBlurKernel(kernelSize);

        CommandBuffer buf = new CommandBuffer();

        int tempTexture0ID = _tempTexture0ID;
        int tempTexture1ID = _tempTexture1ID;

        buf.GetTemporaryRT(tempTexture0ID, width: width, height: height, depthBuffer: 0, filter: FilterMode.Bilinear, format: RenderTextureFormat.RGB111110Float);
        buf.GetTemporaryRT(tempTexture1ID, width: width, height: height, depthBuffer: 0, filter: FilterMode.Bilinear, format: RenderTextureFormat.RGB111110Float);

        buf.Blit(BuiltinRenderTextureType.CurrentActive, tempTexture0ID);

        // Kawase Blur
        for (int i = 0; i < kernel.Length; i++) {

            // Offset for Kawase blur.
            float offset = kernel[i] + 0.5f;
            buf.SetGlobalVector(_offsetID, new Vector4(offset, offset, -offset, offset));
            buf.SetGlobalFloat(_boostID, boost);

            buf.Blit(tempTexture0ID, tempTexture1ID, _commandBuffersMaterial, (int)Pass.Blur);
            int t = tempTexture1ID;
            tempTexture1ID = tempTexture0ID;
            tempTexture0ID = t;
        }

        buf.SetGlobalTexture(globalTextureName, tempTexture0ID);
        buf.ReleaseTemporaryRT(tempTexture0ID);
        buf.ReleaseTemporaryRT(tempTexture1ID);

        return buf;
    }
}