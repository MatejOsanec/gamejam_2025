namespace ColorLibrary.Editor {

    using UnityEditor;

    public static class ColorSchemeEditorExtensions {

        public static void SetColorScheme(this ColorSchemeSO colorSchemeSO, ColorScheme colorScheme) {

            colorSchemeSO._colorScheme = colorScheme;

            EditorUtility.SetDirty(colorSchemeSO);
        }

        public static void SetSortingOrder(this ColorSchemeSO colorSchemeSO, int sortingOrder) {

            colorSchemeSO._order = sortingOrder;

            EditorUtility.SetDirty(colorSchemeSO);
        }
    }
}
