using UnityEngine;

[ExecuteAlways]
public class MaterialPropertyBlockFloatAnimator : MaterialPropertyBlockAnimator {

    [Space]
    [SerializeField] private float _value = 0.0f;

    protected override void SetProperty() {

        _materialPropertyBlockController.materialPropertyBlock.SetFloat(propertyId, _value);
    }
}

