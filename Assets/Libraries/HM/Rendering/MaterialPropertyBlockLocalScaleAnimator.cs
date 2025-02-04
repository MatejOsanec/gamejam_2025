using UnityEngine;

[ExecuteAlways]
public class MaterialPropertyBlockLocalScaleAnimator : MaterialPropertyBlockAnimator {

    [Space]
    [SerializeField] Transform _targetTransform = default;

    protected override void SetProperty() {
        
        _materialPropertyBlockController.materialPropertyBlock.SetVector(propertyId, _targetTransform.localScale);
    }
}
