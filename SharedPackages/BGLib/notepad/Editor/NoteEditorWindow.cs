using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;

namespace Notepad {

    public class NoteEditorWindow : EditorWindow {

        private NoteSO _note = null;
        private NoteSOEditor _soEditor;

        public static void ShowWindow(NoteSO noteToEdit) {

            Assert.IsNotNull(noteToEdit.title);
            Assert.IsNotNull(noteToEdit.content);

            var window = GetWindow<NoteEditorWindow>();
            window.titleContent = new GUIContent(noteToEdit.title);
            window._note = noteToEdit;
            window._soEditor = (NoteSOEditor)Editor.CreateEditor(window._note);
            window.Show();
        }

        private void OnGUI() {

            if (_note == null) {
                return;
            }

            _soEditor.DrawEditor();

            if (GUILayout.Button("Save", EditorStyles.miniButton)) {
                if (_note.title.Length == 0) {
                    EditorUtility.DisplayDialog("Error", "A note needs a title", "OK");
                    return;
                }

                if (!_note.Save()) {
                    return; // Error dialogue displayed in save.
                }

                this.Close();
            }
        }
    }
}
