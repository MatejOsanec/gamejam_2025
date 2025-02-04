using System;
using System.Linq;
using UnityEngine;
using UnityEditor;

internal class MaterialVectorShowIfAnyDrawer : MaterialPropertyDrawer {

    private string[] _conditions;
    private readonly int _fieldCount;
    private bool _normalize;
    private bool _show;
    private readonly int _requiredCount;

    protected MaterialVectorShowIfAnyDrawer() { }

    public MaterialVectorShowIfAnyDrawer(float fields) {
        _fieldCount = Mathf.Clamp(Mathf.RoundToInt(fields), 2, 4);
        _conditions = new string[0];
        _normalize = false;
        _requiredCount = 0;
    }

    public MaterialVectorShowIfAnyDrawer(float fields, string string1) {
        _fieldCount = Mathf.Clamp(Mathf.RoundToInt(fields), 2, 4);
        _requiredCount = 0;
        SetConditionsAndNormalization(new[] { string1 });
    }

    public MaterialVectorShowIfAnyDrawer(float fields, string string1, string string2) {
        _fieldCount = Mathf.Clamp(Mathf.RoundToInt(fields), 2, 4);
        _requiredCount = 0;
        SetConditionsAndNormalization(new[] { string1, string2 });
    }

    public MaterialVectorShowIfAnyDrawer(float fields, string string1, string string2, string string3) {
        _fieldCount = Mathf.Clamp(Mathf.RoundToInt(fields), 2, 4);
        _requiredCount = 0;
        SetConditionsAndNormalization(new[] { string1, string2, string3 });
    }

    public MaterialVectorShowIfAnyDrawer(float fields, float requiredCount, string string1, string string2) {
        _fieldCount = Mathf.Clamp(Mathf.RoundToInt(fields), 2, 4);
        _requiredCount = (int) requiredCount;
        SetConditionsAndNormalization(new[] { string1, string2 });
    }

    public MaterialVectorShowIfAnyDrawer(float fields, float requiredCount, string string1, string string2, string string3) {
        _fieldCount = Mathf.Clamp(Mathf.RoundToInt(fields), 2, 4);
        _requiredCount = (int) requiredCount;
        SetConditionsAndNormalization(new[] { string1, string2, string3});
    }

    public MaterialVectorShowIfAnyDrawer(float fields, float requiredCount, string string1, string string2, string string3, string string4) {
        _fieldCount = Mathf.Clamp(Mathf.RoundToInt(fields), 2, 4);
        _requiredCount = (int) requiredCount;
        SetConditionsAndNormalization(new[] { string1, string2, string3, string4 });
    }

    public MaterialVectorShowIfAnyDrawer(float fields, float requiredCount, string string1, string string2, string string3, string string4, string string5) {
        _fieldCount = Mathf.Clamp(Mathf.RoundToInt(fields), 2, 4);
        _requiredCount = (int) requiredCount;
        SetConditionsAndNormalization(new[] { string1, string2, string3, string4, string5 });
    }

    private void SetConditionsAndNormalization(string[] strings) {

        _normalize = strings[0] == "normalize";
        int normalizeInt = _normalize ? 1 : 0;

        _conditions =  new string[strings.Length - normalizeInt];

        for (int i = 0; i < _conditions.Length; i++) {
            _conditions[i] = strings[i + normalizeInt];
        }
    }

    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)  {
        return 0.0f;
    }

    private Vector4 Round( Vector4 value, int digits )
    {
        float multiplication = Mathf.Pow( 10, digits );
        float division = 1f / multiplication;

        value.x = Mathf.Round( value.x * multiplication ) * division;
        value.y = Mathf.Round( value.y * multiplication ) * division;
        value.z = Mathf.Round( value.z * multiplication ) * division;
        value.w = Mathf.Round( value.w * multiplication ) * division;
        return value;
    }

    public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) {

        _show = MaterialDrawerConditionsTester.TestConditions(editor.targets, _conditions, _requiredCount); //requireAllKeywords;

        if (!_show) {
            return;
        }

        if (_conditions.Length > 0) {
            EditorGUI.indentLevel++;
        }

        var vectorValue = prop.vectorValue;

        EditorGUI.BeginChangeCheck();

            var originalWideMode = EditorGUIUtility.wideMode;
            EditorGUIUtility.wideMode = false;

            switch( _fieldCount )
            {
                case 2:
                    vectorValue = EditorGUILayout.Vector2Field(label, vectorValue);
                    break;
                case 3:
                    vectorValue = EditorGUILayout.Vector3Field(label, vectorValue);
                    break;
                default:
                    vectorValue = EditorGUILayout.Vector4Field( label, vectorValue );
                    break;
            }

            EditorGUIUtility.wideMode = originalWideMode;
            EditorGUILayout.Space(3);

            if (_conditions.Length > 0) {
                EditorGUI.indentLevel--;
            }

        if (EditorGUI.EndChangeCheck()) {
            if (_normalize) {
                vectorValue = Round(vectorValue.normalized, 3);
            }
            prop.vectorValue = vectorValue;
        }

    }

}