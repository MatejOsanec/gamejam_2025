namespace HMUI {

    using UnityEditor;

    [CustomEditor(typeof(Touchable), true)]
    [CanEditMultipleObjects]
    public class TouchableEditor : Editor {

#if BS_TOURS
        public override void OnInspectorGUI() {}
#else
        private SerializedProperty _skewProperty;

        protected void OnEnable() {

            _skewProperty = serializedObject.FindProperty("_skew");
        }

        public override void OnInspectorGUI() {

            EditorGUILayout.PropertyField(_skewProperty);
            serializedObject.ApplyModifiedProperties();
        }
#endif
    }
}
