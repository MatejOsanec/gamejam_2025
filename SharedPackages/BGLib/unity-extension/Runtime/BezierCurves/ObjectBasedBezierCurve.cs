namespace BGLib.UnityExtension.BezierCurves {

    public class ObjectBasedBezierCurve : BaseBezierCurve<ObjectBasedBezierCurveData> {

        public override bool isReady => _bezierCurveData.hasReferences;

        internal override CurveData GetBezierCurveData() {

            return new CurveData {
                startPoint = _bezierCurveData.startPoint,
                endPoint = _bezierCurveData.endPoint,
                startControlPoint = _bezierCurveData.startControlPoint,
                endControlPoint = _bezierCurveData.endControlPoint
            };
        }

        internal override void SetBezierCurveData(CurveData newValue) {

            _bezierCurveData.endControlPoint = newValue.endControlPoint;
            _bezierCurveData.startControlPoint = newValue.startControlPoint;
        }
    }
}
