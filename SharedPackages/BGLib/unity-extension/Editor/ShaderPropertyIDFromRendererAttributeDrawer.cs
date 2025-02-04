using System.Linq;

namespace BGLib.UnityExtension.Editor {

    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(ShaderPropertyIDFromRendererAttribute))]
    public class ShaderPropertyIDFromRendererAttributeDrawer :
        ShaderPropertyIDAttributeDrawer<ShaderPropertyIDFromRendererAttribute> {

        private const string kPropertyNotFoundMessage =
            "Cannot display '{0}' because no property with name '{1}' is found";
        private const string kRendererNotFoundMessage =
            "Cannot display property '{0}' because no renderers found on {1}";
        private const string kPropertyIsNullMessage =
            "Cannot display property '{0}' because  '{1}' is null";
        private const string kNoShaderFoundMessage =
            "Cannot display property '{0}' because no shader properties found on '{1}'";

        protected override DataGatherResult<(string[] names, string[] displayNames, int selectedIndex)> GatherDataAndCacheIfNecessary(SerializedProperty property) {

            var result = GetTargetFromRenderer(property);

            if (!result.succeeded) {
                return DataGatherResult<(string[] names, string[] displayNames, int selectedIndex)>.Fail(result.errorMessage);
            }

            if (CacheMatchesData(result.result, cachedTarget)) {
                return DataGatherResult<(string[] names, string[] displayNames, int selectedIndex)>.Success((null, null, 0));
            }

            cachedTarget.Clear();

            foreach (var renderer in result.result.Where(e => e != null)) {
                cachedTarget.Add(renderer);
            }

            if (cachedTarget.Count == 0) {
                var message = string.Format(kRendererNotFoundMessage, property.name, castedAttribute.propertyName);
                return DataGatherResult<(string[] names, string[] displayNames, int selectedIndex)>.Fail(message);
            }

            var (names, displayNames, selectedPropIndex) = CreatePropertiesNames(property, castedAttribute);

            if (names == null && displayNames == null) {
                var message = string.Format(kNoShaderFoundMessage, property.name, castedAttribute.propertyName);
                return DataGatherResult<(string[] names, string[] displayNames, int selectedIndex)>.Fail(message);
            }

            return DataGatherResult<(string[] names, string[] displayNames, int selectedIndex)>.Success((names, displayNames, selectedPropIndex));
        }

        protected virtual DataGatherResult<Renderer[]> GetTargetFromRenderer(SerializedProperty property) {

            return GetTargetFromRenderer(property, castedAttribute.propertyName);
        }

        protected DataGatherResult<Renderer[]> GetTargetFromRenderer(SerializedProperty property, string propertyName) {

            var foundProperty = property.serializedObject.FindProperty(propertyName);

            if (foundProperty == null) {
                var message = string.Format(kPropertyIsNullMessage, property.name, propertyName);
                return DataGatherResult<Renderer[]>.Fail(message);
            }

            if (foundProperty.isArray) {

                var renderers = new Renderer[foundProperty.arraySize];

                for (int i = 0; i < foundProperty.arraySize; i++) {
                    var element = foundProperty.GetArrayElementAtIndex(i);
                    if (!(element.objectReferenceValue is Renderer rendererFromArray)) {
                        renderers[i] = null;
                        continue;
                    }
                    renderers[i] = rendererFromArray;
                }
                return DataGatherResult<Renderer[]>.Success(renderers);
            }

            if (!(foundProperty.objectReferenceValue is Renderer renderer)) {
                var message = string.Format(kPropertyIsNullMessage, property.name, propertyName);
                return DataGatherResult<Renderer[]>.Fail(message);
            }

            return DataGatherResult<Renderer[]>.Success(new[] { renderer });
        }
    }
}
