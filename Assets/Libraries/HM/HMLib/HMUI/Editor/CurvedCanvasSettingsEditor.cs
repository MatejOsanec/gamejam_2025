using UnityEditor;

namespace HMUI {

    [CustomEditor(typeof(CurvedCanvasSettings))]
    [CanEditMultipleObjects]
    public class CurvedCanvasSettingsEditor : Editor {

        SerializedProperty _radiusProperty;
        SerializedProperty _useFlatInEditModeProperty;

        void OnEnable() {

            _radiusProperty = serializedObject.FindProperty("_radius");
            _useFlatInEditModeProperty = serializedObject.FindProperty("_useFlatInEditMode");
        }

        public override void OnInspectorGUI() {

            serializedObject.Update();
            EditorGUILayout.PropertyField(_radiusProperty);
            EditorGUILayout.PropertyField(_useFlatInEditModeProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
