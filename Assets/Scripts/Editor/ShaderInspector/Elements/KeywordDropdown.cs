namespace BGLib.ShaderInspector {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public class KeywordDropdown : Element {

        public enum Style {
            Feature,
            SubFeature
        }

        public class Option {

            public readonly string keyword;
            public readonly bool countsTowardsKeywordCount;
            public readonly string displayName;
            public readonly string description;
            public readonly string documentationUrl;
            public readonly string documentationButtonLabel;
            public readonly List<Element> childElements;
            public readonly DisplayFilter displayFilter;
            // Used when the option is selected but filtered out by display filter
            public readonly string displayFilterErrorMessage;


            public Option(
                string keyword,
                string displayName,
                string description = null,
                string documentationUrl = null,
                string documentationButtonLabel = null,
                bool countsTowardsKeywordCount = true,
                List<Element> childElements = null,
                DisplayFilter displayFilter = null,
                string displayFilterErrorMessage = null
            ) {

                this.keyword = keyword;
                this.displayName = displayName;
                this.description = description;
                this.documentationUrl = documentationUrl;
                this.documentationButtonLabel = documentationButtonLabel;
                this.countsTowardsKeywordCount = countsTowardsKeywordCount;
                this.childElements = childElements;
                this.displayFilter = displayFilter;
                this.displayFilterErrorMessage = displayFilterErrorMessage;
            }
        }

        private readonly string _propertyName;
        private readonly Style _style;
        private readonly string _displayName;
        private readonly bool _countsTowardsKeywordCount;
        private readonly string _description;
        private readonly string _documentationUrl;
        private readonly string _documentationButtonLabel;
        private readonly IReadOnlyList<Option> _options;
        private string _tooltip;
        private readonly string _customTooltip;
        private readonly string[] _displayOptions;
        private readonly List<Element> _childElements;
        private readonly Color? _backgroundColor;

        private readonly List<Option> _reusableDisplayFilteredOptions = new List<Option>();
        private bool _foldout;
        // A bit hacky way how to provide info box to info box gathering logic
        private int _invalidIndexCachedInfoBox = -1;
        private InfoBox _invalidIndexSelectedInfoBox;

        public KeywordDropdown(
            string propertyName,
            Style style,
            IReadOnlyList<Option> options,
            string displayName = null,
            string tooltip = null,
            string description = null,
            string documentationUrl = null,
            string documentationButtonLabel = null,
            bool? countsTowardsKeywordCount = null,
            List<Element> childElements = null,
            Color? backgroundColor = null,
            DisplayFilter displayFilter = null,
            DisplayFilter enabledFilter = null
        ) : base(displayFilter, enabledFilter) {

            _propertyName = propertyName;
            _style = style;
            _options = options;
            _displayName = displayName;
            _customTooltip = tooltip;
            _description = description;
            _documentationUrl = documentationUrl;
            _documentationButtonLabel = documentationButtonLabel;
            _countsTowardsKeywordCount = countsTowardsKeywordCount ?? style == Style.Feature;
            _childElements = childElements;
            _backgroundColor = backgroundColor;
            _displayOptions = options.Select(option => option.displayName).ToArray();

            _foldout = SessionState.GetBool(_propertyName, false);
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
                GUILayout.Label($"Trying to draw non-existing property \"{_propertyName}\"", ShaderInspectorLayout.errorLabelStyle);
                return;
            }
            if (property.type != MaterialProperty.PropType.Float) {
                GUILayout.Label($"Trying to use non-int/float property \"{_propertyName}\" as a Feature toggle", ShaderInspectorLayout.errorLabelStyle);
                return;
            }

            var selectedIndex = Mathf.RoundToInt(property.floatValue);
            selectedIndex = Mathf.Clamp(selectedIndex, 0, _options.Count - 1);
            var isDisabledByPreset = presetsData.IsDisabledByPresets(property, out var disablingPresetName);
            if (_tooltip == null) {
                RefreshTooltip(selectedIndex);
            }
            AlignKeywordAndProperty(selectedIndex, property, properties);

            var displayName = !string.IsNullOrEmpty(_displayName) ? _displayName : property.displayName;
            MaterialEditor.BeginProperty(property);

            // Apply Options Display Filter
            var (filteredOptions, filteredOptionsDisplayNames, filteredSelectedIndex, isFilteredIndexValid) =
                GetDisplayFilteredOptionsWithSelectedIndex(selectedIndex, properties);

            if (_style == Style.SubFeature) {
                var shouldBeDisabled = ShouldBeDisabled(properties);
                using (new EditorGUI.DisabledScope(isDisabledByPreset || parentDisabled || shouldBeDisabled)) {
                    var newIndex = EditorGUILayout.Popup(displayName, filteredSelectedIndex, filteredOptionsDisplayNames);
                    if (filteredSelectedIndex != newIndex) {
                        ApplyNewSelectedFilteredIndex(filteredOptions, newIndex);
                    }
                }
                MaterialEditor.EndProperty();

                var rect = GUILayoutUtility.GetLastRect();
                ShaderInspectorLayout.ShowTooltipIfHoverWithDisablingPreset(rect, _tooltip, disablingPresetName);

                if (!isFilteredIndexValid && !shouldBeDisabled && !parentDisabled) {
                    DrawInvalidSelectedIndexWarning(filteredOptions[filteredSelectedIndex]);
                }
                DrawChildElements();
            }
            else if (_style == Style.Feature) {
                var state = ShaderInspectorHelper.GetKeywordState(_options[selectedIndex].keyword, property, properties);
                using (var dropdown = new ShaderInspectorLayout.FeatureDropdown(displayName, _tooltip, state, filteredSelectedIndex, _foldout, disablingPresetName, parentDisabled || ShouldBeDisabled(properties), filteredOptionsDisplayNames, _backgroundColor)) {
                    _foldout = dropdown.foldout;
                    if (filteredSelectedIndex != dropdown.selectedIndex || (state == KeywordState.Mixed && dropdown.state != KeywordState.Mixed)) {
                        ApplyNewSelectedFilteredIndex(filteredOptions, dropdown.selectedIndex);
                    }

                    if (!isFilteredIndexValid && state == KeywordState.Enabled && !parentDisabled) {
                        DrawInvalidSelectedIndexWarning(filteredOptions[filteredSelectedIndex]);
                    }

                    MaterialEditor.EndProperty();
                    SessionState.SetBool(_propertyName, _foldout);

                    if (_foldout) {
                        DrawChildElements();
                    }
                }
            }

            void DrawInvalidSelectedIndexWarning(Option selectedOption) {

                if (!string.IsNullOrEmpty(selectedOption.displayFilterErrorMessage)) {
                    EditorGUILayout.HelpBox($"Selected option \"{selectedOption.displayName}\" is no longer available. \n{selectedOption.displayFilterErrorMessage}", MessageType.Error);
                }
                else {
                    EditorGUILayout.HelpBox($"Selected option \"{selectedOption.displayName}\" is no longer available. \nYou must have changed its prerequisite.", MessageType.Error);
                }
                if (_invalidIndexSelectedInfoBox != null) {
                    _invalidIndexSelectedInfoBox.lastRectDrawnInto = GUILayoutUtility.GetLastRect();
                }

            }

            void ApplyNewSelectedFilteredIndex(IReadOnlyList<Option> displayFilteredOptions, int newFilteredIndex) {

                ApplyNewSelectedIndex(IndexOf(_options,displayFilteredOptions[newFilteredIndex]));
            }

            static int IndexOf<T>(IReadOnlyList<T> self, T item) {

                var i = 0;
                foreach (T element in self) {
                    if (Equals(element, item)) {
                        return i;
                    }
                    i++;
                }
                return -1;
            }

            void ApplyNewSelectedIndex(int newIndex) {

                selectedIndex = newIndex;
                property.floatValue = newIndex;
                SetKeywords(property, selectedIndex);
                RefreshTooltip(selectedIndex);
            }

            // Shader Between Styles
            void DrawChildElements() {

                EditorGUI.indentLevel++;
                using (new EditorGUI.DisabledGroupScope(parentDisabled || ShouldBeDisabled(properties))) {
                    ShaderInspectorLayout.Description(_description, _documentationUrl, _documentationButtonLabel);
                }

                var selectedOption = _options[selectedIndex];
                ShaderInspectorLayout.Description(selectedOption.description, selectedOption.documentationUrl, selectedOption.documentationButtonLabel);

                // Self child elements
                if (_childElements != null) {
                    foreach (var shaderInspectorElement in _childElements) {
                        shaderInspectorElement.OnGUI(materialEditor, properties, searchString, presetsData, parentDisabled || ShouldBeDisabled(properties));
                    }
                }

                // Options child elements
                if (selectedOption.childElements != null) {
                    foreach (var shaderInspectorElement in selectedOption.childElements) {
                        shaderInspectorElement.OnGUI(materialEditor, properties, searchString, presetsData, parentDisabled || ShouldBeDisabled(properties));
                    }
                }
                EditorGUI.indentLevel--;
            }

            if (_style == Style.Feature) {
                EditorGUILayout.Space(kDefaultVerticalSpacingAfterFeatures);
            }
        }

        private void RefreshTooltip(int selectedOptionIndex) {

            var keyword = _options[selectedOptionIndex].keyword;
            _tooltip = ShaderInspectorHelper.BuildTooltip(_customTooltip, _propertyName, keyword);
        }

        private void SetKeywords(MaterialProperty prop, int optionIndex) {

            // Backwards compatibility, disabling <>_NONE keywords that are no longer used
            foreach (Material target in prop.targets) {
                // Taken from MaterialEnumShowIfDrawer.cs
                var keyword = (prop.name + "_NONE").Replace(' ', '_').ToUpperInvariant();
                target.DisableKeyword(keyword);
            }

            for (int i = 0; i < _options.Count; i++) {
                var keyword = _options[i].keyword;
                if (string.IsNullOrEmpty(keyword)) {
                    continue;
                }

                foreach (Material target in prop.targets) {
                    if (optionIndex == i) {
                        target.EnableKeyword(keyword);
                    }
                    else {
                        target.DisableKeyword(keyword);
                    }
                }
            }
        }

        // Filters out options based on their display filter, returns filtered out dropdown and adjusted selection index
        // If option filtered out is currently selected, it includes it into results anyway but flags isCurrentSelectionValid as false
        private (IReadOnlyList<Option> displayFilteredOptions, string[] displayFilteredOptionNames, int displayFilteredIndex, bool isCurrentSelectionValid)
            GetDisplayFilteredOptionsWithSelectedIndex(int selectedIndex, MaterialProperty[] materialProperties) {

            if (_options.All(option => option.displayFilter == null)) {
                return (_options, _displayOptions, selectedIndex, true);
            }

            _reusableDisplayFilteredOptions.Clear();
            var adjustedSelectedIndex = -1;
            var includedFilteredOutOption = false;

            for (int i = 0; i < _options.Count; i++) {
                var option = _options[i];
                var shouldBeIncluded = option.displayFilter == null || option.displayFilter.Invoke(materialProperties);
                var mustBeIncluded = i == selectedIndex;
                if (i == selectedIndex) {
                    adjustedSelectedIndex = _reusableDisplayFilteredOptions.Count;
                }

                if (shouldBeIncluded || mustBeIncluded) {
                    _reusableDisplayFilteredOptions.Add(option);
                }
                if (mustBeIncluded && !shouldBeIncluded) {
                    includedFilteredOutOption = true;
                }
            }

            return (_reusableDisplayFilteredOptions,
                // I'm aware of this might be generating a lot of garbage, I haven't figured out better way of doing this (we could cache it if nothing changes)
                _reusableDisplayFilteredOptions.Select(option => option.displayName).ToArray(),
                adjustedSelectedIndex,
                !includedFilteredOutOption);
        }

        public override void MarkUsedMaterialPropertiesSelfOnly(HashSet<MaterialProperty> usedMaterialProperties, MaterialProperty[] properties) {

            MaterialProperty property = ShaderInspector.FindProperty(_propertyName, properties);
            if (property == null) {
                return;
            }
            usedMaterialProperties.Add(property);

            // A bit hacky way how to provide error info box to info box gathering logic outside of OnGUI
            var selectedIndex = Mathf.RoundToInt(property.floatValue);
            var (displayFilteredOptions, filteredOptionsDisplayNames, filteredSelectedIndex, isFilteredIndexValid) =
                GetDisplayFilteredOptionsWithSelectedIndex(selectedIndex, properties);
            if (!isFilteredIndexValid) {
                if (_invalidIndexCachedInfoBox != selectedIndex) {
                    _invalidIndexCachedInfoBox = selectedIndex;
                    var selectedOption = displayFilteredOptions[filteredSelectedIndex];
                    if (!string.IsNullOrEmpty(selectedOption.displayFilterErrorMessage)) {
                        _invalidIndexSelectedInfoBox = new InfoBox(selectedOption.displayFilterErrorMessage, MessageType.Error);
                    }
                    else {
                        var displayName = !string.IsNullOrEmpty(_displayName) ? _displayName : property.displayName;
                        _invalidIndexSelectedInfoBox = new InfoBox($"\"{displayName}\" dropdown has selected option \"{filteredOptionsDisplayNames[filteredSelectedIndex]}\" that should not be available because of display filter", MessageType.Error);
                    }
                }
            }
            else {
                _invalidIndexCachedInfoBox = -1;
                _invalidIndexSelectedInfoBox = null;
            }
        }

        public override (int enabledKeywordsCount, bool anyMixed) GetEnabledKeywordsCount(MaterialProperty[] properties) {

            if (!ShouldBeDrawn(properties, searchString: null)) {
                return (enabledKeywordsCount: 0, anyMixed: false);
            }
            MaterialProperty property = ShaderInspector.FindProperty(_propertyName, properties);
            var selectedIndex = Mathf.RoundToInt(property.floatValue);
            selectedIndex = Mathf.Clamp(selectedIndex, 0, _displayOptions.Length - 1);
            var selectedOption = _options[selectedIndex];

            var enabledKeywordsCount = 0;
            var anyMixed = false;
            if (_countsTowardsKeywordCount && selectedOption.countsTowardsKeywordCount && !string.IsNullOrEmpty(selectedOption.keyword)) {
                var state = ShaderInspectorHelper.GetKeywordState(selectedOption.keyword, _propertyName, properties);
                enabledKeywordsCount = state == KeywordState.Enabled ? 1 : 0;
                anyMixed = state == KeywordState.Mixed;
            }
            if (selectedOption.childElements == null) {
                return (enabledKeywordsCount, anyMixed);
            }
            foreach (var childElement in selectedOption.childElements) {
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

            yield return (this, isParentActive, parentElement);
            var isSelfActive = ShouldBeDrawn(properties, searchString: null) && !ShouldBeDisabled(properties);
            if (_invalidIndexSelectedInfoBox != null) {
                yield return (_invalidIndexSelectedInfoBox, isSelfActive && isParentActive, this);
            }
            // Self child elements
            if (_childElements != null) {
                foreach (var childElement in _childElements) {
                    foreach (var element in childElement.EnumerateSelfAndChildElementsRecursively(isParentActive && isSelfActive, properties, this)) {
                        yield return element;
                    }
                }
            }
            var selectedOptionIndex = -1;
            MaterialProperty property = ShaderInspector.FindProperty(_propertyName, properties);
            if (property != null) {
                selectedOptionIndex = Mathf.RoundToInt(property.floatValue);
            }
            // Options child elements
            for (int i = 0; i < _options.Count; i++) {
                var option = _options[i];
                if (option.childElements == null) {
                    continue;
                }

                var isSelectedOption = selectedOptionIndex == i;
                foreach (var childElement in option.childElements) {
                    foreach (var element in childElement.EnumerateSelfAndChildElementsRecursively(isParentActive && isSelfActive && isSelectedOption, properties, this)) {
                        yield return element;
                    }
                }
            }
        }

        public override bool ShouldBeDrawnWithSearchString(MaterialProperty[] properties, string searchString) {

            MaterialProperty property = ShaderInspector.FindProperty(_propertyName, properties);
            return _propertyName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                   (property?.displayName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                   _options.Any(option =>
                       option.displayName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                       option.keyword.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                       (option.childElements != null &&
                        option.childElements.Any(optionChild => optionChild.ShouldBeDrawnWithSearchString(properties, searchString))));
        }

        public void KeywordChangedExternally(string propertyName, string keyword, bool isEnabled, MaterialProperty[] properties) {

            if (_propertyName != propertyName) {
                return;
            }
            var index = FindIndex(_options, option => option.keyword == keyword);
            var hasKeyword = index > -1;

            if (!hasKeyword) {
                return;
            }
            MaterialProperty property = ShaderInspector.FindProperty(_propertyName, properties);
            // Not enabled, going to default index
            if (!isEnabled) {
                index = 0;
            }
            property.floatValue = index;
            SetKeywords(property, index);
            RefreshTooltip(index);
        }

        public static int FindIndex<T>(IReadOnlyList<T> list, Predicate<T> match) {

            for (int index = 0; index < list.Count; index++) {
                if (match(list[index])) {
                    return index;
                }
            }
            return -1;
        }




        // Check if there is a mismatch between applied keyword and propertyIndex
        private void AlignKeywordAndProperty(int selectedIndex, MaterialProperty property, MaterialProperty[] properties) {

            var keywordFromSelectedIndex = _options[selectedIndex].keyword;
            var state = ShaderInspectorHelper.GetKeywordState(keywordFromSelectedIndex, property, properties);
            if (state == KeywordState.Mixed) {
                return;
            }
            var keywordFromSelectedIndexShouldBeEnabled = string.IsNullOrEmpty(keywordFromSelectedIndex);
            if ((state == KeywordState.Disabled && keywordFromSelectedIndexShouldBeEnabled) ||
                state == KeywordState.Enabled && !keywordFromSelectedIndexShouldBeEnabled) {

                SetKeywords(property, selectedIndex);
            }
        }

        public override void ForceExpand() {

            _foldout = true;
            SessionState.SetBool(_propertyName, _foldout);
        }
    }
}
