using Unity.Collections;
using UnityEngine;

public class BloomPrePassBackgroundGradient : BloomPrePassBackgroundTextureGradient {

    [SerializeField] Gradient _gradient = default;

    protected override void UpdatePixels(NativeArray<Color32> pixels, int numberOfPixels) {
        
        for (int i = 0; i < numberOfPixels; i++) {
            pixels[i] = _gradient.Evaluate((float)i / (numberOfPixels - 1));
        }
    }
}
