namespace BGLib.Polyglot.Editor {
    
    using UnityEditor;
    using UnityEditor.AnimatedValues;

    [CustomEditor(typeof(LocalizedTextMeshProUGUI), true)]
    [CanEditMultipleObjects]
    public class LocalizedTextMeshProUGUIEditor : LocalizedEditor<LocalizedTextMeshProUGUI> {
        
        private AnimBool? showParameters;

        public override void OnEnable() {
            
            base.OnEnable();
            showParameters = new AnimBool(true);
            showParameters.valueChanged.AddListener(Repaint);
        }

        public override void OnInspectorGUI() {
            
            OnInspectorGUI("key");

            if (serializedObject.isEditingMultipleObjects) {
                return;
            }
            var text = target as LocalizedTextMeshProUGUI;
            if (text == null) {
                return;
            }
            var parameters = text.Parameters;
            if (showParameters == null || parameters.Count <= 0) {
                return;
            }
            showParameters.value = EditorGUILayout.Foldout(showParameters.value, "Parameters");
            if (EditorGUILayout.BeginFadeGroup(showParameters.faded)) {
                EditorGUI.indentLevel++;
                for (int index = 0; index < parameters.Count; index++) {
                    var parameter = parameters[index];
                    EditorGUILayout.SelectableLabel(parameter != null ? parameter.ToString() : "null");
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();
        }
    }
}
