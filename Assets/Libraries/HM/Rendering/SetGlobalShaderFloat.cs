using System;
using UnityEngine;

[ExecuteAlways]
public class SetGlobalShaderFloat : MonoBehaviour {
    
    [SerializeField] string _propertyName;
    [SerializeField] float _value;

    private int _propertyId;

    private void Start() {

        _propertyId = Shader.PropertyToID(_propertyName);
    }

    private void Update() {

        Shader.SetGlobalFloat(_propertyId, _value);
    }

    private void OnValidate() {

        _propertyId = Shader.PropertyToID(_propertyName);
    }
}

