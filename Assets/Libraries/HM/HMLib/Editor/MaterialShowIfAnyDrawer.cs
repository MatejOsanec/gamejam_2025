using UnityEngine;
using UnityEditor;

public class MaterialShowIfAnyDrawer : MaterialPropertyDrawer {

    private readonly string[] _conditions;
    private bool _show;

    private int _requiredCount;
    private bool _requiredConditionValue;
    private bool _anyConditionValue;


    public MaterialShowIfAnyDrawer(string name1) {
        _conditions = new[] { name1 };
        _requiredCount = 0;
    }

    public MaterialShowIfAnyDrawer(string name1, string name2) {
        _conditions = new[] { name1, name2 };
        _requiredCount = 0;
    }

    public MaterialShowIfAnyDrawer(string name1, string name2, string name3) {
        _conditions = new[] { name1, name2, name3 };
        _requiredCount = 0;
    }

    public MaterialShowIfAnyDrawer(string name1, string name2, string name3, string name4) {
        _conditions = new[] { name1, name2, name3, name4 };
        _requiredCount = 0;
    }

    public MaterialShowIfAnyDrawer(float requiredCount, string name1) {
        _conditions = new[] { name1 };
        _requiredCount = (int) requiredCount;
    }

    public MaterialShowIfAnyDrawer(float requiredCount, string name1, string name2) {
        _conditions = new[] { name1, name2 };
        _requiredCount = (int) requiredCount;
    }

    public MaterialShowIfAnyDrawer(float requiredCount, string name1, string name2, string name3) {
        _conditions = new[] { name1, name2, name3 };
        _requiredCount = (int) requiredCount;
    }

    public MaterialShowIfAnyDrawer(float requiredCount, string name1, string name2, string name3, string name4) {
        _conditions = new[] { name1, name2, name3, name4 };
        _requiredCount = (int) requiredCount;
    }

    public MaterialShowIfAnyDrawer(float requiredCount, string name1, string name2, string name3, string name4, string name5) {
        _conditions = new[] { name1, name2, name3, name4, name5 };
        _requiredCount = (int) requiredCount;
    }

    public MaterialShowIfAnyDrawer(float requiredCount, string name1, string name2, string name3, string name4, string name5, string name6) {
        _conditions = new[] { name1, name2, name3, name4, name5, name6 };
        _requiredCount = (int) requiredCount;
    }


    public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor) {

        // Disabling this for shaders with ShaderInspector
        if (editor.customShaderGUI != null) {
            editor.DefaultShaderProperty(prop, label);
            return;
        }

        _show = MaterialDrawerConditionsTester.TestConditions(editor.targets, _conditions, _requiredCount);

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

public static class MaterialDrawerConditionsTester {

    public static bool TestConditions(Object[] targets, string[] conditions, int requiredCount) {

        bool requiredConditionValue = true;
        bool anyConditionValue = requiredCount >= conditions.Length;

        foreach (var target in targets) {

            Material material = target as Material;
            if (material == null) {
                continue;
            }

            for (int i = 0; i < conditions.Length; i++) {

                if (i < requiredCount) {

                    if (conditions[i].StartsWith("0")) {
                        requiredConditionValue &= !material.IsKeywordEnabled(conditions[i].Substring(1));
                    }
                    else {
                        requiredConditionValue &= material.IsKeywordEnabled(conditions[i]);
                    }

                }
                else {

                    if (conditions[i].StartsWith("0")) {
                        anyConditionValue |= !material.IsKeywordEnabled(conditions[i].Substring(1));
                    }
                    else {
                        anyConditionValue |= material.IsKeywordEnabled(conditions[i]);
                    }

                }
            }
        }

        return requiredConditionValue && anyConditionValue;
    }

}

// internal class MaterialShowIfAllDrawer : MaterialShowIfAnyDrawer {
//
//     public MaterialShowIfAllDrawer(string string1) : base(string1) { }
//     public MaterialShowIfAllDrawer(string string1, string string2) : base(string1, string2) { }
//     public MaterialShowIfAllDrawer(string string1, string string2, string string3) : base(string1, string2, string3) { }
//     public MaterialShowIfAllDrawer(string string1, string string2, string string3, string string4) : base(string1, string2, string3, string4) { }
//
//     public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) {
//         requireAllKeywords = true;
//         base.OnGUI(position, prop, label, editor);
//     }
//
// }
