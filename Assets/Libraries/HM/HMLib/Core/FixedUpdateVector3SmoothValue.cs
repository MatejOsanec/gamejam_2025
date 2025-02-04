using UnityEngine;

public class FixedUpdateVector3SmoothValue : FixedUpdateSmoothValue<Vector3> {

    public FixedUpdateVector3SmoothValue(float smooth) : base(smooth) {}

    protected override Vector3 Interpolate(Vector3 value0, Vector3 value1, float t) {

        return Vector3.LerpUnclamped(value0, value1, t);
    }
}
