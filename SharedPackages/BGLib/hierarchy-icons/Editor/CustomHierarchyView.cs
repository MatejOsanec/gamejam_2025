using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using System.Linq;
using System.Reflection;
using UnityEngine.Assertions;

namespace BGLib.HierarchyIcons.Editor {

    [InitializeOnLoad]
    public class CustomHierarchyView {

        private const string kHasUnsavedDataIconPath = "Packages/com.beatgames.bglib.hierarchy-icons/Icons/EditOffTintable.png";
        private const string kIgnoreOverridesIconPath = "Packages/com.beatgames.bglib.hierarchy-icons/Icons/EditTintable.png";

        private static readonly Color hasUnsavedDataIconColor;
        private static readonly Color ignoreOverridesColor;

        private static readonly List<Type> _componentsWithHierarchyIconAttribute;
        private static readonly Dictionary<Type, HierarchyDataContainer> _hierarchyDataContainers;
        private static readonly Dictionary<string, Texture> _icons;

        static CustomHierarchyView() {

            EditorApplication.hierarchyWindowItemOnGUI -= HierarchyWindowItemOnGUI;
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
            _hierarchyDataContainers = new();
            _icons = new();
            _componentsWithHierarchyIconAttribute = GetComponentsWithHierarchyIconAttribute();
            if (!ColorUtility.TryParseHtmlString("#f29386", out hasUnsavedDataIconColor)) {
                throw new System.Exception("Could not parse hex code for hasUnsavedDataIconColor");
            }
            if (!ColorUtility.TryParseHtmlString("#a7e795", out ignoreOverridesColor)) {
                throw new System.Exception("Could not parse hex code for ignoreOverridesColor");
            }

            foreach (var type in _componentsWithHierarchyIconAttribute) {
                _hierarchyDataContainers.Add(type, new(type));
            }
        }

        private static void TryRegisterIcon(string path) {

            if (string.IsNullOrEmpty(path) || _icons.ContainsKey(path)) {
                return;
            }

            Texture texture = (Texture)AssetDatabase.LoadAssetAtPath(path, typeof(Texture));
            Assert.IsNotNull(texture, $"[HierarchyIcons] Texture did not exist at path {path}");
            _icons.Add(path, texture);
        }

        private static void HierarchyWindowItemOnGUI(int instanceId, Rect selectionRect) {

            GameObject selection = (GameObject)EditorUtility.InstanceIDToObject(instanceId);
            if (selection == null) {
                return;
            }

            var statusIcons = new List<HierarchyItemStatusElementDrawer>();

            statusIcons.AddRange(GetStatusIconsForUnsavedAndIgnore(selection));
            statusIcons.AddRange(GetComponentIconIfNecessary(selection));
            statusIcons.AddRange(GetComponentParentIconIfNecessary(selection));

            // Draw icons out from right-to-left
            for (int i = 0; i < statusIcons.Count; i++) {
                Rect iconRect = new Rect(selectionRect);
                iconRect.x += iconRect.width - (iconRect.height * (i + 1));
                iconRect.width = iconRect.height;
                statusIcons[i].Draw(iconRect);
            }
        }

        private static List<Type> GetComponentsWithHierarchyIconAttribute() {

            List<Type> result = new();

            var typesWithMyAttribute =
                from a in AppDomain.CurrentDomain.GetAssemblies().AsParallel() // Note the AsParallel here, this will parallelize everything after.
                from t in a.GetTypes()
                let attributes = t.GetCustomAttributes(typeof(HierarchyIconAttribute), true)
                where attributes != null && attributes.Length > 0
                select new { Type = t, Attributes = attributes.Cast<HierarchyIconAttribute>() };

            foreach (var type in typesWithMyAttribute) {
                if (!typeof(MonoBehaviour).IsAssignableFrom(type.Type)) {
                    continue;
                }

                result.Add(type.Type);
            }

            return result;
        }

