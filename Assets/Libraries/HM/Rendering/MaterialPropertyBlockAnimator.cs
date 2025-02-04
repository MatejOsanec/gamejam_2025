using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class MaterialPropertyBlockAnimator : MonoBehaviour {

    [SerializeField] string _property = default;
    [SerializeField] protected MaterialPropertyBlockController _materialPropertyBlockController = default;

    protected int propertyId;
    private bool _isInitialized;

    public MaterialPropertyBlockController materialPropertyBlockController {
        get => _materialPropertyBlockController;
        set {
            _materialPropertyBlockController = value;

            enabled = _materialPropertyBlockController != null;
        }
    }

    protected virtual void SetProperty() { }

    protected void Awake() {

        LazyInit();

        enabled = _materialPropertyBlockController != null;
    }

    protected void Update() {

#if UNITY_EDITOR
        LazyInit();
#endif

        SetProperty();

        _materialPropertyBlockController.ApplyChanges();
    }

    private void LazyInit() {

        if (_isInitialized) {
            return;
        }

        _isInitialized = true;
        propertyId = Shader.PropertyToID(_property);
    }

    [ContextMenu("RefreshPropertyID")]
    private void RefreshProperty() {
        propertyId = Shader.PropertyToID(_property);
    }
}
