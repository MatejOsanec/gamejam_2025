namespace BGLib.UnityExtension.Editor {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public abstract class ShaderPropertyIDAttributeDrawer<T> : PropertyDrawer where T : ShaderPropertyIDAttribute {

        protected class DataGatherResult<TD> {

            public readonly bool succeeded;
            public readonly TD result;
            public readonly string errorMessage;

            private DataGatherResult(bool succeeded, TD result, string errorMessage) {

                this.succeeded = succeeded;
                this.result = result;
                this.errorMessage = errorMessage;
            }

            public static DataGatherResult<TD> Success(TD result) {

                return new DataGatherResult<TD>(
                    succeeded: true,
                    result: result,
                    errorMessage: string.Empty
                );
            }

            public static DataGatherResult<TD> Fail(string message) {

                return new DataGatherResult<TD>(
                    succeeded: false,
                    result: default,
                    errorMessage: message
                );
            }
        }

        protected T castedAttribute => attribute as T;

        protected readonly HashSet<Renderer> cachedTarget = new();

        private string[] _propertiesNames = Array.Empty<string>();
        private string[] _propertiesDisplayNames = Array.Empty<string>();
        private int _selectedPropertyIndex;
        private bool _dataAcquired;

        private const int kHelpBoxHeight = 38; //Gathered via debugging from EditorGUILayout.HelpBox

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            var propertyData = GatherDataAndCacheIfNecessary(property);

            _dataAcquired = propertyData.succeeded;

            if (!propertyData.succeeded) {
                EditorGUI.HelpBox(position, propertyData.errorMessage, MessageType.Error);
                return;
            }

            if (propertyData.result is { names: not null, displayNames: not null }) {
                _propertiesNames = propertyData.result.names;
                _propertiesDisplayNames = propertyData.result.displayNames;
                _selectedPropertyIndex = propertyData.result.selectedIndex;
            }

            var newSelected = EditorGUI.Popup(position, label.text, _selectedPropertyIndex, _propertiesDisplayNames);

            if (newSelected == _selectedPropertyIndex) {
                return;
            }

            property.stringValue = newSelected != 0 ? _propertiesNames[newSelected] : string.Empty;
            _selectedPropertyIndex = newSelected;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

            if (_dataAcquired) {
                return base.GetPropertyHeight(property, label);
            }

            return kHelpBoxHeight;
        }

        protected bool CacheMatchesData(Renderer[] data, HashSet<Renderer> cache) {

            if (cache.Count != data.Length) {
                return false;
            }

            foreach (var renderer in data.Where(e => e != null)) {
                if (!cache.Contains(renderer)) {
                    return false;
                }
            }

            return true;
        }

        protected (string[] names, string[] displayNames, int selectedPropIndex) CreatePropertiesNames(
            SerializedProperty property,
            ShaderPropertyIDAttribute shaderPropertyAttribute
        ) {

            var propertyNames = new SortedSet<string>();

            int index = 0;
            var current = property.stringValue;

            int selectedPropIndex = 0;

            foreach (var renderer in cachedTarget) {
                var materials = renderer.sharedMaterials.Where(e => e != null).ToArray();
                if(materials.Length == 0) {
                    continue;
                }
                var properties = MaterialEditor.GetMaterialProperties(materials);
                var filter = shaderPropertyAttribute.filter;
                foreach (var shaderProperty in FilterProperties(properties, filter)) {
                    var name = shaderProperty.name;
                    propertyNames.Add(name);
                    if (selectedPropIndex == 0 && name == current) {
                        selectedPropIndex = index + 1;
                    }
                    index++;
                }
            }

            if (propertyNames.Count == 0) {
                return (null, null, 0);
            }

            var propertyNamesAmount = propertyNames.Count + 1;

            var propertiesNames = new string[propertyNamesAmount];
            var propertiesDisplayNames = new string[propertyNamesAmount];

            propertiesNames[0] = "";
            propertiesDisplayNames[0] = "None";

            var currentIndex = 1;

            foreach (var propertyName in propertyNames) {
                propertiesNames[currentIndex] = propertyName;
                propertiesDisplayNames[currentIndex] = ObjectNames.NicifyVariableName(propertyName);
                currentIndex++;
            }

            return (propertiesNames, propertiesDisplayNames, selectedPropIndex);
        }

        private MaterialProperty.PropType ConvertTypes(ShaderPropertyAttributeFilter.PropType filterPropType) {

            switch (filterPropType) {

                case ShaderPropertyAttributeFilter.PropType.Color:
                    return MaterialProperty.PropType.Color;
                case ShaderPropertyAttributeFilter.PropType.Vector:
                    return MaterialProperty.PropType.Vector;
                case ShaderPropertyAttributeFilter.PropType.Float:
                    return MaterialProperty.PropType.Float;
                case ShaderPropertyAttributeFilter.PropType.Range:
                    return MaterialProperty.PropType.Range;
                case ShaderPropertyAttributeFilter.PropType.Texture:
                    return MaterialProperty.PropType.Texture;
                case ShaderPropertyAttributeFilter.PropType.Int:
                    return MaterialProperty.PropType.Int;
                default:
                    throw new ArgumentOutOfRangeException(nameof(filterPropType), filterPropType, null);
            }
        }

        private IEnumerable<MaterialProperty> FilterProperties(
            MaterialProperty[] properties,
            ShaderPropertyAttributeFilter filter
        ) {

            if (filter == null) {
                return properties;
            }

            IEnumerable<MaterialProperty> resultProperties = properties;

            if (filter.propType != ShaderPropertyAttributeFilter.PropType.Any) {
                resultProperties = resultProperties.Where(e => e.type == ConvertTypes(filter.propType));
            }

            if (filter.nameFilter != string.Empty) {
                resultProperties = resultProperties.Where(e => e.name.Contains(filter.nameFilter));
            }

            return resultProperties;
        }

        protected abstract DataGatherResult<(string[] names, string[] displayNames, int selectedIndex)> GatherDataAndCacheIfNecessary(SerializedProperty property);
    }
}
