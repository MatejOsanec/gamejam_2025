using UnityEngine;

public abstract class BloomPrePassEffectSO : TextureEffectSO, IBloomPrePassParams {

    [SerializeField] int _textureWidth = 512;
    [SerializeField] int _textureHeight = 512;
    [SerializeField] Vector2 _fov = new Vector2(140.0f, 140.0f);
    [SerializeField] float _linesWidth = 0.02f;

    public TextureEffectSO textureEffect => this;
    public int textureWidth => _textureWidth;
    public int textureHeight => _textureHeight;
    public Vector2 fov => _fov;
    public float linesWidth => _linesWidth;

    public virtual ToneMapping toneMapping => ToneMapping.None;
}
