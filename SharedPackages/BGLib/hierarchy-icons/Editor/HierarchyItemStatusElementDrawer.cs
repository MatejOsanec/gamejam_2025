using System;
using UnityEngine;
using UnityEditor;

namespace BGLib.HierarchyIcons.Editor {

    internal abstract class HierarchyItemStatusElementDrawer {

        public readonly Texture texture;
        public readonly Color tint;
        public readonly string tooltip;

        protected HierarchyItemStatusElementDrawer(Texture texture, Color tint, string tooltip) {

            this.texture = texture;
            this.tint = tint;
            this.tooltip = tooltip;
        }

        public abstract void Draw(Rect rect);
    }

    internal class IconDrawer : HierarchyItemStatusElementDrawer {

        private readonly GUIContent _guiContent;
        private readonly GUIStyle _guiStyle;

        public IconDrawer(Texture texture, Color tint, string tooltip) : base(texture, tint, tooltip) {

            _guiContent = new GUIContent { image = texture, tooltip = tooltip };
            _guiStyle = new GUIStyle { border = new RectOffset(0, 0, 0, 0) };
        }

        public override void Draw(Rect rect) {

            var previousColor = GUI.contentColor;
            GUI.contentColor = tint;
            GUI.Box(rect, _guiContent, _guiStyle);
            GUI.contentColor = previousColor;
        }
    }

    internal class SelectObjectsButtonDrawer : HierarchyItemStatusElementDrawer {

        private readonly GameObject[] _objectsToSelect;
        private readonly GUIContent _guiContent;
        private readonly GUIStyle _guiStyle;

        public SelectObjectsButtonDrawer(Texture texture, Color tint, string tooltip, GameObject[] objectsToSelect)
            : base(texture, tint, tooltip) {

            _objectsToSelect = objectsToSelect;
            _guiContent = new GUIContent { image = texture, tooltip = String.Format(tooltip, objectsToSelect.Length) };
            _guiStyle = new GUIStyle { border = new RectOffset(0, 0, 0, 0) };
        }

        public override void Draw(Rect rect) {

            var previousColor = GUI.contentColor;
            GUI.contentColor = tint;
            if (GUI.Button(rect, _guiContent, _guiStyle)) {
                Selection.objects = _objectsToSelect;
            }
            GUI.contentColor = previousColor;
        }
    }
}
