using System;
using UnityEngine;
using UnityEditor;

internal class MaterialFloatEnumShowIfDrawer : MaterialPropertyDrawer {
    private readonly GUIContent[] _keywords;
    private readonly string[] _conditions;
    private bool _show;

    public MaterialFloatEnumShowIfDrawer(string kw1, string kw2, string kw3) : this(2.0f, new[] { kw1, kw2, kw3 }) { }
    public MaterialFloatEnumShowIfDrawer(string kw1, string kw2, string kw3, string kw4) : this(3.0f, new[] { kw1, kw2, kw3, kw4 }) { }
    public MaterialFloatEnumShowIfDrawer(string kw1, string kw2, string kw3, string kw4, string kw5) : this(4.0f, new[] { kw1, kw2, kw3, kw4, kw5 }) { }
    public MaterialFloatEnumShowIfDrawer(string kw1, string kw2, string kw3, string kw4, string kw5, string kw6) : this(5.0f, new[] { kw1, kw2, kw3, kw4, kw5, kw6 }) { }
    public MaterialFloatEnumShowIfDrawer(string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7) : this(6.0f, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7 }) { }
    public MaterialFloatEnumShowIfDrawer(string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7, string kw8) : this(7.0f, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8 }) { }
    public MaterialFloatEnumShowIfDrawer(string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7, string kw8, string kw9) : this(8.0f, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8, kw9 }) { }
    public MaterialFloatEnumShowIfDrawer(string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7, string kw8, string kw9, string kw10) : this(9.0f, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8, kw9, kw10 }) { }

    public MaterialFloatEnumShowIfDrawer(float keywordCount, string kw1, string kw2, string kw3) : this(keywordCount, new[] { kw1, kw2, kw3 }) { }
    public MaterialFloatEnumShowIfDrawer(float keywordCount, string kw1, string kw2, string kw3, string kw4) : this(keywordCount, new[] { kw1, kw2, kw3, kw4 }) { }
    public MaterialFloatEnumShowIfDrawer(float keywordCount, string kw1, string kw2, string kw3, string kw4, string kw5) : this(keywordCount, new[] { kw1, kw2, kw3, kw4, kw5 }) { }
    public MaterialFloatEnumShowIfDrawer(float keywordCount, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6) : this(keywordCount, new[] { kw1, kw2, kw3, kw4, kw5, kw6 }) { }
    public MaterialFloatEnumShowIfDrawer(float keywordCount, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7) : this(keywordCount, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7 }) { }
    public MaterialFloatEnumShowIfDrawer(float keywordCount, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7, string kw8) : this(keywordCount, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8 }) { }
    public MaterialFloatEnumShowIfDrawer(float keywordCount, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7, string kw8, string kw9) : this(keywordCount, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8, kw9 }) { }
    public MaterialFloatEnumShowIfDrawer(float keywordCount, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7, string kw8, string kw9, string kw10) : this(keywordCount, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8, kw9, kw10 }) { }

    // IMPORTANT - When changing constructor structure, update MaterialConsistencyChecker

    public MaterialFloatEnumShowIfDrawer(float keywordCount, params string[] keywords) {

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

    // private void SetKeyword(MaterialProperty prop, int index) {
    //
    //     for (int i = 0; i < _keywords.Length; ++i) {
    //         string keywordName = GetKeywordName(prop.name, _keywords[i].text);
    //         foreach (Material target in prop.targets) {
    //             if (index == i) {
    //                 target.EnableKeyword(keywordName);
    //             }
    //             else {
    //                 target.DisableKeyword(keywordName);
    //             }
    //         }
    //     }
    // }

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
        //SetKeyword(prop, index);
    }

    public override void Apply(MaterialProperty prop) {
        base.Apply(prop);
        if (!IsPropertyTypeSuitable(prop) || prop.hasMixedValue) {
            return;
        }
        //SetKeyword(prop, (int)prop.floatValue);
    }

    private static string GetKeywordName(string propName, string name) => (propName + "_" + name).Replace(' ', '_').ToUpperInvariant();
}
