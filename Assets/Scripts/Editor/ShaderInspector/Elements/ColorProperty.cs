namespace BGLib.ShaderInspector {

    using UnityEditor;
    using UnityEngine;

    public class ColorProperty : SpecificProperty<Color> {

        private bool _hdr;

        public ColorProperty(
            string propertyName,
            string displayName = null,
            string tooltip = null,
            string description = null,
            string documentationUrl = null,
            string documentationButtonLabel = null,
            DisplayFilter displayFilter = null,
            DisplayFilter enabledFilter = null,
            InOutValueModificationDelegate uiToMaterialDelegate = null,
            InOutValueModificationDelegate materialToUIDelegate = null,
            bool hdr = false
        ) : base(
            propertyName,
            MaterialProperty.PropType.Color,
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

            _hdr = hdr;
        }

        protected override void DrawProperty(
            MaterialEditor materialEditor,
            MaterialProperty property,
            MaterialProperty[] properties,
            string displayName
        ) {

            var color = property.colorValue;
            if (_materialToUIDelegate != null) {
                color = _materialToUIDelegate(color);
            }
            MaterialEditor.BeginProperty(property);
            color = EditorGUILayout.ColorField(new GUIContent(text: displayName), color, showEyedropper: true, showAlpha: true, hdr: _hdr);
            if (_uiToMaterialDelegate != null) {
                color = _uiToMaterialDelegate(color);
            }
            property.colorValue = color;
            MaterialEditor.EndProperty();
        }
    }
}
