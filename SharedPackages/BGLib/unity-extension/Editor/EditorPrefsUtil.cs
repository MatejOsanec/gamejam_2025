using UnityEditor;
using UnityEngine;

public static class EditorPrefsUtil {

    public static Color GetColor(string key, Color defaultValue) {

        string colorString = EditorPrefs.GetString(key);
        if (colorString == null) {
            return defaultValue;
        }
        return ColorUtility.TryParseHtmlString(colorString, out Color color) ? color : defaultValue;
    }

    public static void SetColor(string key, Color value) {

        EditorPrefs.SetString(key, "#" + ColorUtility.ToHtmlStringRGBA(value));
    }
}
