using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class ColorExtensions {

    public static bool FromHtmlStringRGBA(string htmlColor, out Color color) {

        color = Color.black;

        if (htmlColor.StartsWith('#')) {
            htmlColor = htmlColor[1..];
        }

        switch (htmlColor.Length) {
            case 6:
                color = new Color (
                    r: HtmlStringToFloat(htmlColor[0..2]),
                    g: HtmlStringToFloat(htmlColor[2..4]),
                    b: HtmlStringToFloat(htmlColor[4..6]),
                    a: 1.0f
                );

                return true;
            case 8:
                color = new Color (
                    r: HtmlStringToFloat(htmlColor[0..2]),
                    g: HtmlStringToFloat(htmlColor[2..4]),
                    b: HtmlStringToFloat(htmlColor[4..6]),
                    a: HtmlStringToFloat(htmlColor[6..8])
                );

                return true;
        }

        return false;

        static float HtmlStringToFloat(string htmlColor) => Convert.ToInt32(htmlColor, 16) / 255.0f;
    }

    public static Color GetColorFromHtmlString(string colorHtmlString)
        => FromHtmlStringRGBA(colorHtmlString, out var c) ? c : Color.black;

    public static Color SaturatedColor(this Color color, float saturation) {

        float h, s, v;

        Color.RGBToHSV(color, out h, out s, out v);
        s = saturation;
        return Color.HSVToRGB(h, s, v);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color ColorWithAlpha(this Color color, float alpha) {

        color.a = alpha;
        return color;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color ColorWithMultipliedAlpha(this Color color, float alphaMultiplier) {

        color.a *= alphaMultiplier;
        return color;
    }

    public static Color ColorWithR(this Color color, float r) {

        color.r = r;
        return color;
    }

    public static Color ColorWithG(this Color color, float g) {

        color.g = g;
        return color;
    }

    public static Color ColorWithB(this Color color, float b) {

        color.b = b;
        return color;
    }

    public static Color ColorWithValue(this Color color, float value) {

        float h, s, v;

        Color.RGBToHSV(color, out h, out s, out v);
        v = value;
        return Color.HSVToRGB(h, s, v);
    }

    public static Color MultiplyRGB(this Color c, float m) {

        return new Color(c.r * m, c.g * m, c.b * m, c.a);
    }

    public static Color LerpRGBUnclamped(this Color a, Color b, float t) {

        return new Color(a.r + (b.r - a.r) * t, a.g + (b.g - a.g) * t, a.b + (b.b - a.b) * t, a.a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEqualTo(this Color32 a, Color32 b) {

        return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
    }
}
