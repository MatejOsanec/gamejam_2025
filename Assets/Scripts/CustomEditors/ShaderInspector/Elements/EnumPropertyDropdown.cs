using System;
using System.Linq;
using BGLib.ShaderInspector;
using UnityEditor;
using UnityEngine;

public class EnumPropertyDropdown<T> : SpecificProperty<float> where T : Enum {

    private readonly T[] _options;

    public EnumPropertyDropdown(
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
        propertyType: MaterialProperty.PropType.Float,
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

        if (uiToMaterialDelegate != null || materialToUIDelegate != null) {
            Debug.LogWarning("InOutValueModificationDelegate not supported currently for enum property");
        }
        _options = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
    }

    protected override void DrawProperty(
        MaterialEditor materialEditor,
        MaterialProperty property,
        MaterialProperty[] properties,
        string displayName
    ) {
        var selectedIndex = Mathf.RoundToInt(property.floatValue);
        selectedIndex = Mathf.Clamp(selectedIndex, 0, _options.Length - 1);
        var selectedOption = _options[selectedIndex];

        MaterialEditor.BeginProperty(property);
        var newValue = (T)EditorGUILayout.EnumPopup(displayName, selectedOption);
        if (!newValue.Equals(selectedOption)) {
            property.floatValue = Convert.ToInt32(newValue);
        }
        MaterialEditor.EndProperty();
    }
}
