using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(FutureFieldAttribute))]
public class FutureFieldAttributePropertyDrawer : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

#if BS_TOURS
        EditorGUI.PropertyField(position, property, label, true);
#endif
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

#if BS_TOURS
        return EditorGUI.GetPropertyHeight(property, label);
#else
        return 0.0f;
#endif
    }
}

[CustomPropertyDrawer(typeof(WillNotBeUsedAttribute))]
public class WillNotBeUsedAttributePropertyDrawer : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

#if !BS_TOURS
        EditorGUI.PropertyField(position, property, label, includeChildren: true);
#endif
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

#if !BS_TOURS
        return EditorGUI.GetPropertyHeight(property, label);
#else
        return 0.0f;
#endif
    }
}
