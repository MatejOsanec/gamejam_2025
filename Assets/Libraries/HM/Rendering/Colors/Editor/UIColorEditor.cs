using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using System.Linq;
using System.Text.RegularExpressions;
using HMUI;
using UnityEngine;

namespace ColorLibrary {

    public class UIColorEditor : IColorLibraryTab {

        private class AlphaColorData {

            public NoAlphaColorSO mainColor;
            public List<ColorAlphaVariationSO> variations = new();

            public AlphaColorData(NoAlphaColorSO mainColor) {
                this.mainColor = mainColor;
            }
        }

        private enum DetailsViewMode {
            Styles,
            AlphaColors
        }

        private const int kButtonIconSize = 10;
        private const string kButtonStyleNameSuffix = "ColorStyle";
        private const string kColorStyleDirectory =
            "Packages/com.beatgames.beatsaber.tours.core/UI/Common/SO/ColorStyles";
        private const string kAlphaColorDirectory =
            "Packages/com.beatgames.beatsaber.tours.core/UI/Common/SO/AlphaSeparatedColors/BaseColors";
        private const string kAlphaVariationDirectory =
            "Packages/com.beatgames.beatsaber.tours.core/UI/Common/SO/AlphaSeparatedColors/AlphaVariations";

        private static readonly Color kListHighlightColor = new(1.25f, 1.25f, 1.25f);

        private DetailsViewMode _viewMode = DetailsViewMode.Styles;
        private string[] _viewModeNames;

        private List<ColorStyleSO> _styles = new();
        private readonly List<ColorStyleSO> _selectedStyles = new();
        private List<AlphaColorData> _alphaColors = new();
        private AlphaColorData _selectedAlphaColor;

        private List<AlphaSO> _alphas = new();
        private int _alphaDropdownSelected = 0;

        private Vector2 _listScrollPosition;
        private Vector2 _detailsScrollPosition;
        private readonly Dictionary<object, string> _displayNames = new();
        private readonly Dictionary<object, Texture> _textureIcons = new();

        private GUIStyle _headerStyle;
        private GUIStyle headerStyle => _headerStyle ?? new GUIStyle(EditorStyles.boldLabel) { fontSize = 12 };

        private readonly Regex _camelCaseSplitRegex = new (@"(\p{Ll}(?=[\p{Lu}0-9])|\p{Lu}(?=\p{Lu}\p{Ll}|[0-9])|[0-9](?=\p{L}))");

        public void Initialize() {

            _viewModeNames = Enum.GetNames(typeof(DetailsViewMode))
                .Select(name => _camelCaseSplitRegex.Replace(name, "$1 "))
                .ToArray();

            RefreshCache();
        }

        public void OnGUI() {

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.Width(350));
            DrawList();
            EditorGUILayout.EndVertical();

            GUILayout.Space(5);

            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            DrawDetails();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawList() {

            _viewMode = (DetailsViewMode)GUILayout.Toolbar((int)_viewMode, _viewModeNames);

            switch (_viewMode) {
                case DetailsViewMode.Styles:
                    DrawStylesList();
                    break;

                case DetailsViewMode.AlphaColors:
                    DrawAlphaColorsList();
                    break;
            }
        }

