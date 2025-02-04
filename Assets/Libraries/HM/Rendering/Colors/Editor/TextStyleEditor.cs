using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using System.Linq;
using System.Text.RegularExpressions;
using HMUI;
using UnityEngine;

namespace ColorLibrary {

    public class TextStyleEditor : IColorLibraryTab {

        private const string kButtonStyleNameSuffix = "TextStyle";
        private const string kTextStyleDirectory =
            "Packages/com.beatgames.beatsaber.tours.core/UI/Common/SO/TextStyles";

        private static readonly Color kListHighlightColor = new(1.25f, 1.25f, 1.25f);

        private List<TextStyleSO> _styles = new();
        private readonly List<TextStyleSO> _selectedStyles = new();

        private Vector2 _listScrollPosition;
        private Vector2 _detailsScrollPosition;
        private readonly Dictionary<object, string> _displayNames = new();

        private GUIStyle _headerStyle;
        private GUIStyle headerStyle => _headerStyle ?? new GUIStyle(EditorStyles.boldLabel) { fontSize = 12 };

        private readonly Regex _camelCaseSplitRegex = new (@"(\p{Ll}(?=[\p{Lu}0-9])|\p{Lu}(?=\p{Lu}\p{Ll}|[0-9])|[0-9](?=\p{L}))");

        public void Initialize() {

            RefreshCache();
        }

        public void OnGUI() {

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.Width(350));
            DrawStylesList();
            EditorGUILayout.EndVertical();

            GUILayout.Space(5);

            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            DrawSelectedStyle();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawStylesList() {

            DrawListHeader("Add new style", CreateNewTextStyle);

            _listScrollPosition = EditorGUILayout.BeginScrollView(_listScrollPosition);

            foreach (var textStyle in _styles) {
                string buttonText = GetOrParseDisplayName(textStyle);

                EditorGUILayout.BeginHorizontal();
                Color originalBackgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = _selectedStyles.Contains(textStyle) ? kListHighlightColor : Color.white;
                if (GUILayout.Button(new GUIContent(buttonText))) {
                    if (!Event.current.control) {
                        _selectedStyles.Clear();
                    }

                    _selectedStyles.Add(textStyle);
                }

                GUI.backgroundColor = originalBackgroundColor;

                if (GUILayout.Button("-", GUILayout.Width(20))) {
                    EditorApplication.delayCall += () => DeleteTextStyle(textStyle);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.LabelField("Hold control to select multiple styles", EditorStyles.boldLabel);
        }

        private void DrawListHeader(string text, Action onCreateNewCallback) {

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(text, headerStyle);
            if (GUILayout.Button("+", GUILayout.Width(20))) {
                EditorApplication.delayCall += () => onCreateNewCallback?.Invoke();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSelectedStyle() {

            if (_selectedStyles.Count == 0) {
                EditorGUILayout.LabelField("No style selected.");
                return;
            }

            string headerText = _selectedStyles.Count == 1 ?
                GetOrParseDisplayName(_selectedStyles[0]) :
                $"{_selectedStyles.Count} selected styles";

            EditorGUILayout.LabelField(headerText, headerStyle);

            GUI.enabled = false;
            for (var index = 0; index < _selectedStyles.Count; index++) {
                var selectedStyle = _selectedStyles[index];
                string labelText = _selectedStyles.Count == 1 ? "Reference" : $"Reference {index + 1}";
                EditorGUILayout.ObjectField(labelText, selectedStyle, typeof(TextStyleSO), allowSceneObjects: false);
            }

            GUI.enabled = true;

            SerializedObject serializedObject = new SerializedObject(_selectedStyles.ToArray());
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_textStyle"));
            serializedObject.ApplyModifiedProperties();
        }

        private void CreateNewTextStyle() {

            bool isSuccessful = TextInputModalDialog.Show(
                "Create new color style",
                "Color style name",
                IsTextStyleNameValid,
                out string textStyleName
            );

            if (!isSuccessful) {
                return;
            }

            var newTextStyle = ScriptableObject.CreateInstance<TextStyleSO>();
            newTextStyle.name = GetStyleAssetName(textStyleName);

            _styles.Add(newTextStyle);
            _styles = _styles.OrderBy(colorStyle => colorStyle.name).ToList();

            AssetDatabase.CreateAsset(newTextStyle, Path.Combine(kTextStyleDirectory, $"{newTextStyle.name}.asset"));
            AssetDatabase.Refresh();

            _selectedStyles.Clear();
            _selectedStyles.Add(newTextStyle);
        }

        private bool IsTextStyleNameValid(string name) {

            string assetName = GetStyleAssetName(name);
            return !_styles.Any(
                so => assetName.Equals(so.name, StringComparison.InvariantCultureIgnoreCase)
            );
        }

        private void DeleteTextStyle(TextStyleSO textStyleSo) {

            bool proceedDelete = EditorUtility.DisplayDialog(
                title: "Delete color style?",
                message: $"Are you sure you want to delete {textStyleSo.name}?",
                ok: "Confirm",
                cancel: "Cancel"
            );

            if (!proceedDelete) {
                return;
            }

            _styles.Remove(textStyleSo);
            AssetDatabase.DeleteAsset(textStyleSo.GetAssetPath());
            AssetDatabase.Refresh();

            if (_selectedStyles.Contains(textStyleSo)) {
                _selectedStyles.Remove(textStyleSo);
            }
        }

        public void RefreshCache() {

            _styles = AssetDatabase.FindAssets($"t:{typeof(TextStyleSO)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<TextStyleSO>)
                .ToList();
        }

        private string GetOrParseDisplayName(ScriptableObject key) {

            if (!_displayNames.ContainsKey(key)) {
                string buttonText = key.name;
                if (buttonText.EndsWith(kButtonStyleNameSuffix)) {
                    buttonText = buttonText.Substring(0, buttonText.Length - kButtonStyleNameSuffix.Length);
                }

                buttonText = _camelCaseSplitRegex.Replace(buttonText, "$1 ");

                _displayNames[key] = buttonText;
            }

            return _displayNames[key];
        }

        private string GetStyleAssetName(string displayName) =>
            $"{displayName.Replace(" ", "")}{kButtonStyleNameSuffix}";
    }
}
