using System;
using UnityEngine;
using UnityEditor;

internal class MaterialEnumShowIfDrawer : MaterialPropertyDrawer {
    private readonly GUIContent[] _keywords;
    private readonly string[] _conditions;
    private bool _show;

    public MaterialEnumShowIfDrawer(params string[] conditions) : this(conditions.Length - 1, conditions) { }
    // IMPORTANT - When changing constructor structure, update MaterialConsistencyChecker

    public MaterialEnumShowIfDrawer(float keywordCount, params string[] keywords) {

        int keywordCountInt = Mathf.RoundToInt(keywordCount);
        _keywords = new GUIContent[keywordCountInt];
        _conditions = new string[keywords.Length - keywordCountInt];

        for (int i = 0, c = 0; i < keywords.Length; ++i) {
            if (i < keywordCountInt) {
                _keywords[i] = new GUIContent(keywords[i]);
            }
            else {
                _conditions[c] = keywords[i];
                c++;
            }
        }

    }

    private static bool IsPropertyTypeSuitable(MaterialProperty prop) => prop.type == MaterialProperty.PropType.Float || prop.type == MaterialProperty.PropType.Range;

    private void SetKeyword(MaterialProperty prop, int index) {

        for (int i = 0; i < _keywords.Length; ++i) {
            string keywordName = GetKeywordName(prop.name, _keywords[i].text);
            foreach (Material target in prop.targets) {
                if (index == i) {
                    target.EnableKeyword(keywordName);
                }
                else {
                    target.DisableKeyword(keywordName);
                }
            }
        }
    }

    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor) {
        if (_show) {
            return !IsPropertyTypeSuitable(prop) ? 45f : base.GetPropertyHeight(prop, label, editor);
        }
        return -EditorGUIUtility.standardVerticalSpacing;
    }

    public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) {

        _show = MaterialDrawerConditionsTester.TestConditions(editor.targets, _conditions, 0);

        if (!_show) {
            return;
        }

        EditorGUI.indentLevel++;
        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = prop.hasMixedValue;
        int floatValue = (int)prop.floatValue;
        int index = EditorGUI.Popup(position, label, floatValue, _keywords);
        EditorGUI.showMixedValue = false;
        EditorGUI.indentLevel--;

        if (!EditorGUI.EndChangeCheck()) {
            return;
        }

        prop.floatValue = (float)index;
        SetKeyword(prop, index);
    }

    public override void Apply(MaterialProperty prop) {
        base.Apply(prop);
        if (!IsPropertyTypeSuitable(prop) || prop.hasMixedValue) {
            return;
        }
        SetKeyword(prop, (int)prop.floatValue);
    }

    private static string GetKeywordName(string propName, string name) => (propName + "_" + name).Replace(' ', '_').ToUpperInvariant();
}
