using UnityEngine;

public static class Vector3Extensions {

    public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value) {

        var ba = b - a;
        var va = value - a;

        return Vector3.Dot(va, ba) / Vector3.Dot(ba, ba);
    }

    public static Vector3 RotatedAroundPivot(this Vector3 vector, Quaternion rotation, Vector3 pivot) {

        return rotation * (vector - pivot) + pivot;
    }

    public static Vector3 MirrorOnYZPlane(this Vector3 vector) {

        return new Vector3(-vector.x, vector.y, vector.z);
    }

    public static Vector3 MirrorEulerAnglesOnYZPlane(this Vector3 vector) {

        return new Vector3(vector.x, -vector.y, -vector.z);
    }

    public static Vector3 Abs(this in Vector3 vector) {

        return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
    }
}
