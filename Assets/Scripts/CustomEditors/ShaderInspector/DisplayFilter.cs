namespace BGLib.ShaderInspector {

    using System;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public static class DisplayFilter {

        #region Keywords

        public static bool HasAnyOfKeywords(this MaterialProperty[] properties, params string[] keywords) {

            return properties.HasAtLeastNKeywords(1, keywords);
        }

        public static bool HasAllOfKeywords(this MaterialProperty[] properties, params string[] keywords) {

            return properties.HasAtLeastNKeywords(keywords.Length, keywords);
        }

        public static bool HasNoneOfKeywords(this MaterialProperty[] properties, params string[] keywords) {

            return properties.HasMaximumNKeywords(0, keywords);
        }

        public static bool HasSecondaryUVsEnabled(this MaterialProperty[] properties) {

            return properties.HasAnyOfKeywords("_SECONDARY_UVS_EXTERNAL_SCALE", "_SECONDARY_UVS_ADDITIVE_OFFSET",
                "_SECONDARY_UVS_IMPORT", "_SECONDARY_UVS_OBJECT_SPACE", "_SECONDARY_UVS_TRAILS");
        }

        public static bool HasTextureEmission(this MaterialProperty[] properties) {

            return properties.HasAnyOfKeywords("_EMISSIONTEXTURE_SIMPLE",
                "_EMISSIONTEXTURE_FLIPBOOK", "_EMISSIONTEXTURE_PULSE");
        }

        public static bool HasEmissionSourceTexture(this MaterialProperty[] properties) {

            return properties.HasNoneOfKeywords("_EMISSION_TEXTURE_SOURCE_FILL", "_EMISSION_TEXTURE_SOURCE_MPM_G");
        }

        /// At least requiredCount of keywords is enabled
        public static bool HasAtLeastNKeywords(this MaterialProperty[] properties, int requiredCount, params string[] keywords) {

            var targets = GetTargets(properties);
            foreach (var target in targets) {
                if (target is not Material material) {
                    continue;
                }

                var requiredCountRemaining = requiredCount;
                for (int i = 0; i < keywords.Length; i++) {
                    // Early break, no chance of satisfying condition
                    if (requiredCountRemaining > keywords.Length - i) {
                        return false;
                    }

                    var keyword = keywords[i];
                    requiredCountRemaining -= material.IsKeywordEnabled(keyword) ? 1 : 0;

                    // Early break, condition already satisfied
                    if (requiredCountRemaining == 0) {
                        break;
                    }
                }

                if (requiredCountRemaining > 0) {
                    return false;
                }
            }

            return true;
        }

        public static bool HasMaximumNKeywords(this MaterialProperty[] properties, int maximumCount, params string[] keywordConditions) {

            var targets = GetTargets(properties);
            foreach (var target in targets) {
                if (target is not Material material) {
                    continue;
                }

                var count = 0;
                foreach (string keyword in keywordConditions) {
                    count += material.IsKeywordEnabled(keyword) ? 1 : 0;
                    // Early break to save some performance
                    if (count > maximumCount) {
                        return false;
                    }
                }
            }

            return true;
        }

        private static Object[] GetTargets(MaterialProperty[] properties) {

            if (properties.Length == 0) {
                return Array.Empty<Object>();
            }
            return properties[0].targets;
        }

        #endregion

        #region Math

        public delegate bool Condition<in T>(T currentValue);

        public enum ComparisonType {

            Equal,
            NotEqual,
            EqualOrLower,
            EqualOrHigher,
            Lower,
            Higher
        }

        public static bool HasNonZeroIndex(this MaterialProperty[] properties, string propertyName) {

            return properties.FloatPropertyComparison(propertyName, ComparisonType.NotEqual, conditionValue: 0);
        }

        public static bool FloatPropertyComparison(this MaterialProperty[] properties, string propertyName, ComparisonType comparisonType, float conditionValue) {

            switch (comparisonType) {
                case ComparisonType.Equal:
                    return properties.FloatPropertyCondition(propertyName, propertyValue => Mathf.Approximately(conditionValue, propertyValue));
                case ComparisonType.NotEqual:
                    return properties.FloatPropertyCondition(propertyName, propertyValue => !Mathf.Approximately(conditionValue, propertyValue));
                case ComparisonType.EqualOrLower:
                    return properties.FloatPropertyCondition(propertyName, propertyValue => propertyValue <= conditionValue);
                case ComparisonType.EqualOrHigher:
                    return properties.FloatPropertyCondition(propertyName, propertyValue => propertyValue >= conditionValue);
                case ComparisonType.Lower:
                    return properties.FloatPropertyCondition(propertyName, propertyValue => propertyValue < conditionValue);
                case ComparisonType.Higher:
                    return properties.FloatPropertyCondition(propertyName, propertyValue => propertyValue > conditionValue);
            }

            return false;
        }

        public static bool FloatPropertyCondition(this MaterialProperty[] properties, string propertyName, Condition<float> condition) {

            if (!TryGetProperty(propertyName, properties, out var property)) {
                return false;
            }
            return condition(property.floatValue);
        }

        public static bool IntPropertyCondition(this MaterialProperty[] properties, string propertyName, Condition<int> condition) {

            if (!TryGetProperty(propertyName, properties, out var property)) {
                return false;
            }
            return condition(property.intValue);
        }

        public static bool ColorPropertyCondition(this MaterialProperty[] properties, string propertyName, Condition<Color> condition) {

            if (!TryGetProperty(propertyName, properties, out var property)) {
                return false;
            }
            return condition(property.colorValue);
        }

        public static bool VectorPropertyCondition(this MaterialProperty[] properties, string propertyName, Condition<Vector4> condition) {

            if (!TryGetProperty(propertyName, properties, out var property)) {
                return false;
            }
            return condition(property.vectorValue);
        }

        public static bool TexturePropertyCondition(this MaterialProperty[] properties, string propertyName, Condition<Texture> condition) {

            if (!TryGetProperty(propertyName, properties, out var property)) {
                return false;
            }
            return condition(property.textureValue);
        }

        #endregion

        private static bool TryGetProperty(string propertyName, MaterialProperty[] properties, out MaterialProperty property) {

            foreach (var prop in properties) {
                if (prop.name == propertyName) {
                    property = prop;
                    return true;
                }
            }
            Debug.LogWarning($"Trying to base a condition on non-existing property \"{propertyName}\"");
            property = null;
            return false;
        }
    }
}
