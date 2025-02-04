namespace BGLib.Polyglot.Editor {

    using UnityEditor;
    using UnityEditor.AnimatedValues;

    [CustomEditor(typeof(LocalizedText), true)]
    [CanEditMultipleObjects]
    public class LocalizedTextEditor : LocalizedEditor<LocalizedText> {
        
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
            var text = target as LocalizedText;
            if (text == null) {
                return;
            }
            var parameters = text.Parameters;
            if (showParameters == null || parameters.Count <= 0) {
                return;
            }
            showParameters.target = EditorGUILayout.Foldout(showParameters.target, "Parameters");
            if (EditorGUILayout.BeginFadeGroup(showParameters.faded)) {
                EditorGUI.indentLevel++;
                for (var index = 0; index < parameters.Count; index++) {
                    var parameter = parameters[index];
                    EditorGUILayout.SelectableLabel(parameter != null ? parameter.ToString() : "null");
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();
        }
    }
}
