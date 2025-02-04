using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

namespace BGLib.HierarchyIcons.Editor {

    public class ObjectOverrideSelectionPopup : EditorWindow {

        public List<ObjectOverride> overrides;
        public ObjectOverride selectedOverride;
        public System.Action<ObjectOverride> onSelected;

        public static ObjectOverrideSelectionPopup ShowWindow(List<ObjectOverride> overrides) {

            if (overrides == null || overrides.Count <= 0) {
                Debug.LogWarning("Attempted to create OverrideSelector window with no provided overrides.");
                return null;
            }

            var window = GetWindow<ObjectOverrideSelectionPopup>();
            window.titleContent = new GUIContent("OverrideSelector");
            window.overrides = overrides;
            window.selectedOverride = null;
            window.position = new Rect(
                position: GUIUtility.GUIToScreenPoint(Event.current.mousePosition),
                size: new Vector2(300, 40)
            );

            window.Show();
            return window;
        }

        private void OnOverrideSelected(object selectedOverrideObject) {

            selectedOverride = (ObjectOverride)selectedOverrideObject;
        }

        private void OnGUI() {

            GenericMenu dropdownContent = new GenericMenu();
            foreach (var overrideOption in overrides) {
                dropdownContent.AddItem(
                    content: new GUIContent($"{overrideOption.instanceObject.name} ({overrideOption.instanceObject.GetType().Name})"),
                    on: overrideOption == selectedOverride,
                    func: OnOverrideSelected,
                    userData: overrideOption
                );

                if (selectedOverride == null) {
                    selectedOverride = overrideOption;
                }
            }
            if (EditorGUILayout.DropdownButton(new GUIContent($"{selectedOverride.instanceObject.name} ({selectedOverride.instanceObject.GetType().Name})"), FocusType.Passive)) {
                dropdownContent.ShowAsContext();
            }

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            try {
                if (GUILayout.Button("Select", EditorStyles.miniButtonLeft)) {

                    onSelected?.Invoke(selectedOverride);
                    this.Close();
                }
                if (GUILayout.Button("Cancel", EditorStyles.miniButtonRight)) {

                    this.Close();
                }
            }
            finally {
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
