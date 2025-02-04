using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

[CustomPropertyDrawer(typeof(ColorSO))]
public class ColorSOPropertyDrawer : PropertyDrawer {

    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

        var color = property.objectReferenceValue as ColorSO;

        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        if (color == null) {

            // Draw fields
            EditorGUI.ObjectField(position, property, GUIContent.none);
        }
        else {
            // // Calculate rects
            var objectRect = new Rect(position.x, position.y, position.width - 65, position.height);
            var colorRect = new Rect(position.x + position.width - 60, position.y, 60, position.height - 2);
            var alphaRect = new Rect(position.x + position.width - 60, position.y + position.height - 2, 60 * color.color.a, 2);
            Color fullColor = color;
            fullColor.a = 1.0f;

            EditorGUI.ObjectField(objectRect, property, GUIContent.none);
            if (color is SimpleColorSO simpleColor) {
                simpleColor.SetColor(EditorGUI.ColorField(colorRect, color));
            }
            else {
                EditorGUI.DrawRect(colorRect, fullColor);
                EditorGUI.DrawRect(alphaRect, Color.white);
            }
        }

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

        return EditorGUI.GetPropertyHeight(property);
    }
}
