using UnityEditor;
using UnityEngine;

public static class GUIUtil {

    private static GUIStyle _separatorStyle;

    public static GUIStyle GetColoredContainerStyle(Color color, int padding = 6, GUIStyle baseStyle = null) {

        var texture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
        texture.SetPixel(0, 0, color);
        texture.Apply();

        GUIStyle style;
        if (baseStyle == null) {
            style = new GUIStyle();
        }
        else {
            style = new GUIStyle(baseStyle);
        }

        style.normal.background = texture;
        style.padding = new RectOffset(padding, padding, padding, padding);

        return style;
    }

    public static void DrawSeparatorHorizontal(int margin) {

        GUIStyle marginStyle = CreateSeparatorStyle(GUI.skin.horizontalSlider, margin);
        EditorGUILayout.LabelField("", marginStyle, GUILayout.ExpandWidth(true), GUILayout.Height(margin * 2));
    }

    public static void DrawSeparatorVertical(int margin) {

        GUIStyle marginStyle = CreateSeparatorStyle(GUI.skin.verticalSlider, margin);
        EditorGUILayout.LabelField("", marginStyle, GUILayout.ExpandHeight(true), GUILayout.Width(margin * 2));
    }

    private static GUIStyle CreateSeparatorStyle(GUIStyle baseStyle, int margin) => new (baseStyle)
    {
        margin = new RectOffset(margin, margin, margin, margin)
    };
}
