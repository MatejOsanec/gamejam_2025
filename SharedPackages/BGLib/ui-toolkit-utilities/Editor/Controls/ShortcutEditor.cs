
using System.Linq;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace UIToolkitUtilities.Controls.Editor {

    using UnityEngine.UIElements;

    public class ShortcutEditor : VisualElement {

        public new class UxmlFactory : UxmlFactory<ShortcutEditor, UxmlTraits> { }

        private const string kControlPath =
            "Packages/com.beatgames.bglib.ui-toolkit-utilities/Editor/Controls/ShortcutEditor.uxml";

        private readonly KeyCode[] _ignoredKeys = new KeyCode[] {
            KeyCode.None,
            KeyCode.LeftShift,
            KeyCode.RightShift,
            KeyCode.LeftAlt,
            KeyCode.RightAlt,
            KeyCode.LeftControl,
            KeyCode.RightControl
        };

        private Label _label;
        private Button _button;

        private string _shortcutPath;
        private bool _isListening;
        private ShortcutBinding? _shortcutBinding;

        public ShortcutEditor() {

            VisualTreeAsset control = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(kControlPath);
            TemplateContainer controlRoot = control.Instantiate();

            _label = controlRoot.Q<Label>("Shortcut");
            _button = controlRoot.Q<Button>("ShortcutButton");

            Add(controlRoot);

            UpdateBinding();

            RegisterCallback<KeyDownEvent>(OnKeyDownEvent, TrickleDown.TrickleDown);

            _button.RegisterCallback<FocusOutEvent>(OnFocusOutEvent);
            _button.clicked += () => SetIsListening(true);
        }

        private void OnFocusOutEvent(FocusOutEvent evt) {

            if (!_isListening) {
                return;
            }

            SetIsListening(false);
        }

        private void OnKeyDownEvent(KeyDownEvent evt) {

            if (!_isListening) {
                return;
            }

            var keyCode = evt.keyCode;
            var eventModifiers = evt.modifiers;

            if (keyCode == KeyCode.Escape) {
                ShortcutManager.instance.ClearShortcutOverride(_shortcutPath);
                SetIsListening(false);
                UpdateBinding();
                return;
            }

            if (_ignoredKeys.Contains(keyCode)) {
                return;
            }

            ShortcutModifiers modifiers = ShortcutModifiers.None;
            if (eventModifiers.HasFlag(EventModifiers.Control)) {
                modifiers |= ShortcutModifiers.Control;
            }

            if (eventModifiers.HasFlag(EventModifiers.Alt)) {
                modifiers |= ShortcutModifiers.Alt;
            }

            if (eventModifiers.HasFlag(EventModifiers.Shift)) {
                modifiers |= ShortcutModifiers.Shift;
            }

            // Execute after 1ms so the window does not open immediately
            schedule.Execute(
                () => {
                    ShortcutBinding newBinding = new ShortcutBinding(new KeyCombination(keyCode, modifiers));
                    ShortcutManager.instance.RebindShortcut(_shortcutPath, newBinding);
                    UpdateBinding();
                }
            ).StartingIn(1);


            SetIsListening(false);
        }

        public void SetShortcutPath(string shortcutPath) {

            _shortcutPath = shortcutPath;
            UpdateBinding();
        }

        private void UpdateBinding() {

            if (!string.IsNullOrWhiteSpace(_shortcutPath)) {
                _shortcutBinding = ShortcutManager.instance.GetShortcutBinding(_shortcutPath);
                style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            }
            else {
                _shortcutBinding = null;
                style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }

            _label.text = GetShortcutText();

            bool isReadOnly = ShortcutManager.instance.IsProfileReadOnly(ShortcutManager.instance.activeProfileId);
            _button.SetEnabled(!isReadOnly);
            tooltip = isReadOnly ?
                "Shortcut profile is read only.\nCreate an editable profile via Edit > Shortcuts... > New Profile" :
                "Click to change shortcut.";
        }

        private void SetIsListening(bool b) {

            _isListening = b;
            _label.text = b ? "Listening..." : GetShortcutText();
        }

        private string GetShortcutText() {

            string shortcutText = _shortcutBinding?.ToString();
            return string.IsNullOrWhiteSpace(shortcutText) ? "-" : shortcutText;
        }
    }
}
