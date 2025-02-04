namespace BeatSaber.InspectorName.Editor {

    using UnityEngine;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(InspectorLabelAttribute))]
    public class InspectorLabelAttributeDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            EditorGUI.PropertyField(position, property, new GUIContent((attribute as InspectorLabelAttribute)?.CustomLabel));
        }
    }
}
