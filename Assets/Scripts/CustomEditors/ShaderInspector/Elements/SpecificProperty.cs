namespace BGLib.ShaderInspector {
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    public abstract class SpecificProperty<T> : Element {

        public delegate T InOutValueModificationDelegate(T originalValue);

        private readonly string _propertyName;
        protected readonly string _displayName;
        protected readonly string _tooltip;
        protected readonly string _description;
        protected readonly string _documentationUrl;
        protected readonly string _documentationButtonLabel;
        private readonly MaterialProperty.PropType _propertyType;
        protected readonly InOutValueModificationDelegate _materialToUIDelegate;
        protected readonly InOutValueModificationDelegate _uiToMaterialDelegate;

        protected SpecificProperty(
            string propertyName,
            MaterialProperty.PropType propertyType,
            string displayName = null,
            string tooltip = null,
            string description = null,
            string documentationUrl = null,
            string documentationButtonLabel = null,
            DisplayFilter displayFilter = null,
            DisplayFilter enabledFilter = null,
            InOutValueModificationDelegate uiToMaterialDelegate = null,
            InOutValueModificationDelegate materialToUIDelegate = null
        ) : base(displayFilter, enabledFilter) {

            _propertyName = propertyName;
            _displayName = displayName;
            _propertyType = propertyType;
            _description = description;
            _documentationUrl = documentationUrl;
            _documentationButtonLabel = documentationButtonLabel;
            _materialToUIDelegate = materialToUIDelegate;
            _uiToMaterialDelegate = uiToMaterialDelegate;
            if (_materialToUIDelegate == null ^ _uiToMaterialDelegate == null) {
                if (_uiToMaterialDelegate?.Method.GetCustomAttribute<UIToMaterialOnlyAllowedAttribute>() == null) {
                    Debug.LogWarning($"materialToUIDelegate and uiToMaterialDelegate should both be null or not null, " +
                                     $"currently one is null while other is not which is highly suspicious for property \"{propertyName}\"");
                }
            }
            _tooltip = ShaderInspectorHelper.BuildTooltip(tooltip, propertyName);
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

            MaterialProperty property = ShaderInspector.FindProperty(_propertyName, properties);
            if (property == null) {
                GUILayout.Label($"Trying to draw non-existing property \"{_propertyName}\"", ShaderInspectorLayout.errorLabelStyle);
                return;
            }
            if (_propertyType != property.type) {
                GUILayout.Label($"Trying to draw property \"{_propertyName}\" with type {_propertyType} while it's type is {property.type}", ShaderInspectorLayout.errorLabelStyle);
                return;
            }
            var isDisabledByPreset = presetsData.IsDisabledByPresets(property, out var disablingPresetName);
            using (new EditorGUI.DisabledScope(isDisabledByPreset || parentDisabled || ShouldBeDisabled(properties))) {
                OnGUIInternal(materialEditor, property, properties, disablingPresetName);
            }
        }

        protected virtual void OnGUIInternal(
            MaterialEditor materialEditor,
            MaterialProperty property,
            MaterialProperty[] properties,
            string disablingPresetName
        ) {

            var previousRect = GUILayoutUtility.GetLastRect();

            var displayName = !string.IsNullOrEmpty(_displayName) ? _displayName : property.displayName;
            DrawProperty(materialEditor, property, properties, displayName);
            var rect = GUILayoutUtility.GetLastRect();
            // Ugly workaround, some ShaderProperty like texture returns 0 width or height making tooltip not working
            // Might not work correctly in all cases
            if (rect.width < 5 || rect.height < 5) {
                var rectY = previousRect.y + previousRect.height;
                rect.width = previousRect.width;
                rect.height = (rect.y + rect.height) - rectY;
                rect.x = previousRect.x;
                rect.y = rectY;
            }
            ShaderInspectorLayout.ShowTooltipIfHoverWithDisablingPreset(rect, _tooltip, disablingPresetName);
            EditorGUI.indentLevel++;
            ShaderInspectorLayout.Description(_description, _documentationUrl, _documentationButtonLabel);
            EditorGUI.indentLevel--;
        }

        protected abstract void DrawProperty(
            MaterialEditor materialEditor,
            MaterialProperty property,
            MaterialProperty[] properties,
            string displayName
        );

        public override void MarkUsedMaterialPropertiesSelfOnly(HashSet<MaterialProperty> usedMaterialProperties, MaterialProperty[] properties) {

            MaterialProperty property = ShaderInspector.FindProperty(_propertyName, properties);
            if (property == null) {
                return;
            }
            usedMaterialProperties.Add(property);
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

            MaterialProperty property = ShaderInspector.FindProperty(_propertyName, properties);
            return _propertyName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                   (property?.displayName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        public override void ForceExpand() { }
    }
}
