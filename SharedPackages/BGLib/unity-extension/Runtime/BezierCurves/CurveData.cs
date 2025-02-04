namespace BGLib.UnityExtension.BezierCurves {

    using UnityEngine;

    public struct CurveData {

        public Vector3 startPoint;
        public Vector3 endPoint;
        public Vector3 startControlPoint;
        public Vector3 endControlPoint;

        public CurveData RotatePointsAroundPivot(Vector3 pivot, Quaternion rot) {

            var newStartPoint = RotatePointAroundPivot(startPoint, pivot, rot);
            var newEndPoint = RotatePointAroundPivot(endPoint, pivot, rot);
            var newStartControlPoint = RotatePointAroundPivot(startControlPoint, pivot, rot);
            var newEndControlPoint = RotatePointAroundPivot(endControlPoint, pivot, rot);

            return new CurveData {
                startPoint = newStartPoint,
                endPoint = newEndPoint,
                startControlPoint = newStartControlPoint,
                endControlPoint = newEndControlPoint
            };
        }

        public Vector3 Evaluate(float t) {

            var midP01 = Vector3.Lerp(startPoint, startControlPoint, t);
            var midP12 = Vector3.Lerp(startControlPoint, endControlPoint, t);
            var midP23 = Vector3.Lerp(endControlPoint, endPoint, t);

            var midP012 = Vector3.Lerp(midP01, midP12, t);
            var midP123 = Vector3.Lerp(midP12, midP23, t);

            return Vector3.Lerp(midP012, midP123, t);
        }

        private static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rot) {

            return rot * (point - pivot) + pivot;
        }
    }
}
