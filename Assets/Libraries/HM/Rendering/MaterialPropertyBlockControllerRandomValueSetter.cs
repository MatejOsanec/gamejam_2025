using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialPropertyBlockControllerRandomValueSetter : MonoBehaviour {

    [SerializeField] MaterialPropertyBlockController _materialPropertyBlockController;

    [SerializeField] string _propertyName = default;

    [SerializeField] float _min = 0.0f;

    [SerializeField] float _max = 1000.0f;

    private MaterialPropertyBlock[] _materialPropertyBlocks;

    private int _propertyId;

    protected void Start() {
        RefreshPropertyId();
        ApplyParams();
    }

    protected void OnValidate() {
        RefreshPropertyId();
        ApplyParams();
    }

    private void RefreshPropertyId() {

        _propertyId = Shader.PropertyToID(_propertyName);
    }

    private void ApplyParams() {

        float _offset = UnityEngine.Random.Range(_min, _max);

        if (_materialPropertyBlockController != null) {

            _materialPropertyBlockController.materialPropertyBlock.SetFloat(_propertyId, _offset);

            _materialPropertyBlockController.ApplyChanges();
        }
    }
}
