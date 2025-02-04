using UnityEngine;

public static class TransformExtensions {

    public static Quaternion InverseTransformRotation(this Transform trans, Quaternion worldRotation) {

        return Quaternion.Inverse(trans.rotation) * worldRotation;
    }

    // Taken from OVRCommon.cs
    public static Transform FindChildRecursively(this Transform parent, string name) {

        for (int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (child.name.Contains(name)) {
                return child;
            }

            var result = child.FindChildRecursively(name);
            if (result != null) {
                return result;
            }
        }

        return null;
    }

    public static int CalculateTransformDepth(this Transform transform) {

        int depth = 0;
        while (transform != null) {
            depth++;
            transform = transform.parent;
        }

        return depth;
    }
}
