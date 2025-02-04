using System;
using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

public class DirectionalLightWithIds : RuntimeLightWithIds {

    [SerializeField] DirectionalLight _directionalLight = default;
    [SerializeField] bool _setIntensityOnly = false;
    [SerializeField, DrawIf("_setIntensityOnly", true)] Color _defaultColor = Color.black;

#if UNITY_EDITOR
    protected void OnValidate() {

        if (_setIntensityOnly) {
            if (_defaultColor == Color.black && _directionalLight.color != Color.black) {
                _defaultColor = _directionalLight.color;
                EditorUtility.SetDirty(_directionalLight);
            }
            else {
                _directionalLight.color = _defaultColor;
            }
        }

    }
#endif

    protected override void ColorWasSet(Color color) {

        if (_setIntensityOnly) {
            color = _defaultColor.ColorWithValue(color.a);
        }

        _directionalLight.color = color;
    }
}
