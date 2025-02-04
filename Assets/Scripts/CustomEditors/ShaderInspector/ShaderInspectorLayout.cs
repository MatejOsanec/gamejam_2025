namespace BGLib.ShaderInspector {

    using System;
    using UnityEditor;
    using UnityEngine;

    /// Custom Shader Inspector Layouts, similar to Unity's GUILayout or EditorGUILayout
    public static class ShaderInspectorLayout {

        public static readonly GUIStyle errorLabelStyle = new GUIStyle(GUI.skin.label) {
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.red },
            wordWrap = true
        };

        private static readonly GUIStyle _descriptionStyle = new GUIStyle(EditorStyles.label) {
            fontSize = 10,
            fontStyle = FontStyle.Italic,
            wordWrap = true
        };

        private static readonly GUIStyle _documentationButtonStyle = new GUIStyle(GUI.skin.button) {
            fontSize = 10
        };

        public ref struct Category {

            public readonly bool foldout;

            private static readonly GUIStyle _foldoutStyle = new GUIStyle(EditorStyles.foldoutHeader) {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                fixedHeight = 24
            };

            private static readonly GUIStyle _numberOfFeaturesStyle = new GUIStyle(EditorStyles.label) {
                fontSize = _foldoutStyle.fontSize,
                fontStyle = _foldoutStyle.fontStyle,
                fixedHeight = _foldoutStyle.fixedHeight,
                alignment = TextAnchor.MiddleRight
            };

            private Rect _foldoutRect;
            private readonly Color _backgroundColor;

            public Category(string name, string tooltip, int featuresCount, bool foldout, bool showMixed, Color? backgroundColor = null) {

                _backgroundColor = backgroundColor ?? new Color(0.22f, 0.22f, 0.22f, 0f);

                // Draw Foldout
                this.foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, name, _foldoutStyle);

                var featureCountLabel = showMixed ? "-" : featuresCount.ToString();
                GUIContent featureCountLabelContent = new GUIContent(featureCountLabel, "Number of enabled features/keywords in this category");
                // Draw Features Count
                Rect rect = GUILayoutUtility.GetLastRect();
                GUI.Label(
                    new Rect(rect.xMax - 100, rect.y, 100, rect.height),
                    featureCountLabelContent,
                    _numberOfFeaturesStyle
                );

                if (!string.IsNullOrEmpty(tooltip)) {
                    ShowTooltipIfHover(rect, tooltip);
                }

                _foldoutRect = GUILayoutUtility.GetLastRect();
                const float kFoldoutArrowWidth = 12.0f;
                _foldoutRect.x -= kFoldoutArrowWidth;
                _foldoutRect.width += kFoldoutArrowWidth;
            }

            public void Dispose() {

                EditorGUILayout.EndFoldoutHeaderGroup();
                EditorGUILayout.Space();
                var afterFoldoutRect = GUILayoutUtility.GetLastRect();

                // Draw Background
                _foldoutRect.height = afterFoldoutRect.y - _foldoutRect.y + afterFoldoutRect.height;
                EditorGUI.DrawRect(_foldoutRect, _backgroundColor);
            }
        }

        public ref struct Feature {

            public readonly KeywordState state;
            public readonly bool foldout;

            public static readonly GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout) {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                fixedHeight = 22
            };

            private Rect _foldoutRect;
            private readonly Color _backgroundColor;

            public Feature(string name, string disablingPresetName, bool disabled, string tooltip, KeywordState state, bool foldout, Color? backgroundColor = null) {

                _backgroundColor = backgroundColor ?? new Color(0.0f, 0.0f, 0.0f, 0.10f);
                var isDisabledByPreset = !string.IsNullOrEmpty(disablingPresetName);
                // This indentLevel workaround is a workaround for a weird issue where actually using EditorGUI.indentLevel caused EditorGUI.Toggle to be non interactable around indentLevel 3+
                var originalIndentLevel = EditorGUI.indentLevel + 1;
                EditorGUI.indentLevel = 0;

                // Draw Foldout & Toggle
                Rect foldoutRect = GUILayoutUtility.GetRect(1.0f, foldoutStyle.fixedHeight, GUILayout.ExpandWidth(true));
                foldoutRect.x += originalIndentLevel * 15.0f;
                Rect toggleRect = foldoutRect;
                var foldoutTextLength = foldoutStyle.CalcSize(new GUIContent(name));
                toggleRect.width = toggleRect.height;
                toggleRect.x = foldoutRect.x + foldoutTextLength.x;

                EditorGUI.BeginChangeCheck();
                using (new EditorGUI.DisabledScope(isDisabledByPreset || disabled)) {
                    EditorGUI.showMixedValue = state == KeywordState.Mixed;
                    var newValue = EditorGUI.Toggle(toggleRect, state == KeywordState.Enabled);
                    EditorGUI.showMixedValue = false;
                    if (EditorGUI.EndChangeCheck()) {
                        this.state = newValue ? KeywordState.Enabled : KeywordState.Disabled;
                    }
                    else {
                        this.state = state;
                    }
                    this.foldout = EditorGUI.Foldout(foldoutRect, foldout, name, toggleOnLabelClick: true, foldoutStyle);
                }

                // Tooltip
                Rect rect = GUILayoutUtility.GetLastRect();
                if (!string.IsNullOrEmpty(tooltip)) {
                    // Disabled scope mostly for styling purposes
                    using (new EditorGUI.DisabledScope(isDisabledByPreset || disabled)) {
                        ShowTooltipIfHoverWithDisablingPreset(rect, tooltip, disablingPresetName);
                    }
                }

                _foldoutRect = GUILayoutUtility.GetLastRect();
                const float kFoldoutArrowWidth = 12.0f;
                _foldoutRect.x -= kFoldoutArrowWidth;
                _foldoutRect.width += kFoldoutArrowWidth;

                EditorGUI.indentLevel = originalIndentLevel;
            }

            public void Dispose() {

                EditorGUILayout.Space();
                var afterFoldoutRect = GUILayoutUtility.GetLastRect();

                // Draw Background
                _foldoutRect.height = afterFoldoutRect.y - _foldoutRect.y + afterFoldoutRect.height;
                _foldoutRect.x += EditorGUI.indentLevel * 10;
                _foldoutRect.width -= EditorGUI.indentLevel * 10;
                EditorGUI.DrawRect(_foldoutRect, _backgroundColor);
                EditorGUI.indentLevel--;
            }
        }

        public ref struct SubFeature {

            public readonly KeywordState state;

            private Rect _foldoutRect;
            private readonly Color _backgroundColor;

            public SubFeature(string name, string disablingPresetName, bool disabled, string tooltip, KeywordState state, Color? backgroundColor = null) {

                _backgroundColor = backgroundColor ?? new Color(1.0f, 1.0f, 1.0f, 0.0f);

                var isDisabledByPreset = !string.IsNullOrEmpty(disablingPresetName);
                using (new EditorGUI.DisabledScope(isDisabledByPreset || disabled)) {
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.showMixedValue = state == KeywordState.Mixed;
                    var newValue = EditorGUILayout.Toggle(name, state == KeywordState.Enabled);
                    EditorGUI.showMixedValue = false;
                    if (EditorGUI.EndChangeCheck()) {
                        this.state = newValue ? KeywordState.Enabled : KeywordState.Disabled;
                    }
                    else {
                        this.state = state;
                    }
                }

                _foldoutRect = GUILayoutUtility.GetLastRect();

                // Tooltip
                Rect rect = GUILayoutUtility.GetLastRect();
                if (!string.IsNullOrEmpty(tooltip)) {
                    // Disabled scope mostly for styling purposes
                    using (new EditorGUI.DisabledScope(isDisabledByPreset || disabled)) {
                        ShowTooltipIfHoverWithDisablingPreset(rect, tooltip, disablingPresetName);
                    }
                }
            }

            public void Dispose() {

                var afterFoldoutRect = GUILayoutUtility.GetLastRect();

                // Draw Background
                _foldoutRect.height = afterFoldoutRect.y - _foldoutRect.y + afterFoldoutRect.height;
                EditorGUI.DrawRect(_foldoutRect, _backgroundColor);
            }
        }

        public ref struct FeatureDropdown {

            public readonly KeywordState state;
            public readonly bool foldout;
            public readonly int selectedIndex;

            private Rect _foldoutRect;
            private readonly Color _backgroundColor;

            public FeatureDropdown(
                string label,
                string tooltip,
                KeywordState state,
                int selectedIndex,
                bool foldout,
                string disablingPresetName,
                bool disabled,
                string[] displayedOptions,
                Color? backgroundColor = null
            ) {

                _backgroundColor = backgroundColor ?? new Color(0.0f, 0.0f, 0.0f, 0.10f);
                EditorGUI.indentLevel++;

                Rect foldoutRect = GUILayoutUtility.GetRect(1.0f, Feature.foldoutStyle.fixedHeight, GUILayout.ExpandWidth(true));
                Rect popupRect = foldoutRect;
                var fullWidth = foldoutRect.width + EditorGUI.indentLevel * 15.0f;
                popupRect.y += (Feature.foldoutStyle.fixedHeight - Feature.foldoutStyle.fontSize) * 0.5f;
                // These magic values kinda work the same way as EditorLayout.Popup
                popupRect.x = fullWidth * 0.45f - 10 + ShaderInspector.kGlobalLeftOffset;
                popupRect.width = fullWidth * 0.55f + 27;

                var isDisabledByPreset = !string.IsNullOrEmpty(disablingPresetName);
                using (new EditorGUI.DisabledScope(isDisabledByPreset || disabled)) {

                    EditorGUI.BeginChangeCheck();
                    using (new EditorGUI.DisabledScope(isDisabledByPreset || disabled)) {
                        EditorGUI.showMixedValue = state == KeywordState.Mixed;
                        var newValue = EditorGUI.Popup(popupRect, selectedIndex, displayedOptions);
                        EditorGUI.showMixedValue = false;
                        if (EditorGUI.EndChangeCheck()) {
                            this.state = KeywordState.Enabled;
                        }
                        else {
                            this.state = state;
                        }
                        this.selectedIndex = newValue;
                    }
                }

                this.foldout = EditorGUI.Foldout(foldoutRect, foldout, label, toggleOnLabelClick: true, Feature.foldoutStyle);

                // Tooltip
                Rect rect = GUILayoutUtility.GetLastRect();
                if (!string.IsNullOrEmpty(tooltip)) {
                    // Disabled scope mostly for styling purposes
                    using (new EditorGUI.DisabledScope(isDisabledByPreset || disabled)) {
                        ShowTooltipIfHoverWithDisablingPreset(rect, tooltip, disablingPresetName);
                    }
                }

                _foldoutRect = GUILayoutUtility.GetLastRect();
                const float kFoldoutArrowWidth = 12.0f;
                _foldoutRect.x -= kFoldoutArrowWidth;
                _foldoutRect.width += kFoldoutArrowWidth;
            }

            public void Dispose() {

                EditorGUILayout.Space();
                var afterFoldoutRect = GUILayoutUtility.GetLastRect();

                // Draw Background
                _foldoutRect.height = afterFoldoutRect.y - _foldoutRect.y + afterFoldoutRect.height;
                _foldoutRect.x += EditorGUI.indentLevel * 10;
                _foldoutRect.width -= EditorGUI.indentLevel * 10;
                EditorGUI.DrawRect(_foldoutRect, _backgroundColor);
                EditorGUI.indentLevel--;
            }
        }

        public static void Description(string description, string documentationUrl = null, string documentationButtonLabel = null) {

            using (new EditorGUILayout.HorizontalScope()) {
                if (string.IsNullOrEmpty(description)) {
                    GUILayout.FlexibleSpace();
                }
                else {
                    EditorGUILayout.LabelField(description, _descriptionStyle);
                }

                DocumentationButton(documentationUrl, documentationButtonLabel);
            }
        }

        public static void ShowTooltipIfHoverWithDisablingPreset(Rect area, string tooltip, string disablingPreset) {

            if (string.IsNullOrEmpty(disablingPreset)) {
                ShowTooltipIfHover(area, tooltip);
                return;
            }
            // Not calling ShowTooltipIfHover intentionally to only create new strings if hovered, not every frame
            if (area.Contains(Event.current.mousePosition)) {
                tooltip = $"Editing disabled by preset \"{disablingPreset}\"\n\n{tooltip}";
                EditorGUI.LabelField(area, new GUIContent(text: string.Empty, tooltip));
            }
        }

        public static void ShowTooltipIfHover(Rect area, string tooltip) {

            if (area.Contains(Event.current.mousePosition)) {
                EditorGUI.LabelField(area, new GUIContent(text: string.Empty, tooltip));
            }
        }

        public static void DocumentationButton(string documentationUrl, string documentationButtonLabel = null) {

            if (string.IsNullOrEmpty(documentationUrl)) {
                return;
            }
            documentationButtonLabel ??= "?";
            GUILayout.FlexibleSpace();

            // Documentation buttons should always be enabled
            var wasEnabled = GUI.enabled;
            GUI.enabled = true;
            if (GUILayout.Button(documentationButtonLabel, _documentationButtonStyle)) {
                Application.OpenURL(documentationUrl);
            }
            GUI.enabled = wasEnabled;
        }
    }
}
