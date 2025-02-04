#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class MaterialShowIfAllDrawer : MaterialPropertyDrawer {

    private readonly string[] _conditions;
    bool _show;

    public MaterialShowIfAllDrawer(string name1) {
        _conditions = new[] { name1 };
    }

    public MaterialShowIfAllDrawer(string name1, string name2) {
        _conditions = new[] { name1, name2 };
    }

    public MaterialShowIfAllDrawer(string name1, string name2, string name3) {
        _conditions = new[] { name1, name2, name3 };
    }

    public MaterialShowIfAllDrawer(string name1, string name2, string name3, string name4) {
        _conditions = new[] { name1, name2, name3, name4 };
    }

    public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor) {
        _show = false;
        foreach (var target in editor.targets) {

            Material material = target as Material;
            if (material == null) {
                continue;
            }

            foreach (string arg in _conditions) {
                _show &= material.IsKeywordEnabled(arg);
            }
        }

        if (!_show) {
            return;
        }

        EditorGUI.indentLevel++;
        editor.DefaultShaderProperty(prop, label);
        EditorGUI.indentLevel--;
    }

    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor) {
        return -EditorGUIUtility.standardVerticalSpacing;
    }

}
#endif
