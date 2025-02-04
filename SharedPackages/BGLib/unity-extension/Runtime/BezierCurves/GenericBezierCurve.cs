namespace BGLib.UnityExtension.BezierCurves {

    using UnityEngine;

    public class GenericBezierCurve : BaseBezierCurve<GenericBezierCurveData> {

        public override bool isReady => true;

        internal override CurveData GetBezierCurveData() {

            var tr = transform;
            var position = tr.position;
            var temp = new CurveData {
                startPoint = _bezierCurveData.startPoint + position,
                endPoint = _bezierCurveData.endPoint + position,
                startControlPoint = _bezierCurveData.startControlPoint + position,
                endControlPoint = _bezierCurveData.endControlPoint + position
            };
            return temp.RotatePointsAroundPivot(position, tr.rotation);
        }

        internal override void SetBezierCurveData(CurveData newValue) {

            var tr = transform;
            var d = newValue.RotatePointsAroundPivot(tr.position, (Quaternion.Inverse(tr.rotation)));
            var position = transform.position;
            _bezierCurveData.SetStartPoint(d.startPoint - position);
            _bezierCurveData.SetEndPoint( d.endPoint - position);
            _bezierCurveData.endControlPoint = d.endControlPoint - position;
            _bezierCurveData.startControlPoint = d.startControlPoint - position;
        }
    }
}
