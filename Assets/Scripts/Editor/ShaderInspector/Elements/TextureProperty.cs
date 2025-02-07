namespace BGLib.ShaderInspector {

    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;

    public class TextureProperty : SpecificProperty<(Texture textureValue, TextureDimension textureDimension, Vector4 textureScaleAndOffset)> {

        public TextureProperty(
            string propertyName,
            string displayName = null,
            string tooltip = null,
            string description = null,
            string documentationUrl = null,
            string documentationButtonLabel = null,
            DisplayFilter displayFilter = null,
            DisplayFilter enabledFilter = null,
            InOutValueModificationDelegate uiToMaterialDelegate = null,
            InOutValueModificationDelegate materialToUIDelegate = null
        ) : base(
            propertyName,
            MaterialProperty.PropType.Texture,
            displayName,
            tooltip,
            description,
            documentationUrl,
            documentationButtonLabel,
            displayFilter,
            enabledFilter,
            uiToMaterialDelegate: null,
            materialToUIDelegate: null
        ) {
            if (uiToMaterialDelegate != null || materialToUIDelegate != null) {
                Debug.LogWarning("InOutValueModificationDelegate not supported currently for texture property");
            }
        }

        protected override void DrawProperty(
            MaterialEditor materialEditor,
            MaterialProperty property,
            MaterialProperty[] properties,
            string displayName
        ) {

            materialEditor.TextureProperty(property, displayName);
        }
    }

    public class TexturePropertyWithHDRColor: TextureProperty {

        private readonly string _colorPropertyName;

        public TexturePropertyWithHDRColor(
            string propertyName,
            string colorPropertyName,
            string displayName = null,
            string tooltip = null,
            string description = null,
            string documentationUrl = null,
            string documentationButtonLabel = null,
            DisplayFilter displayFilter = null,
            DisplayFilter enabledFilter = null,
            InOutValueModificationDelegate uiToMaterialDelegate = null,
            InOutValueModificationDelegate materialToUIDelegate = null
        ) : base(
            propertyName,
            displayName,
            tooltip,
            description,
            documentationUrl,
            documentationButtonLabel,
            displayFilter,
            enabledFilter,
            uiToMaterialDelegate,
            materialToUIDelegate
        ) {
            _colorPropertyName = colorPropertyName;
        }

        protected override void DrawProperty(
            MaterialEditor materialEditor,
            MaterialProperty property,
            MaterialProperty[] properties,
            string displayName
        ) {
            MaterialProperty colorProperty = ShaderInspector.FindProperty(_colorPropertyName, properties);
            if (colorProperty == null) {
                GUILayout.Label($"Trying to draw non-existing property \"{_colorPropertyName}\"", ShaderInspectorLayout.errorLabelStyle);
                return;
            }
            if (colorProperty.type != MaterialProperty.PropType.Color) {
                GUILayout.Label($"Trying to draw property \"{_colorPropertyName}\" with type {MaterialProperty.PropType.Color} while it's type is {property.type}", ShaderInspectorLayout.errorLabelStyle);
                return;
            }

            materialEditor.TexturePropertyWithHDRColor(
                new GUIContent(text: displayName),
                textureProp: property,
                colorProperty: colorProperty,
                showAlpha: true
            );
        }

        public override void MarkUsedMaterialPropertiesSelfOnly(HashSet<MaterialProperty> usedMaterialProperties, MaterialProperty[] properties) {

            base.MarkUsedMaterialPropertiesSelfOnly(usedMaterialProperties, properties);
            MaterialProperty colorProperty = ShaderInspector.FindProperty(_colorPropertyName, properties);
            if (colorProperty != null) {
                usedMaterialProperties.Add(colorProperty);
            }
        }

        public override bool ShouldBeDrawnWithSearchString(MaterialProperty[] properties, string searchString) {

            MaterialProperty colorProperty = ShaderInspector.FindProperty(_colorPropertyName, properties);
            return base.ShouldBeDrawnWithSearchString(properties, searchString) ||
                   _colorPropertyName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                   (colorProperty?.displayName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false);;
        }
    }

    public class TexturePropertyMiniThumbnail: TextureProperty {

        public TexturePropertyMiniThumbnail(
            string propertyName,
            string displayName = null,
            string tooltip = null,
            string description = null,
            string documentationUrl = null,
            string documentationButtonLabel = null,
            DisplayFilter displayFilter = null,
            DisplayFilter enabledFilter = null,
            InOutValueModificationDelegate uiToMaterialDelegate = null,
            InOutValueModificationDelegate materialToUIDelegate = null
        ) : base(
            propertyName,
            displayName,
            tooltip,
            description,
            documentationUrl,
            documentationButtonLabel,
            displayFilter,
            enabledFilter,
            uiToMaterialDelegate,
            materialToUIDelegate
        ) { }

        protected override void DrawProperty(
            MaterialEditor materialEditor,
            MaterialProperty property,
            MaterialProperty[] properties,
            string displayName
        ) {

            materialEditor.TexturePropertySingleLine(new GUIContent(displayName), property);
        }
    }
}
