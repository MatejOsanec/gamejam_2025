using System.Runtime.CompilerServices;
using UnityEngine;

public static class MathfExtra {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Mod(float value, float mod) => value - mod * Mathf.Floor(value / mod);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Mod(int value, int mod) => value - mod * (int)Mathf.Floor(value / (float)mod);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Round(float value, int decimals) {

        var m = (int)Mathf.Pow(10, decimals);
        return (float)Mathf.RoundToInt(value * m) / m;
    }

    /// <summary>
    ///   <para>Returns the value of largest absolute of two values.</para>
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>a or b</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float MaxAbs(float a, float b) {

        return Mathf.Abs(a) > Mathf.Abs(b) ? a : b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Approximately(float a, float b, float precision) {

        return Mathf.Abs(a - b) < precision;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ShortestAngleDifference(float from, float to) {

        return (to - from + 180.0f) % 360.0f - 180.0f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Round(this Vector4 value, int digits) {

        float multiplication = Mathf.Pow(10.0f, digits);
        float division = 1.0f / multiplication;

        value.x = Mathf.Round(value.x * multiplication) * division;
        value.y = Mathf.Round(value.y * multiplication) * division;
        value.z = Mathf.Round(value.z * multiplication) * division;
        value.w = Mathf.Round(value.w * multiplication) * division;
        return value;
    }

    public static int Repeat(int t, int length) => Mathf.Clamp(t - Mathf.FloorToInt(t / (float)length) * length, 0, length);
}
