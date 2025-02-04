using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialPropertyBlockControllerArrayRandomValueSetter : MonoBehaviour {

    [SerializeField] MaterialPropertyBlockController[] _materialPropertyBlockControllers;

    [SerializeField] string _propertyName = default;

    [SerializeField] Vector3 _min;

    [SerializeField] Vector3 _max;

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

        Vector3 _offset = new Vector3(UnityEngine.Random.Range(_min.x, _max.x), UnityEngine.Random.Range(_min.y, _max.y), UnityEngine.Random.Range(_min.z, _max.z));

        foreach (var t in _materialPropertyBlockControllers) {
            if (t != null) {

                t.materialPropertyBlock.SetVector(_propertyId, _offset);

                t.ApplyChanges();
            }
        }
    }
}
