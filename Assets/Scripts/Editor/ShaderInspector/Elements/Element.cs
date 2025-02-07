namespace BGLib.ShaderInspector {

    using System.Collections.Generic;
    using UnityEditor;

    /// Subclasses of this are abstract representations of what and how should be drawn, each of them doing the actual drawing
    /// It typically uses ShaderInspectorLayout or Unity's GUILayout or EditorGUILayout internally
    public abstract class Element {

        public delegate bool DisplayFilter(MaterialProperty[] properties);

        protected const float kDefaultVerticalSpacingAfterFeatures = 4.0f;
        protected const float kDefaultVerticalSpacingBeforeFeatures = 4.0f;
        protected const float kDefaultVerticalSpacingInsideCategory = 8.0f;
        protected const float kDefaultVerticalSpacingAfterGroupWithTitle = 12.0f;

        private readonly DisplayFilter _displayFilter;
        private readonly DisplayFilter _enabledFilter;

        /// Draws the element (unless filtered out by DisplayFilter or search) and all it's child elements
        public abstract void OnGUI(
            MaterialEditor materialEditor,
            MaterialProperty[] properties,
            string searchString,
            ShaderInspector.PresetsData presetsData,
            bool parentDisabled
        );

        /// Used for drawing properties without custom drawing code at the end of custom inspector
        /// cannot use OnGUI for that as hidden (like foldout) properties would not be accounted for correctly
        public abstract void MarkUsedMaterialPropertiesSelfOnly(HashSet<MaterialProperty> usedMaterialProperties, MaterialProperty[] properties);

        /// Gets count of enabled keywords for self and all children
        public abstract (int enabledKeywordsCount, bool anyMixed) GetEnabledKeywordsCount(MaterialProperty[] properties);

        /// Enumerates the whole tree of elements = self, all children recursively
        /// Even inactive/hidden elements (like foldouts and display filters)
        public abstract IEnumerable<(Element element, bool isActive, Element parentElement)> EnumerateSelfAndChildElementsRecursively(
            bool isParentActive,
            MaterialProperty[] properties,
            Element parentElement
        );

        // Both filters, null => true
        protected Element(DisplayFilter displayFilter, DisplayFilter enabledFilter) {

            _displayFilter = displayFilter;
            _enabledFilter = enabledFilter;
        }

        protected bool ShouldBeDrawn(MaterialProperty[] properties, string searchString) {

            if (!string.IsNullOrEmpty(searchString) && !ShouldBeDrawnWithSearchString(properties, searchString)) {
                return false;
            }
            if (_displayFilter == null) {
                return true;
            }
            return _displayFilter(properties);
        }

        protected bool ShouldBeDisabled(MaterialProperty[] properties) {

            return _enabledFilter != null && !_enabledFilter.Invoke(properties);
        }

        public abstract bool ShouldBeDrawnWithSearchString(MaterialProperty[] properties, string searchString);

        public abstract void ForceExpand();
    }
}
