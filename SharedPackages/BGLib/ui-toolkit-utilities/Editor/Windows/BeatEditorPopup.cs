namespace BGLib.UiToolkitUtilities.Editor.Windows {

    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    public abstract class BeatEditorPopup<TModel> : PopupWindowContent {

        protected readonly TModel _model;

        protected abstract string GetTemplatePath();

        public void Show(Rect rect) {

            UnityEditor.PopupWindow.Show(rect, this);
        }

        public override void OnGUI(Rect rect) {
            // Intentionally left empty
        }

        public sealed override void OnOpen() {

            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(GetTemplatePath()).CloneTree(editorWindow.rootVisualElement);

            OnModelAttached(editorWindow.rootVisualElement);
        }

        public sealed override void OnClose() {

            OnModelDetached();
        }

        protected BeatEditorPopup(TModel model) {

            _model = model;
        }

        protected virtual void OnModelAttached(VisualElement visualElement) { }
        protected virtual void OnModelDetached() { }
    }
}
