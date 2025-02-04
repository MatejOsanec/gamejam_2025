using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialPropertyValuesSetter : MonoBehaviour {

    [SerializeField] MaterialPropertyBlockController _materialPropertyBlockController = default;

    [SerializeField] PropertyNameFloatValuePair[] _floats = default;
    [SerializeField] PropertyNameVectorValuePair[] _vectors = default;
    [SerializeField] PropertyNameColorValuePair[] _colors = default;
    [SerializeField] PropertyNameIntValuePair[] _ints = default;

    [System.Serializable]
    public class PropertyValuePairBase {

        [SerializeField] string _propertyName = default;
        public int propertyId { get; private set; }

        public PropertyValuePairBase() {
            RefreshPropertyId();
        }

        public void RefreshPropertyId() {

            propertyId = Shader.PropertyToID(_propertyName);
        }
    }

    [System.Serializable]
    public class PropertyNameFloatValuePair : PropertyValuePairBase {
        public float value;
    }

    [System.Serializable]
    public class PropertyNameIntValuePair : PropertyValuePairBase {
        public int value;
    }

    [System.Serializable]
    public class PropertyNameVectorValuePair : PropertyValuePairBase {
        public Vector4 vector;
    }

    [System.Serializable]
    public class PropertyNameColorValuePair : PropertyValuePairBase {
        public Color color;
    }

    protected void Start() {

        RefreshPropertyIds();
        ApplyParams();
    }

    protected void OnValidate() {

        if (_materialPropertyBlockController == null) {
            _materialPropertyBlockController = GetComponent<MaterialPropertyBlockController>();
        }
        RefreshPropertyIds();
        ApplyParams();
    }

    private void RefreshPropertyIds() {

        if (_floats != null) {
            foreach (var nameValuePair in _floats) {
                nameValuePair.RefreshPropertyId();
            }
        }

        if (_vectors != null) {
            foreach (var nameValuePair in _vectors) {
                nameValuePair.RefreshPropertyId();
            }
        }

        if (_colors != null) {
            foreach (var nameValuePair in _colors) {
                nameValuePair.RefreshPropertyId();
            }
        }

        if (_ints != null) {
            foreach (var nameValuePair in _ints) {
                nameValuePair.RefreshPropertyId();
            }
        }

    }

    private void ApplyParams() {

#if UNITY_EDITOR
        if (_materialPropertyBlockController == null) {
            return;
        }
#endif

        if (_floats != null) {
            foreach (var nameValuePair in _floats) {
                _materialPropertyBlockController.materialPropertyBlock.SetFloat(nameValuePair.propertyId, nameValuePair.value);
            }
        }

        if (_vectors != null) {
            foreach (var nameValuePair in _vectors) {
                _materialPropertyBlockController.materialPropertyBlock.SetVector(nameValuePair.propertyId, nameValuePair.vector);
            }
        }

        if (_colors != null) {
            foreach (var nameValuePair in _colors) {
                _materialPropertyBlockController.materialPropertyBlock.SetVector(nameValuePair.propertyId, nameValuePair.color);
            }
        }

        if (_ints != null) {
            foreach (var nameValuePair in _ints) {
                _materialPropertyBlockController.materialPropertyBlock.SetInt(nameValuePair.propertyId, nameValuePair.value);
            }
        }

        _materialPropertyBlockController.ApplyChanges();
    }
}
