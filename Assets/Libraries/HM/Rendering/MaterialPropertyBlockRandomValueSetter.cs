using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialPropertyBlockRandomValueSetter : MonoBehaviour {

    [SerializeField] Renderer[] _renderers = default;

    [SerializeField] string _propertyName = default;
    [SerializeField] float _minValue = 0.0f;
    [SerializeField] float _maxValue = 1.0f;

    private MaterialPropertyBlock[] _materialPropertyBlocks;

    private int _propertyId;

    protected void Start() {

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

        if (_materialPropertyBlocks == null || _materialPropertyBlocks.Length != _renderers.Length) {
            _materialPropertyBlocks = new MaterialPropertyBlock[_renderers.Length];
        }

        for (int i = 0; i < _renderers.Length; i++) {

            if (_materialPropertyBlocks[i] == null) {
                _materialPropertyBlocks[i] = new MaterialPropertyBlock();
            }

            _materialPropertyBlocks[i].SetFloat(_propertyId, UnityEngine.Random.Range(_minValue, _maxValue));

            _renderers[i].SetPropertyBlock(_materialPropertyBlocks[i]);
        }
    }
}
