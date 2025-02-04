using UnityEngine;

public static class Vector2Extensions {

    public static float SignedAngleToLine(this Vector2 vec, Vector2 line) {

        var angle0 = Vector2.SignedAngle(vec, line);
        var angle1 = Vector2.SignedAngle(vec, -line);

        return Mathf.Abs(angle0) < Mathf.Abs(angle1) ? angle0 : angle1;
    }

    public static Vector2 Clamp(this Vector2 value, Vector2 min, Vector2 max) {

        return new Vector2(
            x: Mathf.Clamp(value.x, min.x, max.x),
            y: Mathf.Clamp(value.y, min.y, max.y)
        );
    }

    public static Vector2 Clamp(this Vector2 value, Rect within) {

        return new Vector2(
            x: Mathf.Clamp(value.x, within.xMin, within.xMax),
            y: Mathf.Clamp(value.y, within.yMin, within.yMax)
        );
    }
}
