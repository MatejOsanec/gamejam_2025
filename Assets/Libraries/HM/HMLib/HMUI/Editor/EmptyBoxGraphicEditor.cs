using UnityEngine;
using UnityEditor;

namespace HMUI {

    [CustomEditor(typeof(EmptyBoxGraphic))]
    [CanEditMultipleObjects]
    public class EmptyBoxGraphicEditor : Editor {
        
        private SerializedProperty _depthProperty;
        
        protected void OnEnable() {
            
            _depthProperty = serializedObject.FindProperty("_depth");
        }

        public override void OnInspectorGUI() {

            serializedObject.Update();
            EditorGUILayout.PropertyField(_depthProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
}