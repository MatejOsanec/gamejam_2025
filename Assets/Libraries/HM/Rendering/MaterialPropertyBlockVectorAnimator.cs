using UnityEngine;

[ExecuteAlways]
public class MaterialPropertyBlockVectorAnimator : MaterialPropertyBlockAnimator {

    [Space]
    [SerializeField] Vector4 _vector = Vector4.zero;

    protected override void SetProperty() {

        _materialPropertyBlockController.materialPropertyBlock.SetVector(propertyId, _vector);
    }
}
