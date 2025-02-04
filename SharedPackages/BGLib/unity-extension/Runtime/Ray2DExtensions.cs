using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Ray2DExtensions {

    public static int CircleIntersections(this Ray2D ray, Vector2 circleCenter, float radius, float[] distances) {

        int numberOfIntersections = 0;
        float A, B, C, det, t;

        A = ray.direction.x * ray.direction.x + ray.direction.y * ray.direction.y;
        B = 2 * (ray.direction.x * (ray.origin.x - circleCenter.x) + ray.direction.y * (ray.origin.y - circleCenter.y));
        C = (ray.origin.x - circleCenter.x) * (ray.origin.x - circleCenter.x) + (ray.origin.y - circleCenter.y) * (ray.origin.y - circleCenter.y) - radius * radius;

        det = B * B - 4 * A * C;
        if ((A <= 0.0000001) || (det < 0)) {
            // No real solutions.
            numberOfIntersections = 0;
        }
        else if (det == 0) {
            // One solution.
            t = -B / (2 * A);
            if (t >= 0.0f) {
                distances[0] = t; // new Vector2(ray.origin.x + t * ray.direction.x, ray.origin.y + t * ray.direction.y);
                numberOfIntersections++;
            }
        }
        else {
            // Two solutions.            
            t = (float)((-B + Mathf.Sqrt(det)) / (2 * A));
            if (t >= 0.0f) {
                distances[numberOfIntersections] = t; // new Vector2(ray.origin.x + t * ray.direction.x, ray.origin.y + t * ray.direction.y);
                numberOfIntersections++;
            }
            t = (float)((-B - Mathf.Sqrt(det)) / (2 * A));
            if (t >= 0.0f) {
                distances[numberOfIntersections] = t; // new Vector2(ray.origin.x + t * ray.direction.x, ray.origin.y + t * ray.direction.y);
                numberOfIntersections++;
            }
        }

        return numberOfIntersections;
    }
}
