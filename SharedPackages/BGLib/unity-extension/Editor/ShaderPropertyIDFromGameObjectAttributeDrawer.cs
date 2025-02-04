namespace BGLib.UnityExtension.Editor {

    using System;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(ShaderPropertyIDFromGameObjectAttribute))]
    public class ShaderPropertyIDFromGameObjectAttributeDrawer :
        ShaderPropertyIDAttributeDrawer<ShaderPropertyIDFromGameObjectAttribute> {

        protected override DataGatherResult<(string[] names, string[] displayNames, int selectedIndex)> GatherDataAndCacheIfNecessary(SerializedProperty property) {

            var renderers = GetTargetFromGameObject(property.serializedObject);

            if (renderers.Length == 0) {
                return DataGatherResult<(string[] names, string[] displayNames, int selectedIndex)>.Fail($"Cannot display property '{property.name}' because no renderers found on GameObject");
            }

            if (CacheMatchesData(renderers, cachedTarget)) {
                return DataGatherResult<(string[] names, string[] displayNames, int selectedIndex)>.Success((null, null, 0));
            }

            cachedTarget.Clear();

            foreach (var renderer in renderers) {
                cachedTarget.Add(renderer);
            }

            var (names, displayNames, selectedPropIndex) = CreatePropertiesNames(property, castedAttribute);

            if (names == null && displayNames == null) {
                return DataGatherResult<(string[] names, string[] displayNames, int selectedIndex)>.Fail($"Cannot display property '{property.name}' because no shader properties found on GameObject");
            }

            return DataGatherResult<(string[] names, string[] displayNames, int selectedIndex)>.Success((names, displayNames, selectedPropIndex));
        }

        private Renderer[] GetTargetFromGameObject(SerializedObject serializedObject) {

            var targetObject = serializedObject.targetObject as Component;

            if (targetObject == null) {
                return Array.Empty<Renderer>();
            }

            return targetObject.GetComponents<Renderer>() ?? Array.Empty<Renderer>();
        }
    }
}
