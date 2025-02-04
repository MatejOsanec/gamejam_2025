namespace BGLib.UnityExtension.Editor {

    using System;
    using UnityEditor;
    using UnityEngine;

    public class AssetDatabaseTracker : IDisposable {

        private bool _needRefreshAssets;
        private bool _needSaveAssets;

        public T CreateScriptableObject<T>(string path) where T : ScriptableObject {

            var so = AssetDatabase.LoadAssetAtPath<T>(path);
            if (so == null) {
                so = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(so, path);
                _needRefreshAssets = true;
            }

            return so;
        }

        public void Flush() {

            if (_needRefreshAssets) {
                _needRefreshAssets = false;
                AssetDatabase.Refresh();
            }

            if (_needSaveAssets) {
                _needSaveAssets = false;
                AssetDatabase.SaveAssets();
            }
        }

        public void SetDirty(UnityEngine.Object unityObject) {

            EditorUtility.SetDirty(unityObject);
            RequestSave();
        }

        public void RequestSave() {

            _needSaveAssets = true;
        }

        public void Dispose() {

            Flush();
        }
    }
}
