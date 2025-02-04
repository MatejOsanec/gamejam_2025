using System;
using UnityEngine;
using UnityEditor;

internal class MaterialToggleShowIfAllDrawer : MaterialPropertyDrawer {

    private readonly string _keyword;
    private readonly string[] _conditions;
    private bool _show;

    protected MaterialToggleShowIfAllDrawer() { }

    public MaterialToggleShowIfAllDrawer(string keyword) {
        _keyword = keyword;
        _conditions = new string[0];
    }

    public MaterialToggleShowIfAllDrawer(string keyword, string condition1) {
        _keyword = keyword;
        _conditions = new[] { condition1 };
    }

    public MaterialToggleShowIfAllDrawer(string keyword, string condition1, string condition2) {
        _keyword = keyword;
        _conditions = new[] { condition1, condition2 };
    }

    public MaterialToggleShowIfAllDrawer(string keyword, string condition1, string condition2, string condition3) {
        _keyword = keyword;
        _conditions = new[] { condition1, condition2, condition3 };
    }

    public MaterialToggleShowIfAllDrawer(string keyword, string condition1, string condition2, string condition3, string condition4) {
        _keyword = keyword;
        _conditions = new[] { condition1, condition2, condition3, condition4 };
    }

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

        _show = false;
        foreach (var target in editor.targets) {

            Material material = target as Material;
            if (material == null) {
                continue;
            }

            if (_conditions.Length == 0) {
                _show = true;
            }
            else {
                foreach (string arg in _conditions) {
                    _show &= material.IsKeywordEnabled(arg);
                }
            }
        }

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
