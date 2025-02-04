using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DrawIfAttribute))]
public class DrawIfPropertyDrawer : PropertyDrawer {
    
    private DrawIfAttribute _drawIfAttribute;
    private SerializedProperty _comparedField;
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

        if (!ShowProperty(property) && _drawIfAttribute.disablingType == DrawIfAttribute.DisablingType.DontDraw) {
            return 0.0f;
        }
        
        return base.GetPropertyHeight(property, label);
    }
    
    private bool ShowProperty(SerializedProperty property) {
        
        _drawIfAttribute = attribute as DrawIfAttribute;

        if (_drawIfAttribute == null) {
            return false;
        }

        var objVal = property.GetValue(hierarchyOffset: 1, _drawIfAttribute.propertyName);
        return objVal.Equals(_drawIfAttribute.value) || (_drawIfAttribute.orValue != null && objVal.Equals(_drawIfAttribute.value));
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        
        if (ShowProperty(property)) {
            EditorGUI.PropertyField(position, property, label);
        } 
        else if (_drawIfAttribute.disablingType == DrawIfAttribute.DisablingType.ReadOnly) {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true;
        }
    }
}