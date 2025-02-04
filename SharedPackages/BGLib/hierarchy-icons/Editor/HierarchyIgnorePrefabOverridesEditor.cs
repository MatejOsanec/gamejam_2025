using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BGLib.HierarchyIcons.Editor {

    [CanEditMultipleObjects]
    [CustomEditor(typeof(HierarchyIgnorePrefabOverrides))]
    public class HierarchyIgnorePrefabOverridesEditor : UnityEditor.Editor {

        ObjectOverrideSelectionPopup _selector = null;

        public override void OnInspectorGUI() {

            base.OnInspectorGUI();

            GUILayout.Space(20);

            HierarchyIgnorePrefabOverrides target = (HierarchyIgnorePrefabOverrides)serializedObject.targetObject;
            GameObject outermost = PrefabUtility.GetOutermostPrefabInstanceRoot(target);
            if (outermost == null) {
                return;
            }

            // Only display the add button if we don't have any remaining overrides.
            List<ObjectOverride> overrides = PrefabUtility.GetObjectOverrides(outermost);
            if (target.toIgnore != null) {
                for (int i = overrides.Count - 1; i >= 0; i--) {
                    if (!target.toIgnore.Contains(overrides[i].instanceObject)) {
                        continue;
                    }

                    overrides.RemoveAt(i);
                }

                /*  NOTE
                    If we get long lists of overrides, it may be worth implementing a hashed version using
                    https://docs.unity3d.com/ScriptReference/ISerializationCallbackReceiver.html
                */
            }

            if (overrides == null || overrides.Count <= 0) {
                return;
            }

            if (GUILayout.Button("Add override", EditorStyles.miniButtonLeft)) {
                if (_selector != null) {
                    _selector.onSelected -= HandleOverrideSelectorOnSelected;
                }
                _selector = ObjectOverrideSelectionPopup.ShowWindow(overrides);
                _selector.onSelected += HandleOverrideSelectorOnSelected;
            }
        }

        private void HandleOverrideSelectorOnSelected(ObjectOverride selectedObject) {

            HierarchyIgnorePrefabOverrides target = (HierarchyIgnorePrefabOverrides)serializedObject.targetObject;
            target.AddIgnore(selectedObject.instanceObject);
            _selector.onSelected -= HandleOverrideSelectorOnSelected;
            _selector = null;
        }
    }
}
