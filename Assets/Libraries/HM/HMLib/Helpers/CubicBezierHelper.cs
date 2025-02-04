using System.Collections.Generic;
using UnityEngine;

/// Collection of functions related to cubic bezier curves
/// (a curve with a start and end 'anchor' point, and two 'control' points to define the shape of the curve between the anchors)
public static class CubicBezierHelper {

    /// Returns point at time 't' (between 0 and 1)  along bezier curve defined by 4 points (anchor_1, control_1, control_2, anchor_2)
    public static Vector3 EvaluateCurve(in Vector3 a1, in Vector3 c1, in Vector3 c2, in Vector3 a2, float t) {

        // intentionally removed clamp, as it is never needed in our usage
        // t = Mathf.Clamp01(t);
        return (1 - t) * (1 - t) * (1 - t) * a1 + 3 * (1 - t) * (1 - t) * t * c1 + 3 * (1 - t) * t * t * c2 + t * t * t * a2;
    }

    /// Calculates the derivative of the curve at time 't'
    /// This is the vector tangent to the curve at that point
    public static Vector3 EvaluateCurveDerivative(in Vector3 a1, in Vector3 c1, in Vector3 c2, in Vector3 a2, float t) {

        // intentionally removed clamp, as it is never needed in our usage
        // t = Mathf.Clamp01(t);
        return (3 * (1 - t) * (1 - t)) * (c1 - a1) + (6 * (1 - t) * t) * (c2 - c1) + (3 * t * t) * (a2 - c2);
    }

    ///Returns the second derivative of the curve at time 't'
    public static Vector3 EvaluateCurveSecondDerivative(in Vector3 a1, in Vector3 c1, in Vector3 c2, in Vector3 a2, float t) {

        // intentionally removed clamp, as it is never needed in our usage
        // t = Mathf.Clamp01(t);
        return (6 * (1 - t)) * (c2 - 2 * c1 + a1) + (6 * t) * (a2 - 2 * c2 + c1);
    }

    /// Calculates the normal vector (vector perpendicular to the curve) at specified time
    public static Vector3 Normal(in Vector3 a1, in Vector3 c1, in Vector3 c2, in Vector3 a2, float t) {

        Vector3 tangent = EvaluateCurveDerivative(in a1, in c1, in c2, in a2, t);
        Vector3 nextTangent = EvaluateCurveSecondDerivative(in a1, in c1, in c2, in a2, t);
        Vector3 c = Vector3.Cross(nextTangent, tangent);
        return Vector3.Cross(c, tangent).normalized;
    }

    /// Splits curve into two curves at time t. Modifies passed in points list
    public static void SplitCurve(List<Vector3> points, float t) {

        Vector3 p0 = points[0], p1 = points[1], p2 = points[2], p3 = points[3];
        Vector3 a1 = Vector3.Lerp(p0, p1, t);
        Vector3 a2 = Vector3.Lerp(p1, p2, t);
        Vector3 a3 = Vector3.Lerp(p2, p3, t);
        Vector3 b1 = Vector3.Lerp(a1, a2, t);
        Vector3 b2 = Vector3.Lerp(a2, a3, t);
        Vector3 pointOnCurve = Vector3.Lerp(b1, b2, t);

        // Update the list to contain { p0, a1, b1, pointOnCurve, b2, a3, p3 }
        points.Clear();
        points.Add(p0);
        points.Add(a1);
        points.Add(b1);
        points.Add(pointOnCurve);
        points.Add(b2);
        points.Add(a3);
        points.Add(p3);
    }

    // Crude, but fast estimation of curve length.
    public static float EstimateCurveLength(in Vector3 p0, in Vector3 p1, in Vector3 p2, in Vector3 p3) {

        float controlNetLength = (p0 - p1).magnitude + (p1 - p2).magnitude + (p2 - p3).magnitude;
        float estimatedCurveLength = (p0 - p3).magnitude + controlNetLength / 2.0f;
        return estimatedCurveLength;
    }
}
