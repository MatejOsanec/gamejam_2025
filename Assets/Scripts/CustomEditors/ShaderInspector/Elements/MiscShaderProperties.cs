namespace BGLib.ShaderInspector {

    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine.Rendering;

    public class MiscShaderProperties: Element {

        public MiscShaderProperties(DisplayFilter displayFilter = null, DisplayFilter enabledFilter = null) : base(
            displayFilter,
            enabledFilter
        ) { }

        public override void OnGUI(
            MaterialEditor materialEditor,
            MaterialProperty[] properties,
            string searchString,
            ShaderInspector.PresetsData presetsData,
            bool parentDisabled
        ) {

            if (SupportedRenderingFeatures.active.editableMaterialRenderQueue) {
                materialEditor.RenderQueueField();
            }
            materialEditor.EnableInstancingField();
        }

        public override void MarkUsedMaterialPropertiesSelfOnly(HashSet<MaterialProperty> usedMaterialProperties, MaterialProperty[] properties) {

            // Empty
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

            return "Render Queue".Contains(searchString) || "Enable GPU Instancing".Contains(searchString);
        }

        public override void ForceExpand() {}
    }
}