        private void DrawStylesList() {

            DrawListHeader("Add new style", CreateNewColorStyle);

            _listScrollPosition = EditorGUILayout.BeginScrollView(_listScrollPosition);

            foreach (var colorStyleSo in _styles) {
                string buttonText = GetOrParseDisplayName(colorStyleSo);
                var texture = GetOrCreateIconTexture(colorStyleSo, forceUpdate: false);

                EditorGUILayout.BeginHorizontal();
                Color originalBackgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = _selectedStyles.Contains(colorStyleSo) ? kListHighlightColor : Color.white;
                if (GUILayout.Button(new GUIContent($" {buttonText}", texture))) {
                    if (!Event.current.control) {
                        _selectedStyles.Clear();
                    }

                    _selectedStyles.Add(colorStyleSo);
                }

                GUI.backgroundColor = originalBackgroundColor;

                if (GUILayout.Button("-", GUILayout.Width(20))) {
                    EditorApplication.delayCall += () => DeleteColorStyle(colorStyleSo);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.LabelField("Hold control to select multiple styles", EditorStyles.boldLabel);
        }

        private void DrawAlphaColorsList() {

            DrawListHeader("Add new alpha color", CreateNewAlphaColor);

            _listScrollPosition = EditorGUILayout.BeginScrollView(_listScrollPosition);

            foreach (var alphaColorData in _alphaColors) {
                string buttonText = GetOrParseDisplayName(alphaColorData.mainColor);
                var texture = GetOrCreateIconTexture(alphaColorData.mainColor, forceUpdate: false);

                EditorGUILayout.BeginHorizontal();
                Color originalBackgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = _selectedAlphaColor == alphaColorData ? kListHighlightColor : Color.white;
                if (GUILayout.Button(new GUIContent($" {buttonText}", texture))) {
                    _selectedAlphaColor = alphaColorData;
                }
                GUI.backgroundColor = originalBackgroundColor;

                if (GUILayout.Button("-", GUILayout.Width(20))) {
                    EditorApplication.delayCall += () => DeleteAlphaColor(alphaColorData);
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawListHeader(string text, Action onCreateNewCallback) {

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(text, headerStyle);
            if (GUILayout.Button("+", GUILayout.Width(20))) {
                EditorApplication.delayCall += () => onCreateNewCallback?.Invoke();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDetails() {

            switch (_viewMode) {
                case DetailsViewMode.Styles:
                    DrawSelectedStyle();
                    break;
                case DetailsViewMode.AlphaColors:
                    DrawSelectedAlphaColor();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
                EditorGUILayout.ObjectField(labelText, selectedStyle, typeof(ColorStyleSO), allowSceneObjects: false);
            }

            GUI.enabled = true;

            SerializedObject serializedObject = new SerializedObject(_selectedStyles.ToArray());
            SerializedProperty serializedProperty = serializedObject.FindProperty("_colorStyle");
            ColorStyleEditor.DrawColorStyle(serializedProperty, showGradientProperties: true, showHeader: false);

            bool hasChanged = serializedObject.ApplyModifiedProperties();
            if (hasChanged) {
                foreach (var selectedStyle in _selectedStyles) {
                    GetOrCreateIconTexture(selectedStyle, forceUpdate: true);
                }
            }
        }

        private void DrawSelectedAlphaColor() {

            if (_selectedAlphaColor == null) {
                EditorGUILayout.LabelField("No alpha color selected.");
                return;
            }

            var noAlphaColorSo = _selectedAlphaColor.mainColor;

            string headerText = GetOrParseDisplayName(noAlphaColorSo);
            EditorGUILayout.LabelField(headerText, headerStyle);

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Reference", noAlphaColorSo, typeof(NoAlphaColorSO), allowSceneObjects: false);
            GUI.enabled = true;

            SerializedObject noAlphaColorObject = new SerializedObject(noAlphaColorSo);
            SerializedProperty noAlphaColorProperty = noAlphaColorObject.FindProperty("_color");
            EditorGUILayout.PropertyField(noAlphaColorProperty, includeChildren: false);

            bool noAlphaColorSoUpdated = noAlphaColorObject.ApplyModifiedProperties();

            if (noAlphaColorSoUpdated) {
                GetOrCreateIconTexture(_selectedAlphaColor.mainColor, forceUpdate: true);
            }

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Variations", headerStyle);
            if (_selectedAlphaColor.variations.Count == 0) {
                EditorGUILayout.LabelField("None");
            }
            else {
                ColorAlphaVariationSO variationToRemove = null;

                foreach (var variationSo in _selectedAlphaColor.variations) {
                    SerializedObject variationObject = new SerializedObject(variationSo);
                    SerializedProperty alphaProperty = variationObject.FindProperty("_alpha");

                    EditorGUILayout.BeginHorizontal();
                    int previewWidth = 10;
                    var previewRect = EditorGUILayout.GetControlRect(
                        GUILayout.Width(previewWidth)
                    );

                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(alphaProperty, GUIContent.none, includeChildren: false);
                    GUI.enabled = true;

                    if (GUILayout.Button("Remove")) {
                        variationToRemove = variationSo;
                    }
                    EditorGUILayout.EndHorizontal();

                    var alphaPreviewTexture = GetOrCreateTexture(
                        variationSo,
                        variationSo.color,
                        10,
                        10,
                        noAlphaColorSoUpdated
                    );
                    EditorGUI.DrawTextureTransparent(previewRect, alphaPreviewTexture);

                    variationObject.ApplyModifiedProperties();
                }

                if (variationToRemove != null) {
                    _selectedAlphaColor.variations.Remove(variationToRemove);
                    AssetDatabase.DeleteAsset(variationToRemove.GetAssetPath());
                    AssetDatabase.Refresh();
                }
            }

            List<AlphaSO> unusedAlphas = new(_alphas);
            _selectedAlphaColor.variations.ForEach(so => unusedAlphas.Remove(so.alpha));

            if (unusedAlphas.Count != 0) {
                string[] unusedAlphaStrings = unusedAlphas.Select(GetOrParseDisplayName).ToArray();
                EditorGUILayout.BeginHorizontal();
                _alphaDropdownSelected = Math.Clamp(_alphaDropdownSelected, 0, unusedAlphas.Count - 1);

                _alphaDropdownSelected = EditorGUILayout.Popup(_alphaDropdownSelected, unusedAlphaStrings);
                if (GUILayout.Button("Add new variant")) {
                    AlphaSO alphaSo = unusedAlphas[_alphaDropdownSelected];
                    CreateNewAlphaVariation(alphaSo);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void CreateNewColorStyle() {

            bool isSuccessful = TextInputModalDialog.Show(
                "Create new color style",
                "Color style name",
                IsColorStyleNameValid,
                out string colorStyleName
            );

            if (!isSuccessful) {
                return;
            }

            var newColorStyle = ScriptableObject.CreateInstance<ColorStyleSO>();
            newColorStyle.name = GetStyleAssetName(colorStyleName);

            _styles.Add(newColorStyle);
            _styles = _styles.OrderBy(colorStyle => colorStyle.name).ToList();

            AssetDatabase.CreateAsset(newColorStyle, Path.Combine(kColorStyleDirectory, $"{newColorStyle.name}.asset"));
            AssetDatabase.Refresh();

            _selectedStyles.Clear();
            _selectedStyles.Add(newColorStyle);
        }

        private bool IsColorStyleNameValid(string name) {

            string assetName = GetStyleAssetName(name);
            return !_styles.Any(
                so => assetName.Equals(so.name, StringComparison.InvariantCultureIgnoreCase)
            );
        }

        private void DeleteColorStyle(ColorStyleSO colorStyleSo) {

            bool proceedDelete = EditorUtility.DisplayDialog(
                title: "Delete color style?",
                message: $"Are you sure you want to delete {colorStyleSo.name}?",
                ok: "Confirm",
                cancel: "Cancel"
            );

            if (!proceedDelete) {
                return;
            }

            _styles.Remove(colorStyleSo);
            AssetDatabase.DeleteAsset(colorStyleSo.GetAssetPath());
            AssetDatabase.Refresh();

            if (_selectedStyles.Contains(colorStyleSo)) {
                _selectedStyles.Remove(colorStyleSo);
            }
        }

        private void CreateNewAlphaColor() {

            bool isSuccessful = TextInputModalDialog.Show(
                "Create new alpha color",
                "Alpha color name",
                IsAlphaColorNameValid,
                out string alphaColorName
            );

            if (!isSuccessful) {
                return;
            }

            var newAlphaColor = ScriptableObject.CreateInstance<NoAlphaColorSO>();
            newAlphaColor.name = GetAlphaAssetName(alphaColorName);

            var alphaColorData = new AlphaColorData(newAlphaColor);
            _alphaColors.Add(alphaColorData);
            _alphaColors = _alphaColors.OrderBy(data => data.mainColor.name).ToList();

            AssetDatabase.CreateAsset(newAlphaColor, Path.Combine(kAlphaColorDirectory, $"{newAlphaColor.name}.asset"));
            AssetDatabase.Refresh();

            _selectedAlphaColor = alphaColorData;
        }

        private bool IsAlphaColorNameValid(string name) {

            string assetName = GetAlphaAssetName(name);
            return !_alphaColors.Any(
                data => assetName.Equals(data.mainColor.name, StringComparison.InvariantCultureIgnoreCase)
            );
        }

        private void DeleteAlphaColor(AlphaColorData alphaColorData) {

            bool proceedDelete = EditorUtility.DisplayDialog(
                title: "Delete alpha color?",
                message: $"Are you sure you want to delete {alphaColorData.mainColor.name}?",
                ok: "Confirm",
                cancel: "Cancel"
            );

            if (!proceedDelete) {
                return;
            }

            _alphaColors.Remove(alphaColorData);
            AssetDatabase.DeleteAsset(alphaColorData.mainColor.GetAssetPath());
            foreach (var alphaVariation in alphaColorData.variations) {
                AssetDatabase.DeleteAsset(alphaVariation.GetAssetPath());
            }

            AssetDatabase.Refresh();

            if (_selectedAlphaColor == alphaColorData) {
                _selectedAlphaColor = null;
            }
        }

        private void CreateNewAlphaVariation(AlphaSO alphaSo) {

            var newVariation = ScriptableObject.CreateInstance<ColorAlphaVariationSO>();
            newVariation.name = $"{_selectedAlphaColor.mainColor.name}_{alphaSo.name}";
            newVariation.InitializeEditor(_selectedAlphaColor.mainColor, alphaSo);

            _selectedAlphaColor.variations.Add(newVariation);
            _selectedAlphaColor.variations = _selectedAlphaColor.variations.OrderBy(so => so.alpha.alphaValue).ToList();

            AssetDatabase.CreateAsset(newVariation, Path.Combine(kAlphaVariationDirectory, $"{newVariation.name}.asset"));
            AssetDatabase.Refresh();
        }

        public void RefreshCache() {

            _styles = AssetDatabase.FindAssets($"t:{typeof(ColorStyleSO)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ColorStyleSO>)
                .ToList();

            var alphaColorsDictionary = AssetDatabase.FindAssets($"t:{typeof(NoAlphaColorSO)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<NoAlphaColorSO>)
                .Select(so => new AlphaColorData(so))
                .ToDictionary(data => data.mainColor);

            var alphaVariations = AssetDatabase.FindAssets($"t:{typeof(ColorAlphaVariationSO)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ColorAlphaVariationSO>)
                .ToList();

            foreach (var alphaVariation in alphaVariations) {
                var baseColor = alphaVariation.baseColor;
                if (alphaColorsDictionary.TryGetValue(baseColor, out var colorData)) {
                    colorData.variations.Add(alphaVariation);
                }
            }

            _alphaColors = alphaColorsDictionary.Values.ToList();

            _alphas = AssetDatabase.FindAssets($"t:{typeof(AlphaSO)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<AlphaSO>)
                .ToList();
        }

        private Texture GetOrCreateIconTexture(ColorStyleSO colorStyleSo, bool forceUpdate = false) {

            if (!_textureIcons.ContainsKey(colorStyleSo) || forceUpdate) {
                var texture = new Texture2D(kButtonIconSize, kButtonIconSize, TextureFormat.RGBAFloat, false);
                if (colorStyleSo.colorStyle.gradient) {
                    float buttonSizeF = kButtonIconSize;
                    Color c1 = colorStyleSo.colorStyle.color0;
                    Color c2 = colorStyleSo.colorStyle.color1;
                    for (int y = 0; y < kButtonIconSize; y++) {
                        for (int x = 0; x < kButtonIconSize; x++) {
                            float gradientValue;
                            if (colorStyleSo.colorStyle.gradientDirection == GradientDirection.Horizontal) {
                                gradientValue = x / buttonSizeF;
                            }
                            else {
                                gradientValue = 1 - y / buttonSizeF;
                            }

                            if (colorStyleSo.colorStyle.flipGradientColors) {
                                gradientValue = 1 - gradientValue;
                            }

                            Color pixelColor = Color.Lerp(c1, c2, gradientValue);
                            texture.SetPixel(x, y, pixelColor);
                        }
                    }
                }
                else {
                    Color c = colorStyleSo.colorStyle.color;
                    for (int y = 0; y < kButtonIconSize; y++) {
                        for (int x = 0; x < kButtonIconSize; x++) {
                            texture.SetPixel(x, y, c);
                        }
                    }
                }
                texture.Apply();

                _textureIcons[colorStyleSo] = texture;
            }

            return _textureIcons[colorStyleSo];
        }

        private Texture GetOrCreateIconTexture(ColorSO colorSo, bool forceUpdate = false) {

            return GetOrCreateTexture(
                colorSo,
                colorSo.color,
                width: kButtonIconSize,
                height: kButtonIconSize,
                forceUpdate
            );
        }

        private Texture GetOrCreateTexture(object key, Color color, int width, int height, bool forceUpdate = false) {

            if (!_textureIcons.ContainsKey(key) || forceUpdate) {
                var texture = new Texture2D(width, height, TextureFormat.RGBAFloat, false);

                for (int y = 0; y < kButtonIconSize; y++) {
                    for (int x = 0; x < kButtonIconSize; x++) {
                        texture.SetPixel(x, y, color);
                    }
                }

                texture.Apply();
                _textureIcons[key] = texture;
            }

            return _textureIcons[key];
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

        private string GetAlphaAssetName(string displayName) => displayName.Replace(" ", "");
    }
}
