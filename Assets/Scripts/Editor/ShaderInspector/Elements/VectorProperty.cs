namespace BGLib.ShaderInspector {

    using UnityEditor;
    using UnityEngine;

    public class VectorProperty : SpecificProperty<Vector4> {

        private readonly int _fieldCount;

        public VectorProperty(
            string propertyName,
            int fieldCount = 4,
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
            MaterialProperty.PropType.Vector,
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

            _fieldCount = fieldCount;
        }

        protected override void OnGUIInternal(
            MaterialEditor materialEditor,
            MaterialProperty property,
            MaterialProperty[] properties,
            string disablingPresetName
        ) {

            var displayName = !string.IsNullOrEmpty(_displayName) ? _displayName : property.displayName;
            DrawProperty(materialEditor, property, properties, displayName);
            var rect = GUILayoutUtility.GetLastRect();
            ShaderInspectorLayout.ShowTooltipIfHoverWithDisablingPreset(rect, _tooltip, disablingPresetName);
            EditorGUI.indentLevel++;
            ShaderInspectorLayout.Description(_description, _documentationUrl, _documentationButtonLabel);
            EditorGUI.indentLevel--;
        }

        protected override void DrawProperty(
            MaterialEditor materialEditor,
            MaterialProperty property,
            MaterialProperty[] properties,
            string displayName
        ) {

            var vector = property.vectorValue;
            MaterialEditor.BeginProperty(property);
            if (_materialToUIDelegate != null) {
                vector = _materialToUIDelegate(vector);
            }
            switch (_fieldCount) {
                case 2:
                    vector = EditorGUILayout.Vector2Field(displayName, vector);
                    break;
                case 3:
                    vector = EditorGUILayout.Vector3Field(displayName, vector);
                    break;
                default:
                    vector = EditorGUILayout.Vector4Field(displayName, vector);
                    break;
            }
            if (_uiToMaterialDelegate != null) {
                vector = _uiToMaterialDelegate(vector);
            }
            property.vectorValue = vector;
            MaterialEditor.EndProperty();
        }
    }
}
