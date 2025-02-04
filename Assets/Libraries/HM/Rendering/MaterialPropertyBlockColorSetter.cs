using BGLib.UnityExtension;
using UnityEngine;

public class MaterialPropertyBlockColorSetter : MonoBehaviour {

    [SerializeField] bool _useTestColor = false;
    [SerializeField] [DrawIf("_useTestColor", true)] [ColorUsage(showAlpha:true, hdr:true)] Color _testColor = Color.white;
    [SerializeField] string _property = default;
    [SerializeField] protected MaterialPropertyBlockController _materialPropertyBlockController = default;
    [SerializeField] bool _inverseAlpha = false;
    [SerializeField] bool _multiplyWithAlpha = false;
    [SerializeField] bool _disableOnZeroAlpha = false;

    private int _propertyId;
    private bool _isInitialized;

    public Color color => _materialPropertyBlockController.materialPropertyBlock.GetColor(_propertyId);
    public MaterialPropertyBlockController materialPropertyBlockController { get => _materialPropertyBlockController; set => _materialPropertyBlockController = value; }

    protected void Awake() {

        InitIfNeeded();
    }

    private void InitIfNeeded() {

        if (_isInitialized) {
            return;
        }

        _isInitialized = true;
        _propertyId = Shader.PropertyToID(_property);
    }

    public void SetColor(Color color) {
        
        if (_inverseAlpha) {
            color.a = 1.0f - color.a;
        }

        if (_multiplyWithAlpha) {
            color.r *= color.a;
            color.g *= color.a;
            color.b *= color.a;
        }

        InitIfNeeded();
        _materialPropertyBlockController.materialPropertyBlock.SetColor(_propertyId, color);
        _materialPropertyBlockController.ApplyChanges();

        if (_disableOnZeroAlpha) {
            _materialPropertyBlockController.SetRendererState(color.a > 0.01f);
        }
    }

    protected void OnValidate() {

        if (Application.isPlaying || _useTestColor == false) {
            return;
        }

        SetColor(_testColor);
    }
    
    [Button("Add Necessary Components")]
    public void AddNecessaryComponents() {
        
        if (_materialPropertyBlockController == null && !gameObject.GetComponent<MaterialPropertyBlockController>()) {
            _materialPropertyBlockController = gameObject.AddComponent<MaterialPropertyBlockController>();
        }
        
    }
}
