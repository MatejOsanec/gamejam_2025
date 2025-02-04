using JetBrains.Annotations;
using UnityEngine;
using Zenject;

[ExecuteAlways]
public class MaterialPropertyBlockFloatCurve : MaterialPropertyBlockAnimator {

    [Space]
    [SerializeField] AnimationCurve _curve = default;
    [SerializeField] float _valueMultiplier = 1.0f;
    [SerializeField] float _speedMultiplier = 1.0f;
    
    protected override void SetProperty() {

        _materialPropertyBlockController.materialPropertyBlock.SetFloat(propertyId, _curve.Evaluate((Time.time * _speedMultiplier) % _curve.length) * _valueMultiplier);
    }
}

