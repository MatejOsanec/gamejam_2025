using UnityEngine;

public abstract class ColorSO : PersistentScriptableObject {

    public abstract Color color {
        get;
    }

    public static implicit operator Color(ColorSO c) {

        if (c == null) {
            return Color.clear;
        }
        else {
            return c.color;
        }
    }
}
