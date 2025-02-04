namespace BGLib.ShaderInspector {

    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class InfoBox : Element {

        private readonly string _message;
        public readonly MessageType messageType;
        private readonly int _indentAmount;
        private readonly string _tooltip;
        private readonly string _documentationUrl;
        private readonly string _documentationButtonLabel;
        public Rect lastRectDrawnInto;

        public InfoBox(
            string message,
            MessageType messageType = MessageType.None,
            int indentAmount = 0,
            string tooltip = null,
            string documentationUrl = null,
            string documentationButtonLabel = null,
            DisplayFilter displayFilter = null,
            DisplayFilter enabledFilter = null
        ) : base(displayFilter, enabledFilter) {

            _message = message;
            this.messageType = messageType;
            _indentAmount = indentAmount;
            _tooltip = tooltip;
            _documentationUrl = documentationUrl;
            _documentationButtonLabel = documentationButtonLabel;
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

            // Disabled info box makes no sense but it styles it correctly
            using (new EditorGUI.DisabledScope(parentDisabled || ShouldBeDisabled(properties))) {
                EditorGUI.indentLevel += _indentAmount;
                using (new EditorGUILayout.HorizontalScope()) {
                    EditorGUILayout.HelpBox(_message, messageType);
                    ShaderInspectorLayout.DocumentationButton(_documentationUrl, _documentationButtonLabel);
                    lastRectDrawnInto = GUILayoutUtility.GetLastRect();
                }

                EditorGUI.indentLevel -= _indentAmount;
                ShaderInspectorLayout.ShowTooltipIfHover(GUILayoutUtility.GetLastRect(), _tooltip);
            }
        }

        public override void MarkUsedMaterialPropertiesSelfOnly(HashSet<MaterialProperty> usedMaterialProperties, MaterialProperty[] properties) {

            // Empty on purpose
        }

        public override (int enabledKeywordsCount, bool anyMixed) GetEnabledKeywordsCount(MaterialProperty[] properties) {

            return (enabledKeywordsCount: 0, anyMixed: false);
        }

        public override IEnumerable<(Element element, bool isActive, Element parentElement)> EnumerateSelfAndChildElementsRecursively(
            bool isParentActive,
            MaterialProperty[] properties,
            Element parentElement
        ) {

            var isActive = isParentActive && ShouldBeDrawn(properties, searchString: null) && !ShouldBeDisabled(properties);
            yield return (this, isActive, parentElement);
        }

        public override bool ShouldBeDrawnWithSearchString(MaterialProperty[] properties, string searchString) {

            return _message.Contains(searchString, StringComparison.OrdinalIgnoreCase);
        }

        public override void ForceExpand() {}
    }
}
