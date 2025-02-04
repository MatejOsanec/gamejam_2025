namespace BGLib.Polyglot.Editor {

    using UnityEditor;
    using UnityEditor.AnimatedValues;

    [CustomEditor(typeof(LocalizedTextMeshPro))]
    [CanEditMultipleObjects]
    public class LocalizedTextMeshProEditor : LocalizedEditor<LocalizedTextMeshPro> {
        
        private AnimBool showParameters = new AnimBool(false);

        public override void OnInspectorGUI() {
            
            OnInspectorGUI("key");

            if (!serializedObject.isEditingMultipleObjects) {
                var text = target as LocalizedTextMeshPro;
                if (text != null) {
                    var parameters = text.Parameters;
                    if (parameters != null && parameters.Count > 0) {
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
        }
    }
}
