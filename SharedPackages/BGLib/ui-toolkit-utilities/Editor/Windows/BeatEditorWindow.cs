namespace BGLib.UiToolkitUtilities.Editor.Windows {

    using System;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    public abstract class BeatEditorWindow<TModel> : EditorWindow {

        private const string kUssContainerRoot = "container-root";

        protected abstract TModel CreateModel();
        protected abstract string GetTemplatePath();
        protected abstract string GetTitle();

        protected virtual void OnModelAttached(VisualElement visualElement) { }
        protected virtual void OnModelDetached() { }

        protected TModel _model { get; private set; }

        public void CreateGUI() {

            try {
                titleContent = new GUIContent(GetTitle());

                _model = CreateModel();

                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(GetTemplatePath()).CloneTree(rootVisualElement);

                OnModelAttached(rootVisualElement);
            }
            catch (Exception e) {
                Debug.LogException(e);
                Close();
            }
        }

        public void OnDestroy() {

            OnModelDetached();
            _model = default;
        }
    }
}
