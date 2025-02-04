using System;
using UnityEngine;
using UnityEditor;

internal class MaterialEnumHeaderDrawer : MaterialPropertyDrawer {
    private readonly GUIContent[] _keywords;

    public MaterialEnumHeaderDrawer(string kw1, string kw2, string kw3) : this(new[] { kw1, kw2, kw3 }) { }

    public MaterialEnumHeaderDrawer(string kw1, string kw2, string kw3, string kw4) : this(new[] { kw1, kw2, kw3, kw4 }) { }

    public MaterialEnumHeaderDrawer(string kw1, string kw2, string kw3, string kw4, string kw5) : this(new[] { kw1, kw2, kw3, kw4, kw5 }) { }

    public MaterialEnumHeaderDrawer(string kw1, string kw2, string kw3, string kw4, string kw5, string kw6) : this(new[] { kw1, kw2, kw3, kw4, kw5, kw6 }) { }

    public MaterialEnumHeaderDrawer(string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7) : this(new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7 }) { }

    public MaterialEnumHeaderDrawer(string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7, string kw8) : this(new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8 }) { }

    public MaterialEnumHeaderDrawer(string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7, string kw8, string kw9) : this(new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8, kw9 }) { }

    public MaterialEnumHeaderDrawer(params string[] keywords) {

        _keywords = new GUIContent[keywords.Length];

        for (int i = 0; i < keywords.Length; ++i) {
            _keywords[i] = new GUIContent(keywords[i]);
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
        return !IsPropertyTypeSuitable(prop) ? 50f : base.GetPropertyHeight(prop, label, editor);
    }

    public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) {

        GUIStyle myStyle = new GUIStyle(EditorStyles.popup) { };

        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = prop.hasMixedValue;
        int floatValue = (int)prop.floatValue;
        EditorGUIUtility.labelWidth = 120;
        int index = EditorGUI.Popup(position, label, floatValue, _keywords, myStyle);
        EditorGUIUtility.labelWidth = 0;
        EditorGUI.showMixedValue = false;
        EditorStyles.label.fontStyle = FontStyle.Normal;

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
