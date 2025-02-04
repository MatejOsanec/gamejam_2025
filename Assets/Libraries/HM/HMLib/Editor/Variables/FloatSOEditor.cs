using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(FloatSO))]
public class FloatSOEditor : Editor {
    public override void OnInspectorGUI() {

        var boolSO = (FloatSO)target;
        EditorGUILayout.LabelField("Value", boolSO.value.ToString());
    }
}