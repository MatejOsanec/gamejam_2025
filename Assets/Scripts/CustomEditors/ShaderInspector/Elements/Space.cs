namespace BGLib.ShaderInspector {

    using System.Collections.Generic;
    using UnityEditor;

    public class Space : Element {

        private readonly float _height;

        public Space(
            float height = 6.0f,
            DisplayFilter displayFilter = null,
            DisplayFilter enabledFilter = null
        ) : base(displayFilter, enabledFilter) {

            _height = height;
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

            EditorGUILayout.Space(_height);
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

            return false;
        }

        public override void ForceExpand() {}
    }
}
