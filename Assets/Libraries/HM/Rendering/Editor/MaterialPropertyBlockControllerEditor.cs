using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MaterialPropertyBlockControllerEditorSettings : ScriptableObject {

    public List<string> testerMaterialProperties = new() { "_Color", "_RimLightColor", "_GlowCutoutColor" };
    public float colorAlpha = 1.0f;
}

[CustomEditor(typeof(MaterialPropertyBlockController))]
public class MaterialPropertyBlockControllerEditor : Editor {

    private MaterialPropertyBlockController targetObject;
    private MaterialPropertyBlockControllerEditorSettings settings;
    private SerializedObject settingsSerializedObject;

    private void OnEnable() {

        targetObject = (MaterialPropertyBlockController)serializedObject.targetObject;
        settings = CreateInstance<MaterialPropertyBlockControllerEditorSettings>();
        settingsSerializedObject = new SerializedObject(settings);
    }

    public override void OnInspectorGUI() {

        base.OnInspectorGUI();
        EditorGUILayout.Separator();

        GUILayout.Label("Color tester", EditorStyles.boldLabel);
        if (GUILayout.Button("Red")) {
            SetColorTo(Color.red);
        }
        if (GUILayout.Button("Green")) {
            SetColorTo(Color.green);
        }
        if (GUILayout.Button("Blue")) {
            SetColorTo(Color.blue);
        }

        EditorGUILayout.Separator();
        GUILayout.Label("Color Tester Settings", EditorStyles.boldLabel);
        settingsSerializedObject.Update();
        EditorGUI.BeginChangeCheck();
        try {
            EditorGUILayout.PropertyField(settingsSerializedObject.FindProperty("colorAlpha"));
            EditorGUILayout.PropertyField(settingsSerializedObject.FindProperty("testerMaterialProperties"), includeChildren: true);
        }
        finally {
            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
            }
        }

    }

    private void SetColorTo(Color c) {

        foreach (var property in settings.testerMaterialProperties) {
            targetObject.materialPropertyBlock.SetColor(property, c.ColorWithAlpha(settings.colorAlpha));
        }
        targetObject.ApplyChanges();
        EditorApplication.QueuePlayerLoopUpdate();
    }
}
