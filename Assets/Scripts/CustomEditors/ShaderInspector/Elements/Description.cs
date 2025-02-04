namespace BGLib.ShaderInspector {

    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class Description : Element {

        private readonly string _message;
        private readonly string _tooltip;
        private readonly string _documentationUrl;
        private readonly string _documentationButtonLabel;

        public Description(
            string message,
            string tooltip = null,
            string documentationUrl = null,
            string documentationButtonLabel = null,
            DisplayFilter displayFilter = null,
            DisplayFilter enabledFilter = null
        ) : base(displayFilter, enabledFilter) {

            _message = message;
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

            // Disabled label makes no sense but it styles it correctly
            using (new EditorGUI.DisabledScope(parentDisabled || ShouldBeDisabled(properties))) {
                ShaderInspectorLayout.Description(_message, _documentationUrl, _documentationButtonLabel);
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

            yield return (this, isParentActive, parentElement);
        }

        public override bool ShouldBeDrawnWithSearchString(MaterialProperty[] properties, string searchString) {

            return _message.Contains(searchString, StringComparison.OrdinalIgnoreCase);
        }

        public override void ForceExpand() {}
    }
}
