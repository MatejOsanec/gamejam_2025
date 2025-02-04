using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BlueNoiseDithering : PersistentScriptableObject {

    [SerializeField] Texture2D _noiseTexture = default;

    [DoesNotRequireDomainReloadInit]
    private static readonly int _noiseParamsID = Shader.PropertyToID("_GlobalBlueNoiseParams");

    [DoesNotRequireDomainReloadInit]
    private static readonly int _globalNoiseTextureID = Shader.PropertyToID("_GlobalBlueNoiseTex");

    public void SetBlueNoiseShaderParams(int cameraPixelWidth, int cameraPixelHeight) {

        Shader.SetGlobalVector(_noiseParamsID, new Vector4(cameraPixelWidth / (float)_noiseTexture.width, cameraPixelHeight / (float)_noiseTexture.height, 0.0f, 0.0f));
        Shader.SetGlobalTexture(_globalNoiseTextureID, _noiseTexture);
    }

#if UNITY_INCLUDE_TESTS
    internal void SetNoiseTexture(Texture2D noiseTexture) {
        _noiseTexture = noiseTexture;
        Shader.SetGlobalTexture(_globalNoiseTextureID, _noiseTexture);
    }
#endif
}
