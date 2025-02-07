namespace BGLib.ShaderInspector {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public class Category : Element {

        private bool foldout;
        private readonly string _title;
        private readonly string _tooltip;
        private readonly string _description;
        private readonly string _documentationUrl;
        private readonly string _documentationButtonLabel;
        private readonly List<Element> _childElements;
        private readonly Color _backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.05f);

        public Category(
            string title,
            string tooltip = null,
            string description = null,
            string documentationUrl = null,
            string documentationButtonLabel = null,
            List<Element> childElements = null,
            Color? backgroundColor = null,
            DisplayFilter displayFilter = null,
            DisplayFilter enabledFilter = null
        ) : base(displayFilter, enabledFilter) {

            _title = title;
            _tooltip = tooltip;
            _description = description;
            _documentationUrl = documentationUrl;
            _documentationButtonLabel = documentationButtonLabel;
            _childElements = childElements;
            if (backgroundColor.HasValue) {
                _backgroundColor = backgroundColor.Value;
            }

            foldout = SessionState.GetBool(_title, false);
        }

        public override void OnGUI(
            MaterialEditor materialEditor,
            MaterialProperty[] properties,
            string searchString,
            ShaderInspector.PresetsData presetsData,
            bool parentDisabled
        ) {

            if (!ShouldBeDrawn(properties, searchString)) {
                return;
            }

            EditorGUILayout.Space(kDefaultVerticalSpacingBeforeFeatures);

            var keywordsCountResult = GetEnabledKeywordsCount(properties);

            using (var category = new ShaderInspectorLayout.Category(_title, _tooltip, keywordsCountResult.enabledKeywordsCount, foldout, keywordsCountResult.anyMixed, _backgroundColor)) {
                foldout = category.foldout;
                SessionState.SetBool(_title, foldout);
                if (foldout) {
                    // Just for styling purposes
                    using (new EditorGUI.DisabledGroupScope(parentDisabled || ShouldBeDisabled(properties))) {
                        ShaderInspectorLayout.Description(_description, _documentationUrl, _documentationButtonLabel);
                    }
                }
                if (foldout && _childElements != null) {
                    EditorGUILayout.Space(kDefaultVerticalSpacingInsideCategory);
                    foreach (var shaderInspectorElement in _childElements) {
                        shaderInspectorElement.OnGUI(materialEditor, properties, searchString, presetsData, parentDisabled || ShouldBeDisabled(properties));
                    }
                }
            }

            EditorGUILayout.Space(kDefaultVerticalSpacingAfterFeatures);
        }

        public override void MarkUsedMaterialPropertiesSelfOnly(HashSet<MaterialProperty> usedMaterialProperties, MaterialProperty[] properties) {

        }

        public override (int enabledKeywordsCount, bool anyMixed) GetEnabledKeywordsCount(MaterialProperty[] properties) {

            if (_childElements == null) {
                return (enabledKeywordsCount: 0, anyMixed: false);
            }
            if (!ShouldBeDrawn(properties, searchString: null)) {
                return (enabledKeywordsCount: 0, anyMixed: false);
            }

            var enabledKeywordsCount = 0;
            var anyMixed = false;
            foreach (var childElement in _childElements) {
                var childResult = childElement.GetEnabledKeywordsCount(properties);
                enabledKeywordsCount += childResult.enabledKeywordsCount;
                anyMixed |= childResult.anyMixed;
            }

            return (enabledKeywordsCount, anyMixed);
        }

        public override IEnumerable<(Element element, bool isActive, Element parentElement)> EnumerateSelfAndChildElementsRecursively(
            bool isParentActive,
            MaterialProperty[] properties,
            Element parent
        ) {

            yield return (this, isParentActive, parent);
            if (_childElements == null) {
                yield break;
            }
            var isSelfActive = ShouldBeDrawn(properties, searchString: null) && !ShouldBeDisabled(properties);
            foreach (var childElement in _childElements) {
                foreach (var element in childElement.EnumerateSelfAndChildElementsRecursively(isParentActive && isSelfActive, properties, this)) {
                    yield return element;
                }
            }
        }

        public override bool ShouldBeDrawnWithSearchString(MaterialProperty[] properties, string searchString) {

            return _title.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                   _childElements.Any(element => element.ShouldBeDrawnWithSearchString(properties, searchString));
        }

        public override void ForceExpand() {

            foldout = true;
            SessionState.SetBool(_title, foldout);
        }
    }
}
