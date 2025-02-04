using UnityEngine;
using System.Text;
using System.Linq;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Notepad {

    [Serializable]
    public struct NoteURL {

        [NullAllowed] public string name;
        [NullAllowed] public string url;
    }

    [Serializable]
    public class NoteSO : ScriptableObject {

        [NullAllowed] public string title;
        [NullAllowed] public string content;
        [NullAllowed] public NoteURL[] urls;

#if UNITY_EDITOR
        private const string kNoteSubDirectory = "Notepad/";
        private const string kNoteExtension = ".asset";
        private const string kDraftNoteName = "draft_";

        public bool Save() {

            string noteDirectoryAsset = kNoteSubDirectory.Substring(0, kNoteSubDirectory.Length - 1);
            if (!AssetDatabase.IsValidFolder("Assets/" + noteDirectoryAsset)) {
                AssetDatabase.CreateFolder("Assets", noteDirectoryAsset);
            }

            if (AssetDatabase.GetAssetPath(this).Length == 0) {
                int numDrafts = AssetDatabase.FindAssets(kDraftNoteName, new string[] { "Assets/" + noteDirectoryAsset }).Length;
                AssetDatabase.CreateAsset(this, "Assets/" + kNoteSubDirectory + kDraftNoteName + numDrafts + kNoteExtension);
            }

            EditorUtility.SetDirty(this);
            string previousFileName = AssetDatabase.GetAssetPath(this)
                .Replace("Assets/" + kNoteSubDirectory, "")
                .Replace(kNoteExtension, "");
            previousFileName = GetFileNameFromTitle(previousFileName);
            string newFileName = GetFileNameFromTitle(title);

            bool renameExists = AssetDatabase.FindAssets(title, new string[] { "Assets/" + noteDirectoryAsset }).Length > 0;
            if (renameExists && previousFileName != newFileName) {
                EditorUtility.DisplayDialog("Error", message: $"Could not rename note from {previousFileName}{kNoteExtension} to {newFileName}{kNoteExtension}, as another file with that name already exists. The title was reverted.", "OK");
                AssetDatabase.Refresh();
                return false;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(this), newFileName);
            AssetDatabase.Refresh();

            return true;
        }

        public string GetFileNameFromTitle(string str) {

            var invalidFileNameChars = System.IO.Path.GetInvalidFileNameChars().ToList();
            invalidFileNameChars.Add('\'');
            invalidFileNameChars.Add('\"');
            var result = new StringBuilder(str.Trim());
            for (int i = result.Length-1; i >= 0; i--) {
                if (result[i] == ' ') {
                    result.Remove(i, 1);
                    result[i] = System.Char.ToUpper(result[i]);
                }

                if (invalidFileNameChars.Contains(result[i])){
                    result[i] = '_';
                }
            }

            if (result.Length > 0) {
                result[0] = System.Char.ToUpper(result[0]);
            }

            return result.ToString();
        }
#endif
    }
}
