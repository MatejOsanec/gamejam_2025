using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(BoolSO))]
public class BoolSOEditor : Editor {
    public override void OnInspectorGUI() {

        var boolSO = (BoolSO)target;
        EditorGUILayout.LabelField("Value", boolSO.value.ToString());
    }
}