        private static IList<HierarchyItemStatusElementDrawer> GetStatusIconsForUnsavedAndIgnore(GameObject selection) {

            bool isSelectionPrefab = PrefabUtility.IsPartOfPrefabInstance(selection);
            if (!isSelectionPrefab) {
                return Array.Empty<HierarchyItemStatusElementDrawer>();
            }

            bool isOutermostPrefab = selection == PrefabUtility.GetOutermostPrefabInstanceRoot(selection);
            if (!isOutermostPrefab) {
                return Array.Empty<HierarchyItemStatusElementDrawer>();
            }

            List<ObjectOverride> overrides = PrefabUtility.GetObjectOverrides(selection);
            if (overrides == null || overrides.Count <= 0) {
                return Array.Empty<HierarchyItemStatusElementDrawer>();
            }

            // Parse ignored overrides. We display an icon if we have any ignored overrides
            List<HierarchyItemStatusElementDrawer> statusIcons = new List<HierarchyItemStatusElementDrawer>(2);
            HierarchyIgnorePrefabOverrides ignorePrefabOverrideComponent = selection.GetComponent<HierarchyIgnorePrefabOverrides>();
            if (ignorePrefabOverrideComponent != null && ignorePrefabOverrideComponent.toIgnore != null) {

                for (int i = overrides.Count - 1; i >= 0; i--) {
                    if (!ignorePrefabOverrideComponent.toIgnore.Contains(overrides[i].instanceObject)) {
                        continue;
                    }

                    overrides.RemoveAt(i);
                }

                if (ignorePrefabOverrideComponent.toIgnore.Count > 0) {
                    TryRegisterIcon(kIgnoreOverridesIconPath);
                    if (_icons.TryGetValue(kIgnoreOverridesIconPath, out var texture)) {
                        statusIcons.Add(new IconDrawer(texture, ignoreOverridesColor, "Ignore overrides"));
                    }
                }

                /*  NOTE
                    If we get long lists of overrides, it may be worth implementing a hashed version using
                    https://docs.unity3d.com/ScriptReference/ISerializationCallbackReceiver.html
                */
            }

            // Display an icon if we have overrides (that aren't ignored)
            if (overrides.Count > 0) {
                TryRegisterIcon(kHasUnsavedDataIconPath);
                if (_icons.TryGetValue(kHasUnsavedDataIconPath, out var texture)) {
                    statusIcons.Add(new IconDrawer(texture, hasUnsavedDataIconColor, "Has unsaved data"));
                }
            }

            return statusIcons;
        }

        private static IList<HierarchyItemStatusElementDrawer> GetComponentIconIfNecessary(GameObject selection) {

            var listOfIcons = new List<HierarchyItemStatusElementDrawer>(1);

            foreach (var container in _hierarchyDataContainers) {
                if (!container.Value.IsComponent(selection.GetInstanceID())) {
                    continue;
                }

                var attr = container.Key.GetCustomAttribute<HierarchyIconAttribute>();
                TryRegisterIcon(attr.gameObjectIconPath);
                if (_icons.TryGetValue(attr.gameObjectIconPath, out var texture)) {
                    listOfIcons.Add(new IconDrawer(texture, attr.gameObjectIconTint, attr.gameObjectTooltip));
                }
            }

            return listOfIcons;
        }

        private static IList<HierarchyItemStatusElementDrawer> GetComponentParentIconIfNecessary(GameObject selection) {

            var listOfIcons = new List<HierarchyItemStatusElementDrawer>(1);

            foreach (var container in _hierarchyDataContainers) {
                if (container.Value.IsParentOfComponent(selection.GetInstanceID(), out var children) && !container.Value.IsComponent(selection.GetInstanceID())) {
                    var attr = container.Key.GetCustomAttribute<HierarchyIconAttribute>();
                    TryRegisterIcon(attr.parentIconPath);
                    if (_icons.TryGetValue(attr.parentIconPath, out var texture)) {
                        listOfIcons.Add(new SelectObjectsButtonDrawer(texture, attr.parentIconTint, attr.parentTooltip, children));
                    }
                }
            }

            return listOfIcons;
        }
    }
}
