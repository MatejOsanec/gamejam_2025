namespace BGLib.ShaderInspector {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public class Feature : Element {

        public enum Style {
            Feature,
            SubFeature
        }

        private bool foldout;
        private readonly string _keyword;
        private readonly string _propertyName;
        private readonly string _displayName;
        private readonly Style _style;
        private readonly string _tooltip;
        private readonly string _description;
        private readonly string _documentationUrl;
        private readonly string _documentationButtonLabel;
        private readonly List<Element> _childElements;
        private readonly Color _backgroundColor;
        private readonly string _foldoutSessionKey;

        public Feature(
            string keyword,
            string propertyName,
            Style style = Style.Feature,
            string displayName = null,
            string tooltip = null,
            string description = null,
            string documentationUrl = null,
            string documentationButtonLabel = null,
            List<Element> childElements = null,
            Color? backgroundColor = null,
            DisplayFilter displayFilter = null,
            DisplayFilter enabledFilter = null
        ) : base(displayFilter, enabledFilter) {


            _keyword = keyword;
            _propertyName = propertyName;
            _style = style;
            _displayName = displayName;
            _description = description;
            _documentationUrl = documentationUrl;
            _documentationButtonLabel = documentationButtonLabel;
            _tooltip = ShaderInspectorHelper.BuildTooltip(tooltip, propertyName, keyword);
            _childElements = childElements;
            if (backgroundColor.HasValue) {
                _backgroundColor = backgroundColor.Value;
            }
            else if (style == Style.Feature) {
                _backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.10f);
            }
            else if (style == Style.SubFeature) {
                _backgroundColor = new Color(0.0f, 0.5f, 1.0f, 0.0f);
            }

            _foldoutSessionKey = string.IsNullOrEmpty(_keyword) ? _propertyName : _keyword;
        }

        public override void OnGUI(
            MaterialEditor materialEditor,
            MaterialProperty[] properties,
            string searchString,
            ShaderInspector.PresetsData presetsData,
            bool parentDisabled
        ) {

            if (!ShouldBeDrawn(properties, searchString)) {
                return;
            }

            if (_style == Style.Feature) {
                EditorGUILayout.Space(kDefaultVerticalSpacingBeforeFeatures);
            }

            MaterialProperty property = ShaderInspector.FindProperty(_propertyName, properties);
            if (property == null) {
                GUILayout.Label($"Trying to draw non-existing feature-property \"{_propertyName}\"", ShaderInspectorLayout.errorLabelStyle);
                return;
            }
            if (property.type != MaterialProperty.PropType.Float) {
                GUILayout.Label($"Trying to use non-int/float property \"{_propertyName}\" as a Feature toggle", ShaderInspectorLayout.errorLabelStyle);
                return;
            }

            var isDisabledByPreset = presetsData.IsDisabledByPresets(property, out var disablingPresetName);
            var state = ShaderInspectorHelper.GetKeywordState(_keyword, property, properties);
            MaterialEditor.BeginProperty(property);

            foldout = SessionState.GetBool(_foldoutSessionKey, state != KeywordState.Disabled);
            var displayName = !string.IsNullOrEmpty(_displayName) ? _displayName : property.displayName;
            if (_style == Style.Feature) {
                using (var feature = new ShaderInspectorLayout.Feature(displayName, disablingPresetName, parentDisabled || ShouldBeDisabled(properties), _tooltip, state, foldout, _backgroundColor)) {
                    var newValue = feature.foldout;
                    if (feature.state != KeywordState.Mixed) {
                        var isEnabled = feature.state == KeywordState.Enabled;
                        property.floatValue = isEnabled ? 1 : 0;
                        ShaderInspectorHelper.SetKeywordEnabled(_keyword, property, isEnabled);
                    }

                    if (newValue != foldout) {
                        foldout = newValue;
                        SessionState.SetBool(_foldoutSessionKey, foldout);
                    }
                    MaterialEditor.EndProperty();

                    if (foldout) {
                        using (new EditorGUI.DisabledGroupScope(parentDisabled || ShouldBeDisabled(properties))) {
                            ShaderInspectorLayout.Description(_description, _documentationUrl, _documentationButtonLabel);
                        }
                    }
                    if (foldout && _childElements != null) {
                        using (new EditorGUI.DisabledGroupScope(feature.state == KeywordState.Disabled)) {
                            foreach (var shaderInspectorElement in _childElements) {
                                shaderInspectorElement.OnGUI(materialEditor, properties, searchString, presetsData, parentDisabled || ShouldBeDisabled(properties));
                            }
                        }
                    }
                }
            } else if (_style == Style.SubFeature) {
                using (var subFeature = new ShaderInspectorLayout.SubFeature(displayName, disablingPresetName, parentDisabled || ShouldBeDisabled(properties), _tooltip, state, _backgroundColor)) {
                    if (subFeature.state != KeywordState.Mixed) {
                        var isEnabled = subFeature.state == KeywordState.Enabled;
                        property.floatValue = isEnabled ? 1 : 0;
                        ShaderInspectorHelper.SetKeywordEnabled(_keyword, property, isEnabled);
                    }
                    MaterialEditor.EndProperty();

                    EditorGUI.indentLevel++;
                    if (subFeature.state != KeywordState.Disabled) {
                        using (new EditorGUI.DisabledGroupScope(parentDisabled || ShouldBeDisabled(properties))) {
                            ShaderInspectorLayout.Description(_description, _documentationUrl, _documentationButtonLabel);
                        }

                        if (_childElements != null) {
                            foreach (var shaderInspectorElement in _childElements) {
                                shaderInspectorElement.OnGUI(materialEditor, properties, searchString, presetsData, parentDisabled || ShouldBeDisabled(properties));
                            }
                        }
                    }
                    EditorGUI.indentLevel--;
                }
            }

            if (_style == Style.Feature) {
                EditorGUILayout.Space(kDefaultVerticalSpacingAfterFeatures);
            }
        }

        public override void MarkUsedMaterialPropertiesSelfOnly(HashSet<MaterialProperty> usedMaterialProperties, MaterialProperty[] properties) {

            MaterialProperty property = ShaderInspector.FindProperty(_propertyName, properties);
            if (property == null) {
                return;
            }
            usedMaterialProperties.Add(property);
        }

        public override (int enabledKeywordsCount, bool anyMixed) GetEnabledKeywordsCount(MaterialProperty[] properties) {

            if (!ShouldBeDrawn(properties, searchString: null)) {
                return (enabledKeywordsCount: 0, anyMixed: false);
            }
            var thisFeatureState = ShaderInspectorHelper.GetKeywordState(_keyword, _propertyName, properties);
            var enabledKeywordsCount = thisFeatureState == KeywordState.Enabled ? 1 : 0;
            var anyMixed = thisFeatureState == KeywordState.Mixed;
            if (_childElements == null || thisFeatureState != KeywordState.Enabled) {
                return (enabledKeywordsCount, anyMixed);
            }

            foreach (var childElement in _childElements) {
                var childResult = childElement.GetEnabledKeywordsCount(properties);
                enabledKeywordsCount += childResult.enabledKeywordsCount;
                anyMixed |= childResult.anyMixed;
            }
            return (enabledKeywordsCount, anyMixed);
        }

        public override IEnumerable<(Element element, bool isActive, Element parentElement)> EnumerateSelfAndChildElementsRecursively(
            bool isParentActive,
            MaterialProperty[] properties,
            Element parentElement
        ) {

            var keywordState = ShaderInspectorHelper.GetKeywordState(_keyword, _propertyName, properties);
            var isEnabled = keywordState == KeywordState.Enabled;
            var isSelfActive = isParentActive && isEnabled && ShouldBeDrawn(properties, searchString: null) &&
                               !ShouldBeDisabled(properties);
            yield return (this, isSelfActive, parentElement);
            if (_childElements == null) {
                yield break;
            }

            foreach (var childElement in _childElements) {
                foreach (var element in childElement.EnumerateSelfAndChildElementsRecursively(isSelfActive, properties, this)) {
                    yield return element;
                }
            }
        }

        public override bool ShouldBeDrawnWithSearchString(MaterialProperty[] properties, string searchString) {

            MaterialProperty property = ShaderInspector.FindProperty(_propertyName, properties);
            return _propertyName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                   (!string.IsNullOrEmpty(_keyword) && _keyword.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                   (property?.displayName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                   (_childElements != null && _childElements.Any(element => element.ShouldBeDrawnWithSearchString(properties, searchString)));
        }

        public override void ForceExpand() {

            foldout = true;
            SessionState.SetBool(_foldoutSessionKey, foldout);
        }
    }

    // Just a shortcut for writing Sub Features
    public class SubFeature : Feature {

        public SubFeature(
            string keyword,
            string propertyName,
            string displayName = null,
            string tooltip = null,
            string description = null,
            string documentationUrl = null,
            string documentationButtonLabel = null,
            List<Element> childElements = null,
            Color? backgroundColor = null,
            DisplayFilter displayFilter = null,
            DisplayFilter enabledFilter = null
        ) : base(
            keyword,
            propertyName,
            Style.SubFeature,
            displayName,
            tooltip,
            description,
            documentationUrl,
            documentationButtonLabel,
            childElements,
            backgroundColor,
            displayFilter,
            enabledFilter
        ) { }
    }



    // Just a shortcut for writing Sub Features
    public class NoKeywordFeature : Feature {

        public NoKeywordFeature(
            string propertyName,
            Style style = Style.Feature,
            string displayName = null,
            string tooltip = null,
            string description = null,
            string documentationUrl = null,
            string documentationButtonLabel = null,
            List<Element> childElements = null,
            Color? backgroundColor = null,
            DisplayFilter displayFilter = null,
            DisplayFilter enabledFilter = null
        ) : base(
            keyword: null,
            propertyName,
            style,
            displayName,
            tooltip,
            description,
            documentationUrl,
            documentationButtonLabel,
            childElements,
            backgroundColor,
            displayFilter,
            enabledFilter
        ) { }
    }
}
