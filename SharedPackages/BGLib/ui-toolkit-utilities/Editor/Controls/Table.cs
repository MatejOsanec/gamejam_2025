using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BGLib.UIToolkitUtilities.Controls.Editor {

    public class Table : UIToolkitUtilities.Controls.Table {

        public new class UxmlFactory : UxmlFactory<Table, UxmlTraits> { }

        public new class UxmlTraits : UIToolkitUtilities.Controls.Table.UxmlTraits {

            UxmlBoolAttributeDescription _enableTogglingColumnVisibility = new() { name = "enable-toggling-visibility" };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc) {

                base.Init(ve, bag, cc);
                Table tableToInit = (Table)ve;
                tableToInit.enableTogglingColumnVisibility = _enableTogglingColumnVisibility.GetValueFromBag(bag, cc);
            }
        }

        public bool enableTogglingColumnVisibility { get; set; }

        private GenericMenu headerRightClick;

        public Table() : base() {

            this.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Defines.kGlobalStyleSheet));
            this.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Defines.kStyleSheetDirectory + "Table.uss"));
        }

        public override void BuildTable() {

            base.BuildTable();

            headerRightClick = new GenericMenu();
            for (int i = 0; i < columnSetup.Count; i++) {
                int index = i;
                headerRightClick.AddItem(new GUIContent(columnSetup[i].name), runtimeColumnData[index].visible, () => {
                    runtimeColumnData[index].visible = !runtimeColumnData[index].visible;
                    ClearTable();
                    BuildTable();
                });
            }
        }

        protected override VisualElement CreateHeaderRow() {

            var result = base.CreateHeaderRow();

            if (enableTogglingColumnVisibility) {
                result.RegisterCallback<MouseUpEvent>(HandleMouseUpEventOnHeader);
            }

            return result;
        }

        private void HandleMouseUpEventOnHeader(MouseUpEvent mouseUpEvent) {

            if (mouseUpEvent.button != (int)MouseButton.RightMouse) {
                return;
            }

            headerRightClick.ShowAsContext();
        }
    }
}
