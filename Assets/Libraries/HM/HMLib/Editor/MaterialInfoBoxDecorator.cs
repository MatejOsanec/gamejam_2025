using System;
using UnityEngine;
using UnityEditor;

internal class MaterialInfoBoxDecorator : MaterialPropertyDrawer {

    private readonly string _info;
    private readonly string[] _conditions;
    private readonly int _requiredCount;
    private bool _show;

    public MaterialInfoBoxDecorator(string info, params string[] conditions) {

        _info = info;
        _conditions = conditions;
        _requiredCount = 0;
    }

    public MaterialInfoBoxDecorator(string info, float requiredCount, params string[] conditions) {

        _info = info;
        _conditions = conditions;
        _requiredCount = (int) requiredCount;
    }

    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor) {

        // Disabling this for shaders with ShaderInspector
        if (editor.customShaderGUI != null) {
            return 0;
        }

        if (_show) {
            return EditorGUIUtility.standardVerticalSpacing;
        }

        return -EditorGUIUtility.standardVerticalSpacing;
    }

    public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor) {

        // Disabling this for shaders with ShaderInspector
        if (editor.customShaderGUI != null) {
            return;
        }

        _show = MaterialDrawerConditionsTester.TestConditions(editor.targets, _conditions, _requiredCount);

        if (!_show) {
            return;
        }

        if (_info.Contains("Error", StringComparison.CurrentCultureIgnoreCase)) {
            EditorGUILayout.HelpBox(_info, MessageType.Error);
        }
        else if (_info.Contains("Warning", StringComparison.CurrentCultureIgnoreCase)) {
            EditorGUILayout.HelpBox(_info, MessageType.Warning);
        }
        else if (_info.Contains("Info", StringComparison.CurrentCultureIgnoreCase)) {
            EditorGUILayout.HelpBox(_info, MessageType.Info);
        }
        else {
            EditorGUILayout.HelpBox(" " + _info, MessageType.None);
        }

        EditorGUILayout.Space(5.0f);
    }
}
