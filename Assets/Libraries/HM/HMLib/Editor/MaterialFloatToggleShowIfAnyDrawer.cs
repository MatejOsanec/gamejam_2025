using System;
using UnityEngine;
using UnityEditor;

internal class MaterialFloatToggleShowIfAnyDrawer : MaterialPropertyDrawer {

    private readonly string[] _conditions;
    private bool _show;

    private int _requiredCount;

    //protected MaterialFloatToggleShowIfAnyDrawer() { }

    public MaterialFloatToggleShowIfAnyDrawer() {
        _conditions = new string[0];
    }

    public MaterialFloatToggleShowIfAnyDrawer(string condition1) {
        _conditions = new[] { condition1 };
        _requiredCount = 0;
    }

    public MaterialFloatToggleShowIfAnyDrawer(string condition1, string condition2) {
        _conditions = new[] { condition1, condition2 };
        _requiredCount = 0;
    }

    public MaterialFloatToggleShowIfAnyDrawer(string condition1, string condition2, string condition3) {
        _conditions = new[] { condition1, condition2, condition3 };
        _requiredCount = 0;
    }

    public MaterialFloatToggleShowIfAnyDrawer(string condition1, string condition2, string condition3, string condition4) {
        _conditions = new[] { condition1, condition2, condition3, condition4 };
        _requiredCount = 0;
    }

    public MaterialFloatToggleShowIfAnyDrawer(float requiredCount, string condition1, string condition2) {
        _conditions = new[] { condition1, condition2 };
        _requiredCount = (int) requiredCount;
    }

    public MaterialFloatToggleShowIfAnyDrawer(float requiredCount, string condition1, string condition2, string condition3) {
        _conditions = new[] { condition1, condition2, condition3 };
        _requiredCount = (int) requiredCount;
    }

    public MaterialFloatToggleShowIfAnyDrawer(float requiredCount, string condition1, string condition2, string condition3, string condition4) {
        _conditions = new[] { condition1, condition2, condition3, condition4 };
        _requiredCount = (int) requiredCount;
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

        _show = MaterialDrawerConditionsTester.TestConditions(editor.targets, _conditions, _requiredCount);

        if (!_show) {
            return;
        }

        if (_conditions.Length > 0) {
            EditorGUI.indentLevel++;
        }
        
        EditorGUI.BeginChangeCheck();
        bool flag = (double)Math.Abs(prop.floatValue) > 1.0 / 1000.0;
        EditorGUI.showMixedValue = prop.hasMixedValue;
        bool on = EditorGUI.Toggle(position, label, flag);
        EditorGUI.showMixedValue = false;
        
        if (_conditions.Length > 0) {
            EditorGUI.indentLevel--;
        }

        if (!EditorGUI.EndChangeCheck()) {
            return;
        }

        prop.floatValue = on ? 1f : 0.0f;
    }
    
}
