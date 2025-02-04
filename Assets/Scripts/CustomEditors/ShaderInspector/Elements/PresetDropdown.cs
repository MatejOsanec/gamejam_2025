namespace BGLib.ShaderInspector {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    public class PresetDropdown: Element {

        public class Option {

            public readonly string displayName;
            public readonly string description;
            public readonly string documentationUrl;
            public readonly string documentationButtonLabel;
            public readonly IReadOnlyList<PresetValue> presetValues;

            public Option(
                string displayName,
                string description = null,
                string documentationUrl = null,
                string documentationButtonLabel = null,
                IReadOnlyList<PresetValue> presetValues = null
            ) {

                this.displayName = displayName;
                this.description = description;
                this.documentationUrl = documentationUrl;
                this.documentationButtonLabel = documentationButtonLabel;
                this.presetValues = presetValues;
            }
        }

        public abstract class PresetValue {

            public readonly string propertyName;

            protected PresetValue(string propertyName) {

                this.propertyName = propertyName;
            }

            public virtual void Apply(
                MaterialEditor materialEditor,
                MaterialProperty[] properties,
                MaterialProperty requestingPresetProperty
            ) {

                MaterialProperty property = ShaderInspector.FindProperty(propertyName, properties);
                if (property == null) {
                    GUILayout.Label($"Trying to use a preset for non-existing property \"{propertyName}\"", ShaderInspectorLayout.errorLabelStyle);
                    return;
                }

                ApplyInternal(property);
            }

            protected abstract void ApplyInternal(MaterialProperty property);
        }

        public class FloatPropertyPreset: PresetValue {

            private readonly float value;

            public FloatPropertyPreset(string propertyName, float value) : base(propertyName) {

                this.value = value;
            }

            protected override void ApplyInternal(MaterialProperty property) {

                property.floatValue = value;
            }
        }

        public class IntPropertyPreset: PresetValue {

            private readonly int value;

            public IntPropertyPreset(string propertyName, int value) : base(propertyName) {

                this.value = value;
            }

            protected override void ApplyInternal(MaterialProperty property) {

                property.intValue = value;
            }
        }

        public class ColorPropertyPreset: PresetValue {

            private readonly Color value;

            public ColorPropertyPreset(string propertyName, Color value) : base(propertyName) {

                this.value = value;
            }

            protected override void ApplyInternal(MaterialProperty property) {

                property.colorValue = value;
            }
        }

        public class TexturePropertyPreset: PresetValue {

            private readonly Texture value;
            private readonly Vector4 scaleAndOffset;

            public TexturePropertyPreset(
                string propertyName,
                Texture value,
                Vector4 scaleAndOffset
            ) : base(propertyName) {

                this.value = value;
                this.scaleAndOffset = scaleAndOffset;
            }

            protected override void ApplyInternal(MaterialProperty property) {

                property.textureValue = value;
                property.textureScaleAndOffset = scaleAndOffset;
            }
        }

        public class VectorPropertyPreset: PresetValue {

            private readonly Vector4 value;

            public VectorPropertyPreset(string propertyName, Vector4 value) : base(propertyName) {

                this.value = value;
            }

            protected override void ApplyInternal(MaterialProperty property) {

                property.vectorValue = value;
            }
        }

        public class KeywordPreset: PresetValue {

            private readonly string keyword;
            private readonly bool enabled;

            public KeywordPreset(string propertyName, string keyword, bool enabled = true) : base(propertyName) {

                this.keyword = keyword;
                this.enabled = enabled;
            }

            public override void Apply(
                MaterialEditor materialEditor,
                MaterialProperty[] properties,
                MaterialProperty requestingPresetProperty
            ) {
                MaterialProperty property = ShaderInspector.FindProperty(propertyName, properties);
                if (property == null) {
                    GUILayout.Label($"Trying to use a preset for non-existing property \"{propertyName}\"", ShaderInspectorLayout.errorLabelStyle);
                    return;
                }

                ShaderInspectorHelper.SetKeywordEnabled(keyword, property, enabled);
                ((ShaderInspector)materialEditor.customShaderGUI).KeywordChangedFromPreset(
                    propertyName,
                    keyword,
                    enabled,
                    properties
                );
            }

            protected override void ApplyInternal(MaterialProperty property) {
                // Not Implemented on purpose, overriding Apply instead
            }
        }

        public class PresetPropertyPreset: PresetValue {

            private readonly int value;

            public PresetPropertyPreset(string propertyName, int value) : base(propertyName) {

                this.value = value;
            }

            public override void Apply(
                MaterialEditor materialEditor,
                MaterialProperty[] properties,
                MaterialProperty requestingPresetProperty
            ) {

                MaterialProperty property = ShaderInspector.FindProperty(propertyName, properties);
                if (property == null) {
                    GUILayout.Label($"Trying to use a preset for non-existing property \"{propertyName}\"", ShaderInspectorLayout.errorLabelStyle);
                    return;
                }

                property.floatValue = value;
                // Following line is a bit hacky, but it's pretty local hack so I think it should be ok
                ((ShaderInspector)materialEditor.customShaderGUI).ForceApplyAllPresets(
                    materialEditor: materialEditor,
                    properties: properties,
                    requestingPresetProperty: requestingPresetProperty
                );
            }

            protected override void ApplyInternal(MaterialProperty property) {
                // Not Implemented on purpose, overriding Apply instead
            }
        }

        public readonly string propertyName;
        public readonly string displayName;
        public readonly IReadOnlyList<Option> options;
        private readonly string _userTooltip = null;
        private readonly string _description = null;
        private readonly string _documentationUrl = null;
        private readonly string _documentationButtonLabel = null;
        private readonly string[] _displayOptions;

        private string _tooltip = null;

        private static readonly StringBuilder _reusableStringBuilder = new StringBuilder();

        public PresetDropdown(
            // Even though it's not declared in the shader, we will use this property name to store selected preset in the material
            string selectedOptionPropertyName,
            string displayName,
            IReadOnlyList<Option> options,
            string tooltip = null,
            string description = null,
            string documentationUrl = null,
            string documentationButtonLabel = null,
            DisplayFilter displayFilter = null,
            DisplayFilter enabledFilter = null
        ) : base(displayFilter, enabledFilter) {

            propertyName = selectedOptionPropertyName;
            this.displayName = displayName;
            this.options = options;
            _userTooltip = tooltip;
            _description = description;
            _documentationUrl = documentationUrl;
            _documentationButtonLabel = documentationButtonLabel;
            _displayOptions = options.Select(option => option.displayName).ToArray();
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

            MaterialProperty property = ShaderInspector.FindProperty(propertyName, properties);
            if (property == null) {
                GUILayout.Label($"Trying to draw non-existing property \"{propertyName}\"", ShaderInspectorLayout.errorLabelStyle);
                return;
            }
            if (property.type != MaterialProperty.PropType.Float) {
                GUILayout.Label($"Trying to use non-int/float property \"{propertyName}\" as a Feature toggle", ShaderInspectorLayout.errorLabelStyle);
                return;
            }

            var isDisabledByPreset = presetsData.IsDisabledByPresets(property, out var disablingPresetName);
            var selectedIndex = GetSelectedOptionIndex(property);

            if (_tooltip == null) {
                RefreshTooltip(selectedIndex);
            }

            var popupDisplayName = !string.IsNullOrEmpty(displayName) ? displayName : property.displayName;
            MaterialEditor.BeginProperty(property);
            using (new EditorGUI.DisabledScope(isDisabledByPreset || parentDisabled || ShouldBeDisabled(properties))) {
                var newIndex = EditorGUILayout.Popup(popupDisplayName, selectedIndex, _displayOptions);
                if (selectedIndex != newIndex) {
                    selectedIndex = newIndex;
                    property.floatValue = newIndex;
                    RefreshTooltip(newIndex);
                    ApplySelectedPreset(materialEditor, property, properties);
                }
            }
            MaterialEditor.EndProperty();
            var rect = GUILayoutUtility.GetLastRect();
            ShaderInspectorLayout.ShowTooltipIfHoverWithDisablingPreset(rect, _tooltip, disablingPresetName);
            EditorGUI.indentLevel++;
            // Just for styling purposes - disabled state
            using (new EditorGUI.DisabledScope(parentDisabled || ShouldBeDisabled(properties))) {
                ShaderInspectorLayout.Description(_description, _documentationUrl, _documentationButtonLabel);

                var selectedOption = options[selectedIndex];
                ShaderInspectorLayout.Description(selectedOption.description, selectedOption.documentationUrl, selectedOption.documentationButtonLabel);
            }
            EditorGUI.indentLevel--;
        }

        public void ApplySelectedPreset(MaterialEditor materialEditor, MaterialProperty[] properties) {

            MaterialProperty property = ShaderInspector.FindProperty(propertyName, properties);
            ApplySelectedPreset(materialEditor, property, properties);
        }

        private void ApplySelectedPreset(MaterialEditor materialEditor, MaterialProperty property, MaterialProperty[] properties) {

            var selectedOption = GetSelectedOption(property);
            var presets = selectedOption.presetValues;
            if (presets == null) {
                return;
            }
            foreach (var preset in presets) {
                preset.Apply(materialEditor, properties, requestingPresetProperty: property);
            }
        }

        public Option GetSelectedOption(MaterialProperty[] properties) {

            MaterialProperty property = ShaderInspector.FindProperty(propertyName, properties);
            return GetSelectedOption(property);
        }

        private Option GetSelectedOption(MaterialProperty property) {

            return options[GetSelectedOptionIndex(property)];
        }

        private int GetSelectedOptionIndex(MaterialProperty property) {

            var selectedIndex = Mathf.RoundToInt(property.floatValue);
            return Mathf.Clamp(selectedIndex, 0, _displayOptions.Length - 1);
        }

        private void RefreshTooltip(int selectedOptionIndex) {

            _reusableStringBuilder.Clear();
            var selectedOption = options[selectedOptionIndex];
            _reusableStringBuilder.Append(_userTooltip);
            if (selectedOption.presetValues != null) {
                _reusableStringBuilder.AppendLine("Preset is setting properties:");
                foreach (var presetValue in selectedOption.presetValues) {
                    _reusableStringBuilder.AppendLine(presetValue.propertyName);
                }
                _reusableStringBuilder.AppendLine();
            }
            _tooltip = _reusableStringBuilder.ToString();
        }


        public override void MarkUsedMaterialPropertiesSelfOnly(HashSet<MaterialProperty> usedMaterialProperties, MaterialProperty[] properties) {

            MaterialProperty property = ShaderInspector.FindProperty(propertyName, properties);
            if (property == null) {
                return;
            }
            usedMaterialProperties.Add(property);
        }

        public void MarkDisabledMaterialProperties(
            ShaderInspector.PresetsData presetsData,
            MaterialProperty[] properties
        ) {

            MaterialProperty property = ShaderInspector.FindProperty(propertyName, properties);
            if (property == null) {
                return;
            }
            var selectedOption = GetSelectedOption(properties);
            if (selectedOption.presetValues == null) {
                return;
            }
            foreach (var presetValue in selectedOption.presetValues) {
                MaterialProperty presetProperty = ShaderInspector.FindProperty(presetValue.propertyName, properties);
                if (presetProperty == null) {
                    continue;
                }
                presetsData.MarkDisabledMaterialProperty(presetProperty, displayName);
            }
        }

        public override (int enabledKeywordsCount, bool anyMixed) GetEnabledKeywordsCount(MaterialProperty[] properties) {

            return (enabledKeywordsCount: 0, anyMixed: false);
        }

        public override IEnumerable<(Element element, bool isActive, Element parentElement)> EnumerateSelfAndChildElementsRecursively(
            bool isParentActive,
            MaterialProperty[] properties,
            Element parentElement
        ) {

            yield return (this, isParentActive, parentElement);
        }

        public override bool ShouldBeDrawnWithSearchString(MaterialProperty[] properties, string searchString) {

            MaterialProperty property = ShaderInspector.FindProperty(propertyName, properties);
            return propertyName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                   (property?.displayName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                   options.Any(option =>
                       option.displayName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                       (option.presetValues != null &&
                        option.presetValues.Any(preset =>
                            preset.propertyName.Contains(searchString, StringComparison.OrdinalIgnoreCase))));
        }

        public override void ForceExpand() {}
    }
}
