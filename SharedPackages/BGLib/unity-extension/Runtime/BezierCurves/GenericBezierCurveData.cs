namespace BGLib.UnityExtension.BezierCurves {

    using System;
    using UnityEngine;

    [Serializable]
    public class GenericBezierCurveData : BaseBezierCurveData {

        [SerializeField] Vector3 _startPoint = Vector2.right;
        [SerializeField] Vector3 _endPoint = Vector2.left;

        public override Vector3 startPoint => _startPoint;
        public override Vector3 endPoint => _endPoint;

        public void SetStartPoint(Vector3 newValue) {

            _startPoint = newValue;
        }

        public void SetEndPoint(Vector3 newValue) {

            _endPoint = newValue;
        }

        public GenericBezierCurveData RotatePointsAroundPivot(Vector3 pivot, Quaternion rot) {

            var newStartPoint = RotatePointAroundPivot(_startPoint, pivot, rot);
            var newEndPoint = RotatePointAroundPivot(_endPoint, pivot, rot);
            var newStartControlPoint = RotatePointAroundPivot(startControlPoint, pivot, rot);
            var newEndControlPoint = RotatePointAroundPivot(endControlPoint, pivot, rot);

            return new GenericBezierCurveData {
                _startPoint = newStartPoint,
                _endPoint = newEndPoint,
                _startControlPointDelta = newStartControlPoint - newStartPoint,
                _endControlPointDelta = newEndControlPoint - newEndPoint
            };
        }

        private static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rot) {

            return rot * (point - pivot) + pivot;
        }
    }
}
