namespace BGLib.ShaderInspector {

    using UnityEditor;

    public class IntProperty : SpecificProperty<int> {

        public IntProperty(
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
            MaterialProperty.PropType.Int,
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

            var value = property.intValue;
            if (_materialToUIDelegate != null) {
                value = _materialToUIDelegate(value);
            }
            MaterialEditor.BeginProperty(property);
            value = EditorGUILayout.IntField(displayName, value);
            if (_uiToMaterialDelegate != null) {
                value = _uiToMaterialDelegate(value);
            }
            property.intValue = value;
            MaterialEditor.EndProperty();
        }
    }
}
