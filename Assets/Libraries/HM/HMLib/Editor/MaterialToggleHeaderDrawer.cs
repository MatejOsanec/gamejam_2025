using System;
using UnityEngine;
using UnityEditor;

internal class MaterialToggleHeaderDrawer : MaterialPropertyDrawer {

    private readonly string _keyword;
    private readonly string[] _conditions;
    private readonly int _requiredCount;
    private bool _show;

    protected MaterialToggleHeaderDrawer() { }

    public MaterialToggleHeaderDrawer(string keyword) {
        _keyword = keyword;
        _conditions = new string[0];
    }

    public MaterialToggleHeaderDrawer(string keyword, string condition1) {
        _keyword = keyword;
        _conditions = new [] {condition1};
        _requiredCount = 0;
    }

    public MaterialToggleHeaderDrawer(string keyword, string condition1, string condition2) {
        _keyword = keyword;
        _conditions = new [] {condition1, condition2 };
        _requiredCount = 0;
    }

    public MaterialToggleHeaderDrawer(string keyword, string condition1, string condition2, string condition3) {
        _keyword = keyword;
        _conditions = new [] {condition1, condition2, condition3};
        _requiredCount = 0;
    }

    public MaterialToggleHeaderDrawer(string keyword, float requiredCount, string condition1, string condition2) {
        _keyword = keyword;
        _conditions = new [] {condition1, condition2};
        _requiredCount = (int) requiredCount;
    }

    public MaterialToggleHeaderDrawer(string keyword, float requiredCount, string condition1, string condition2, string condition3) {
        _keyword = keyword;
        _conditions = new [] {condition1, condition2, condition3};
        _requiredCount = (int) requiredCount;
    }

    private void SetKeyword(MaterialProperty prop, bool on) {
        SetKeywordInternal(prop, on);
    }

    private void SetKeywordInternal(MaterialProperty prop, bool on) {

        if (string.IsNullOrEmpty(_keyword)) {
            return;
        }

        foreach (Material target in prop.targets) {
            if (on) {
                target.EnableKeyword(_keyword);
            }
            else {
                target.DisableKeyword(_keyword);
            }
        }
    }

    private static bool IsPropertyTypeSuitable(MaterialProperty prop) => prop.type == MaterialProperty.PropType.Float || prop.type == MaterialProperty.PropType.Range;

    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor) {

        if (_show) {
            return base.GetPropertyHeight(prop, label, editor) + 5.0f;
        }

        return -EditorGUIUtility.standardVerticalSpacing;
    }

    public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) {
        if (!IsPropertyTypeSuitable(prop)) {
            return;
        }

        _show = MaterialDrawerConditionsTester.TestConditions(editor.targets, _conditions, _requiredCount);

        if (!_show) {
            return;
        }

        if (_conditions.Length > 0) {
            EditorGUI.indentLevel++;
        }

        label.text = " " + label.text;
        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorGUI.BeginChangeCheck();
        bool flag = (double)Math.Abs(prop.floatValue) > 1.0 / 1000.0;
        EditorGUI.showMixedValue = prop.hasMixedValue;
        bool on = EditorGUI.ToggleLeft(position, label, flag);
        EditorGUI.showMixedValue = false;
        EditorStyles.label.fontStyle = FontStyle.Normal;

        if (_conditions.Length > 0) {
            EditorGUI.indentLevel--;
        }

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
