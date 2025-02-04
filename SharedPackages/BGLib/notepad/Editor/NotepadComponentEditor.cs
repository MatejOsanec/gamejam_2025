using UnityEngine;
using UnityEditor;
using Notepad;

[CustomEditor(typeof(NotepadComponent))]
public class NotepadComponentEditor : Editor {

    private SerializedProperty _noteProperty;

    public override void OnInspectorGUI() {

        _noteProperty = serializedObject.FindProperty("note");
        if (_noteProperty == null) {
            return;
        }

        serializedObject.Update();
        EditorGUILayout.PropertyField(_noteProperty, GUIContent.none);
        serializedObject.ApplyModifiedProperties();

        DrawSelection();
        EditorGUILayout.Separator();
        DrawNoteContents();
    }

    private void DrawSelection() {

        if (_noteProperty.objectReferenceValue == null) {
            if (GUILayout.Button("New note", EditorStyles.miniButton)) {
                _noteProperty.objectReferenceValue = ScriptableObject.CreateInstance<NoteSO>();
                NoteSO noteSO = _noteProperty.objectReferenceValue as NoteSO;
                noteSO.title = string.Empty;
                noteSO.content = string.Empty;
                NoteEditorWindow.ShowWindow((NoteSO)_noteProperty.objectReferenceValue);
            }
        }
        else {
            if (GUILayout.Button("Edit note", EditorStyles.miniButton)) {
                NoteEditorWindow.ShowWindow((NoteSO)_noteProperty.objectReferenceValue);
            }
        }
    }

    private void DrawNoteContents() {

        if (_noteProperty.objectReferenceValue == null) {
            return;
        }

        NoteSO note = (NoteSO)_noteProperty.objectReferenceValue;
        if (note.title?.Length <= 0 || note.content?.Length <= 0) {
            return;
        }

        EditorGUILayout.LabelField(note.title, EditorStyles.boldLabel);

        GUIStyle multiLineLabel = EditorStyles.label;
        multiLineLabel.wordWrap = true;
        multiLineLabel.richText = true;
        EditorGUILayout.LabelField(note.content, multiLineLabel);

        if (note.urls != null && note.urls.Length > 0) {
            EditorGUILayout.LabelField("Links: ", EditorStyles.boldLabel);

            foreach (var noteUrl in note.urls) {
                if (GUILayout.Button(noteUrl.name, EditorStyles.miniButton)) {
                    Application.OpenURL(noteUrl.url);
                }
            }
        }
    }
}
