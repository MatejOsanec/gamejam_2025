using UnityEditor;
using System.Collections.Generic;

namespace ColorLibrary {

    public class CacheDataWithSerializedProperty {

        public string name;
        public readonly SerializedObject serializedObject;
        public readonly SerializedProperty serializedProperty;
        private string path;

        public CacheDataWithSerializedProperty(SerializedObject serializedObject, string name, SerializedProperty serializedProperty, string path = "") {

            this.serializedObject = serializedObject;
            this.name = name;
            this.serializedProperty = serializedProperty;
            this.path = path;
        }
    }

    public class BaseUIColorCacheData : CacheDataWithSerializedProperty {

        public readonly List<AlphaVariationCacheData> alphaVariations;

        public BaseUIColorCacheData(SerializedObject serializedObject, string name, SerializedProperty colorProperty, string path) : base(serializedObject, name, colorProperty, path) {

            alphaVariations = new List<AlphaVariationCacheData>();
        }
    }

    public class AlphaVariationCacheData : CacheDataWithSerializedProperty {

        public readonly string baseColorName;
        public readonly float alphaValue;
        public readonly string alphaName;

        public AlphaVariationCacheData(SerializedObject serializedObject, string name, string baseColorName, SerializedProperty alphaProperty, string path) : base(serializedObject, name, alphaProperty, path) {

            this.baseColorName = baseColorName;
            AlphaSO alphaSO = (AlphaSO)alphaProperty.objectReferenceValue;
            alphaValue = alphaSO ? alphaSO.alphaValue : 0.0f;
            alphaName = alphaSO.ToString();
        }
    }
}
