namespace BGLib.Polyglot.Editor {

    using System;
    using UnityEditor;
    using UnityEditor.AnimatedValues;
    using UnityEngine;

    public abstract class LocalizedEditor<T> : Editor where T : class, ILocalize {

        private Vector2 _scroll;
        private AnimBool? _showAutoComplete;
        private LocalizationModel _localization = default!;

        private const string kLocalizedComponentVarName = "localizedComponent";

        public virtual void OnEnable() {

            _showAutoComplete = new AnimBool(true);
            _showAutoComplete.valueChanged.AddListener(Repaint);
            _localization = EditorLocalization.instance;
        }

        protected void OnInspectorGUI(string propertyPath) {

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            var iterator = serializedObject.GetIterator();
            bool hasComponent = true;
            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false) {

                EditorGUILayout.PropertyField(iterator, true, Array.Empty<GUILayoutOption>());
                if (iterator.name == propertyPath) {
                    var key = iterator.stringValue;
                    var hasValue = _localization.TryGet(
                        key,
                        _localization.SelectedLanguage,
                        out var localizedString
                    );
                    EditorGUILayout.LabelField("Localized Text:");
                    EditorGUILayout.SelectableLabel(localizedString);

                    if (string.IsNullOrEmpty(key)) {
                        continue;
                    }
                    if (!hasValue) {
                        DrawAutoComplete(iterator);
                    }
                    continue;
                }

                if (iterator.name == kLocalizedComponentVarName && iterator.objectReferenceValue == null) {
                    hasComponent = false;
                }
            }

            serializedObject.ApplyModifiedProperties();

            if (hasComponent && EditorGUI.EndChangeCheck() && target is T text) {
                text.OnLocalize(_localization);
            }
        }

        private void DrawAutoComplete(SerializedProperty property) {

            var localization = EditorLocalization.instance;
            var localizedStrings = localization.importer.GetLanguagesStartsWith(property.stringValue);

            if (localizedStrings.Count == 0) {
                localizedStrings = localization.importer.GetLanguagesContains(property.stringValue);
            }

            var selectedLanguage = (int)_localization.SelectedLanguage;

            if (_showAutoComplete != null) {
                _showAutoComplete.target = EditorGUILayout.Foldout(_showAutoComplete.target, "Auto-Complete");
                if (EditorGUILayout.BeginFadeGroup(_showAutoComplete.faded)) {
                    EditorGUI.indentLevel++;

                    var height = EditorGUIUtility.singleLineHeight * (Mathf.Min(localizedStrings.Count, 6) + 1);
                    _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(height));
                    foreach (var local in localizedStrings) {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel(local.Key);
                        if (GUILayout.Button(local.Value[selectedLanguage], "CN CountBadge")) {
                            property.stringValue = local.Key;
                            GUIUtility.hotControl = 0;
                            GUIUtility.keyboardControl = 0;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndScrollView();
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndFadeGroup();

        }
    }
}
