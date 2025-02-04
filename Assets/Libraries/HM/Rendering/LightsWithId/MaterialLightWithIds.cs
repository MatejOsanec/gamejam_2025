using UnityEngine;

public class MaterialLightWithIds : RuntimeLightWithIds {

    [SerializeField] MeshRenderer _meshRenderer = default;
    [SerializeField] bool _setAlphaOnly = false;
    [SerializeField] [DrawIf("_setAlphaOnly", false)] bool _alphaIntoColor = false;
    [SerializeField] [DrawIf("_setAlphaOnly", false)] bool _setColorOnly = false;
    [SerializeField] string _colorProperty = "_Color";

#if UNITY_EDITOR
    [SerializeField] [DrawIf("_setAlphaOnly", false)] bool _useTestColor = false;
    [SerializeField] [DrawIf("_useTestColor", true)] [ColorUsage(showAlpha:true, hdr:true)] Color _testColor = Color.white;
#endif

    [DoesNotRequireDomainReloadInit]
    private static MaterialPropertyBlock _materialPropertyBlock = default;

    private Color _color;
    private float _alpha;
    private int _propertyId;

    protected override void Awake() {

        base.Awake();

        _propertyId = Shader.PropertyToID(_colorProperty);
        if (_setColorOnly) {
            _alpha = _meshRenderer.sharedMaterial.GetColor(_propertyId).a;
        }
        if (_setAlphaOnly) {
            _color = _meshRenderer.sharedMaterial.GetColor(_propertyId);
        }
    }

    protected override void ColorWasSet(Color color) {

        if (_materialPropertyBlock == null) {
            _materialPropertyBlock = new MaterialPropertyBlock();
        }

        if (_setAlphaOnly) {
            _color.a = color.a;
        }
        else {
            _color = _alphaIntoColor ? new Color(color.a, color.a, color.a) : color;
        }

        if (_setColorOnly) {
            _color.a = _alpha;
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
