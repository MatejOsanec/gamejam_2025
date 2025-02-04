namespace BGLib.ShaderInspector {

    using UnityEditor;

    public static class CustomDisplayFilter {

        public static bool ExampleCustomDisplayFilter(this MaterialProperty[] properties) {

            return properties.HasNoneOfKeywords("A1", "C2") ||
                   properties.HasAnyOfKeywords("B", "D5") ||
                   properties.HasAnyOfKeywords("T") ||
                   (properties.HasAllOfKeywords("F1", "F2") && !properties.HasNoneOfKeywords("G"));
        }
    }
}
