using System;
using UnityEngine;
using UnityEditor;

internal class MaterialToggleShowIfAnyDrawer : MaterialPropertyDrawer {

    protected string _keyword;
    protected string[] _conditions;
    private bool _show;

    protected int _requiredCount;

    protected MaterialToggleShowIfAnyDrawer() { }

    public MaterialToggleShowIfAnyDrawer(string keyword, params string[] conditions) {
        _keyword = keyword;
        _conditions = conditions;
        _requiredCount = 0;
    }
    
    public MaterialToggleShowIfAnyDrawer(string keyword, float requiredCount, params string[] conditions) {
        _keyword = keyword;
        _conditions = conditions;
        _requiredCount = (int) requiredCount;
    }

    // IMPORTANT - Keep first argument as the Toggle Keyword and MaterialConsistencyChecker is okay - otherwise update

    private void SetKeyword(MaterialProperty prop, bool on) {
        SetKeywordInternal(prop, on);
    }

    private void SetKeywordInternal(MaterialProperty prop, bool on) {
        if (!string.IsNullOrEmpty(_keyword)) {
            foreach (Material target in prop.targets) {
                if (on) {
                    target.EnableKeyword(_keyword);
                }
                else {
                    target.DisableKeyword(_keyword);
                }
            }
        }
    }

    private static bool IsPropertyTypeSuitable(MaterialProperty prop) => prop.type == MaterialProperty.PropType.Float || prop.type == MaterialProperty.PropType.Range;

    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor) {
        if (_show) {
            return !IsPropertyTypeSuitable(prop) ? 45f : base.GetPropertyHeight(prop, label, editor);
        }
        return -EditorGUIUtility.standardVerticalSpacing;
    }

    public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) {

        if (!IsPropertyTypeSuitable(prop)) {
            return;
        }

        // Disabling this for shaders with ShaderInspector
        if (editor.customShaderGUI != null) {
            editor.DefaultShaderProperty(prop, label.text);
            return;
        }

        _show = MaterialDrawerConditionsTester.TestConditions(editor.targets, _conditions, _requiredCount);

        if (!_show) {
            return;
        }

        EditorGUI.indentLevel++;
        EditorGUI.BeginChangeCheck();
        bool flag = (double)Math.Abs(prop.floatValue) > 1.0 / 1000.0;
        EditorGUI.showMixedValue = prop.hasMixedValue;
        bool on = EditorGUI.Toggle(position, label, flag);
        EditorGUI.showMixedValue = false;
        EditorGUI.indentLevel--;

        if (!EditorGUI.EndChangeCheck()) {
            return;
        }

        prop.floatValue = on ? 1f : 0.0f;
        SetKeyword(prop, on);
    }

    public override void Apply(MaterialProperty prop) {
        base.Apply(prop);

        if (!IsPropertyTypeSuitable(prop) || prop.hasMixedValue) {
            return;
        }

        SetKeyword(prop, (double)Math.Abs(prop.floatValue) > 1.0 / 1000.0);
    }
}

internal class MaterialFakeToggleShowIfAnyDrawer : MaterialToggleShowIfAnyDrawer {

    public MaterialFakeToggleShowIfAnyDrawer() {
        _keyword = null;
        _conditions = new string[0];
    }

    public MaterialFakeToggleShowIfAnyDrawer(string condition1) {
        _keyword = null;
        _conditions = new[] { condition1 };
        _requiredCount = 0;
    }

    public MaterialFakeToggleShowIfAnyDrawer(string condition1, string condition2) {
        _keyword = null;
        _conditions = new[] { condition1, condition2 };
        _requiredCount = 0;
    }

    public MaterialFakeToggleShowIfAnyDrawer(string condition1, string condition2, string condition3) {
        _keyword = null;
        _conditions = new[] { condition1, condition2, condition3 };
        _requiredCount = 0;
    }

    public MaterialFakeToggleShowIfAnyDrawer(float requiredCount, string condition1, string condition2) {
        _keyword = null;
        _conditions = new[] { condition1, condition2 };
        _requiredCount = (int) requiredCount;
    }

    public MaterialFakeToggleShowIfAnyDrawer(float requiredCount, string condition1, string condition2, string condition3) {
        _keyword = null;
        _conditions = new[] { condition1, condition2, condition3 };
        _requiredCount = (int) requiredCount;
    }

    public MaterialFakeToggleShowIfAnyDrawer(float requiredCount, string condition1, string condition2, string condition3, string condition4) {
        _keyword = null;
        _conditions = new[] { condition1, condition2, condition3, condition4 };
        _requiredCount = (int) requiredCount;
    }

    public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) {
        base.OnGUI(position, prop, label, editor);
    }

}
