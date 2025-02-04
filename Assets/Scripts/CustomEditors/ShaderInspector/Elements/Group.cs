namespace BGLib.ShaderInspector {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// Purpose of this element is to group other elements and then either indent or show/hide them together
    public class Group : Element {

        private readonly string _title;
        private readonly int _indentationCount;
        private readonly string _description;
        private bool _hasFoldout;
        private readonly List<Element> _childElements;

        private bool _foldout;

        private static readonly GUIStyle _foldoutStyle = new GUIStyle(EditorStyles.foldout) {
            fontSize = 12,
            fontStyle = FontStyle.Bold
        };

        private static readonly GUIStyle _titleStyle = new GUIStyle(EditorStyles.label) {
            fontSize = 12,
            fontStyle = FontStyle.Bold
        };

        public Group(
            string title = null,
            bool hasFoldout = false,
            int indentationCount = 0,
            string description = null,
            List<Element> childElements = null,
            DisplayFilter displayFilter = null,
            DisplayFilter enabledFilter = null
        ) : base(displayFilter, enabledFilter) {

            _title = title;
            _hasFoldout = hasFoldout;
            _indentationCount = indentationCount;
            _description = description;
            _childElements = childElements;
            _foldout = SessionState.GetBool(_title, false);
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

            if (_childElements == null) {
                return;
            }

            if (!_hasFoldout && !string.IsNullOrEmpty(_title)) {
                // This is here because either by design or by Unity bug EditorGUI.indentLevel is not applied onto GUILayout.Label
                using(new GUILayout.HorizontalScope()) {
                    GUILayout.Space(EditorGUI.indentLevel * 15.0f);
                    GUILayout.Label(_title, _titleStyle);
                }
            }
            if (_hasFoldout) {
                EditorGUI.indentLevel += 1;
                _foldout = EditorGUILayout.Foldout(_foldout, _title, toggleOnLabelClick: true, _foldoutStyle);
                SessionState.SetBool(_title, _foldout);
            }

            if (!_hasFoldout || _foldout) {
                ShaderInspectorLayout.Description(_description);
                EditorGUI.indentLevel += _indentationCount;
                foreach (var shaderInspectorElement in _childElements) {
                    shaderInspectorElement.OnGUI(materialEditor, properties, searchString, presetsData, parentDisabled || ShouldBeDisabled(properties));
                }
                EditorGUI.indentLevel -= _indentationCount;
            }
            if (_hasFoldout) {
                EditorGUI.indentLevel -= 1;
            }

            if (!string.IsNullOrEmpty(_title)) {
                GUILayout.Space(kDefaultVerticalSpacingAfterGroupWithTitle);
            }
        }

        public override void MarkUsedMaterialPropertiesSelfOnly(HashSet<MaterialProperty> usedMaterialProperties, MaterialProperty[] properties) {

            // Empty
        }

        public override (int enabledKeywordsCount, bool anyMixed) GetEnabledKeywordsCount(MaterialProperty[] properties) {

            if (_childElements == null) {
                return (0, false);
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
            Element parentElement
        ) {

            yield return (this, isParentActive, parentElement);
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

            return _title != null &&  _title.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                   _childElements.Any(element => element.ShouldBeDrawnWithSearchString(properties, searchString));
        }

        public override void ForceExpand() {

            _foldout = true;
            SessionState.SetBool(_title, _foldout);
        }
    }

    // Shortcut for Group with foldout
    public class SubCategory : Group {

        public SubCategory(
            string title,
            int indentationCount = 1,
            string description = null,
            List<Element> childElements = null,
            DisplayFilter displayFilter = null,
            DisplayFilter enabledFilter = null
        ) : base(
            title,
            hasFoldout: true,
            indentationCount,
            description,
            childElements,
            displayFilter,
            enabledFilter
        ) { }
    }
}
