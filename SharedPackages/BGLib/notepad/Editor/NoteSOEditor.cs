using UnityEngine;
using UnityEditor;

namespace Notepad {

    [CustomEditor(typeof(NoteSO))]
    public class NoteSOEditor : Editor {

        private NoteSO _note = null;
        private bool _richText = true;
        private bool _openURL = true;

        public override void OnInspectorGUI() {

            DrawEditor();
        }

        public void DrawEditor() {

            _note = (NoteSO)serializedObject.targetObject;

            _richText = EditorGUILayout.Toggle("Show Rich Text", _richText, EditorStyles.toggle);

            EditorGUILayout.LabelField("Title");
            string newTitle = EditorGUILayout.TextField(_note.title, EditorStyles.textField);
            if (newTitle != _note.title) {
                _note.title = newTitle;
            }

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Content");
            GUIStyle textAreaStyle = EditorStyles.textArea;
            textAreaStyle.richText = _richText;
            textAreaStyle.wordWrap = true;
            _note.content = EditorGUILayout.TextArea(_note.content, textAreaStyle);

            EditorGUILayout.Separator();

            _note.urls = DrawURLArray("URLs", _note.urls);
        }


        private NoteURL[] DrawURLArray(string label, NoteURL[] array) {

            if (array == null) {
                array = new NoteURL[0];
            }

            _openURL = EditorGUILayout.Foldout(_openURL, label, true);
            int newSize = array.Length;

            if (_openURL) {
                newSize = EditorGUILayout.DelayedIntField("Size", newSize);
                newSize = newSize < 0 ? 0 : newSize;

                // resize array
                if (newSize != array.Length) {
                    NoteURL[] newArray = new NoteURL[newSize];
                    for (var i = 0; i < newSize; i++) {
                        if (i < array.Length) {
                            newArray[i] = array[i];
                        }
                    }
                    array = newArray;
                }

                // Header
                EditorGUILayout.BeginHorizontal();
                try {
                    EditorGUILayout.LabelField("Name");
                    EditorGUILayout.LabelField("URL");
                }
                finally {
                    EditorGUILayout.EndHorizontal();
                }

                // render array elements
                for (var i = 0; i < newSize; i++) {
                    EditorGUILayout.BeginHorizontal();
                    try {
                        array[i].name = EditorGUILayout.TextField(array[i].name);
                        array[i].url = EditorGUILayout.TextField(array[i].url);
                    }
                    finally {
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }

            return array;
        }
    }
}
