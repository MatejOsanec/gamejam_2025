namespace BGLib.UnityExtension.Editor {

    using System;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(InfoBoxAttribute))]
    public class InfoBoxAttributeDrawer : DecoratorDrawer {

        private const int kIconHeight = 38; //Gathered via debugging from EditorGUILayout.HelpBox
        private float _width = -1;

        public override void OnGUI(Rect position) {

            if (attribute is not InfoBoxAttribute infoBoxAttribute) {
                return;
            }

            if (!Mathf.Approximately(position.size.x, 1)) {
                _width = position.size.x;
            }

            var height = GetHeight(_width, infoBoxAttribute);

            EditorGUI.HelpBox(
                new Rect(position.position, new Vector2(_width, height)),
                infoBoxAttribute.info,
                CastToMessageType(infoBoxAttribute.messageType)
            );
        }

        public override float GetHeight() {

            if (attribute is not InfoBoxAttribute infoBoxAttribute) {
                return 0;
            }

            return GetHeight(_width, infoBoxAttribute);
        }

        private float GetHeight(float width, InfoBoxAttribute infoBoxAttribute) {

            if (Mathf.Approximately(width, -1)) {
                return kIconHeight;
            }

            Texture2D texture = null;

            if (infoBoxAttribute.messageType != InfoBoxAttribute.Type.None) {
                texture = EditorGUIUtility.FindTexture(GetIconName(infoBoxAttribute.messageType));
            }

            return EditorStyles.helpBox.CalcHeight(new GUIContent(infoBoxAttribute.info, texture), width);
        }

        private static MessageType CastToMessageType(InfoBoxAttribute.Type type) {

            return type switch {
                InfoBoxAttribute.Type.None => MessageType.None,
                InfoBoxAttribute.Type.Info => MessageType.Info,
                InfoBoxAttribute.Type.Warning => MessageType.Warning,
                InfoBoxAttribute.Type.Error => MessageType.Error,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        private static string GetIconName(InfoBoxAttribute.Type type) {

            return type switch {
                InfoBoxAttribute.Type.Info => "console.infoicon",
                InfoBoxAttribute.Type.Warning => "console.warnicon",
                InfoBoxAttribute.Type.Error => "console.erroricon",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}
