using System;
using UnityEngine;

[ExecuteAlways]
public class MaterialPropertyBlockPositionUpdater : MaterialPropertyBlockAnimator {

    [Space]
    [SerializeField] [NullAllowed] Transform _targetTransform = default;

    protected override void SetProperty() {

        if (_targetTransform == null) {
            return;
        }

        _materialPropertyBlockController.materialPropertyBlock.SetVector(propertyId, _targetTransform.position);
    }
}
