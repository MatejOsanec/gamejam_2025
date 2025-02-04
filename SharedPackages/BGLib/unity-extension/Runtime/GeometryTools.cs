using UnityEngine;
using System.Collections;

public class GeometryTools {

    public static bool ThreePointsToBox(Vector3 p0, Vector3 p1, Vector3 p2, out Vector3 center, out Vector3 halfSize, out Quaternion orientation) {

        Vector3 up = Vector3.Cross(p1 - p2, p0 - p2).normalized;

        // Continue only if normal exists
        if (up.sqrMagnitude > 0.00001f) {

            Vector3 forward = (p0 - p1).normalized;
            Vector3 left = Vector3.Cross(forward, up);

            orientation = new Quaternion();
            orientation.SetLookRotation(forward, up);

            float a = Mathf.Abs((new Plane(left, p0)).GetDistanceToPoint(p2));
            float b = Vector3.Magnitude(p0 - p1);

            Vector3 pc = (p0 + p1) * 0.5f;

            center = pc - left * a * 0.5f;
            halfSize = new Vector3(a * 0.5f, 0.0f, b * 0.5f);

            return true;
        }
        else {

            center = Vector3.zero;
            halfSize = Vector3.zero;
            orientation = Quaternion.identity;

            return false;
        }
    }
}
