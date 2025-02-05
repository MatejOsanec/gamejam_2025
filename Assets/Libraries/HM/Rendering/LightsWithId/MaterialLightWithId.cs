using System;
using UnityEngine;

public class MaterialLightWithId : LightWithIdMonoBehaviour {

    [SerializeField] MeshRenderer _meshRenderer = default;
    [SerializeField] bool _setAlphaOnly = false;
    [SerializeField] [DrawIf("_setAlphaOnly", false)] bool _alphaIntoColor = false;
    [SerializeField] [DrawIf("_setAlphaOnly", false)] bool _setColorOnly = false;
    [SerializeField] string _colorProperty = "_Color";
    [SerializeField] [DrawIf("_setColorOnly", false)] float _alphaIntensity = 1.0f;
    [SerializeField] [DrawIf("_setAlphaOnly", false)] bool _multiplyColorWithAlpha = false;
    [SerializeField] [DrawIf("_setAlphaOnly", false)] bool _multiplyColor = false;
    [SerializeField] [DrawIf("_multiplyColor", true)] float _colorMultiplier = 1.0f;

#if UNITY_EDITOR
    [SerializeField] [DrawIf("_setAlphaOnly", false)] bool _useTestColor = false;
    [SerializeField] [DrawIf("_useTestColor", true)] [ColorUsage(showAlpha:true, hdr:true)] Color _testColor = Color.white;
#endif

    public Color color => _color;

    
    private static MaterialPropertyBlock _materialPropertyBlock = default;

    private Color _color;
    private float _alpha;
    private int _propertyId;

    protected void Awake() {

        _propertyId = Shader.PropertyToID(_colorProperty);
        if (_setColorOnly) {
            _alpha = _meshRenderer.sharedMaterial.GetColor(_propertyId).a;
        }
        if (_setAlphaOnly) {
            _color = _meshRenderer.sharedMaterial.GetColor(_propertyId);
        }
    }

    public override void ColorWasSet(Color color) {

        if (_materialPropertyBlock == null) {
            _materialPropertyBlock = new MaterialPropertyBlock();
        }

        color.a *= _alphaIntensity;

        if (_setAlphaOnly) {
            _color.a = color.a;
        }
        else {
            _color = _alphaIntoColor ? new Color(color.a, color.a, color.a) : color;
        }

        if (_setColorOnly) {
            _color.a = _alpha;
        }

        var colorMultiplier = 1.0f;

        if (_multiplyColorWithAlpha) {
            colorMultiplier *= color.a;
        }

        if (_multiplyColor) {
            colorMultiplier *= _colorMultiplier;
        }

        if (_multiplyColorWithAlpha || _multiplyColor) {
            _color.r *= colorMultiplier;
            _color.g *= colorMultiplier;
            _color.b *= colorMultiplier;
        }

        _materialPropertyBlock.Clear();
        _materialPropertyBlock.SetColor(_propertyId, _color);
        _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
    }

#if UNITY_EDITOR
    protected void OnValidate() {

        if (_useTestColor == false) {
            return;
        }

        ColorWasSet(_testColor);
    }
#endif
}
