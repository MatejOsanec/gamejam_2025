namespace BGLib.ShaderInspector {

    using UnityEditor;

    public class RangeProperty : SpecificProperty<float> {

        public RangeProperty(
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
            MaterialProperty.PropType.Range,
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

            var value = property.floatValue;
            var rangeLimits = property.rangeLimits;
            if (_materialToUIDelegate != null) {
                value = _materialToUIDelegate(value);
            }
            MaterialEditor.BeginProperty(property);
            value = EditorGUILayout.Slider(displayName, value, rangeLimits.x, rangeLimits.y);
            if (_uiToMaterialDelegate != null) {
                value = _uiToMaterialDelegate(value);
            }
            property.floatValue = value;
            MaterialEditor.EndProperty();
        }
    }
}
