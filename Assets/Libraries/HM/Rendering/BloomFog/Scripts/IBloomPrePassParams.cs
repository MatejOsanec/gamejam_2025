using UnityEngine;

public interface IBloomPrePassParams {

    TextureEffectSO textureEffect { get; }
    int textureWidth { get; }
    int textureHeight { get; }
    Vector2 fov { get; }
    float linesWidth { get; }
    ToneMapping toneMapping { get; }
}
