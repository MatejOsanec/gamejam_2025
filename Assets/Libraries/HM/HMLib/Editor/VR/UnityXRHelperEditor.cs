using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnityXRHelper))]
public class UnityXRHelperEditor : Editor {

    private UnityXRHelper _unityXRHelper;

    void OnEnable() {

        _unityXRHelper = serializedObject.targetObject as UnityXRHelper;
    }

    public override void OnInspectorGUI() {

        DrawDefaultInspector();
        if (!Application.isPlaying || _unityXRHelper == null) {
            return;
        }
        if (_unityXRHelper.leftController != null) {
            EditorGUILayout.LabelField(
                $"Left controller manufacturer: {_unityXRHelper.leftController.manufacturerName}"
            );
        }
        if (_unityXRHelper.rightController != null) {
            EditorGUILayout.LabelField(
                $"Right controller manufacturer: {_unityXRHelper.rightController.manufacturerName}"
            );
        }
    }
}
