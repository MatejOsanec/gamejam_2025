using UnityEngine;

public class BloomPrePassBackgroundColor : BloomPrePassNonLightPass {

    [SerializeField] float _intensity = 1.0f;
    [SerializeField] float _minAlpha = 0.0f;
    [SerializeField] float _grayscaleFactor = 0.7f;
    
    [Space]
    [SerializeField] Shader _shader = default;
    
    public Color color { get => _color; set => _color = value; }

    private Color bgColor {
        get {
            var color = _color;
            color.a *= _intensity;
            if (color.a < _minAlpha) {
                color.a = _minAlpha;
            }
            var grayscaleColor = new Color();
            var grayscale = color.grayscale;
            grayscaleColor.r = grayscale;
            grayscaleColor.g = grayscale;
            grayscaleColor.b = grayscale;
            grayscaleColor.a = color.a;
            color = Color.Lerp(color, grayscaleColor, _grayscaleFactor); // Desaturate a little bit.
            return color;
        }
    }

    private Color _color;

    [DoesNotRequireDomainReloadInit]
    private static readonly int _colorID = Shader.PropertyToID("_Color");    
    
    [DoesNotRequireDomainReloadInit]
    private static Material _material;
    
    [DoesNotRequireDomainReloadInit]
    private static bool _initialized;

    private void InitIfNeeded() {

        if (_initialized) {
            return;
        }
        _initialized = true;

        if (_material == null) {
            _material = new Material(_shader);
            _material.hideFlags = HideFlags.HideAndDontSave;
        }

        if (_material == null) {
            _initialized = false;
        }
    }
    
    
    public override void Render(RenderTexture dest, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix) {

        InitIfNeeded();
        
        _material.SetColor(_colorID, bgColor);
        Graphics.Blit(null, dest, _material);
    }
}
