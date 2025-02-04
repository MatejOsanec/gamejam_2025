using BGLib.UnityExtension;
using UnityEngine;

public class InstancedMaterialLightWithId : LightWithIdMonoBehaviour {

    [SerializeField] MaterialPropertyBlockColorSetter _materialPropertyBlockColorSetter = default;
    [SerializeField] bool _setColorOnly = false;
    [SerializeField] [DrawIf("_setColorOnly", false)] float _intensity = 1.0f;
    [SerializeField] [DrawIf("_setColorOnly", false)] float _minAlpha = 0.0f;
    [SerializeField] [DrawIf("_setColorOnly", false)] bool _saturateIntensity = false;
    [SerializeField] bool _hdr = false;
    

    public float intensity {
        get => _intensity;
        set {
            _intensity = value;

            ColorWasSet(_originalColor);
        }
    }

    private Color _originalColor;
    private Color _color;
    private bool _startColorWasSet;

    public override void ColorWasSet(Color newColor) {

        if (_startColorWasSet == false) {
            _color = newColor;
            _startColorWasSet = true;
        }

        _originalColor = newColor;
        float newAlpha = newColor.a;
        
        if (_setColorOnly) {
            newAlpha = _color.a;
        }
        else {
            newAlpha = Mathf.Max(_minAlpha, newAlpha) * _intensity;
            if (_saturateIntensity) {
                newAlpha = Mathf.Clamp01(newAlpha);
            }
        }
        
        _color = newColor.ColorWithAlpha(newAlpha);
        
        if (_hdr) {
            _color *= _intensity;
        }
        _materialPropertyBlockColorSetter.SetColor(_color);
    }
    
    [Button("Add Necessary Components")]
    public void AddNecessaryComponents() {
        
        if (_materialPropertyBlockColorSetter == null && !gameObject.GetComponent<MaterialPropertyBlockColorSetter>()) {
            _materialPropertyBlockColorSetter = gameObject.AddComponent<MaterialPropertyBlockColorSetter>();
        }
        
        _materialPropertyBlockColorSetter.AddNecessaryComponents();
        
    }
}
