namespace BGLib.UnityExtension.BezierCurves {

    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Assertions;

    public abstract class BaseBezierCurve<T> : BaseBezierCurve where T : BaseBezierCurveData {

        [SerializeField] protected T _bezierCurveData;

#if UNITY_EDITOR
        protected void OnDrawGizmosSelected() {

            if (Selection.activeGameObject != gameObject) {
                return;
            }

            if (!isReady) {
                return;
            }

            var color = Gizmos.color;
            var bezierCurveData = GetBezierCurveData();
            Gizmos.color = mainPointsColor;
            Gizmos.DrawSphere(bezierCurveData.startPoint, mainPointsRadius);
            Gizmos.DrawSphere(bezierCurveData.endPoint, mainPointsRadius);
            Gizmos.color = controlPointsColor;
            Gizmos.DrawSphere(bezierCurveData.startControlPoint, controlPointsRadius);
            Gizmos.DrawSphere(bezierCurveData.endControlPoint, controlPointsRadius);
            Gizmos.color = Color.gray;
            Gizmos.DrawLine(bezierCurveData.startPoint, bezierCurveData.startControlPoint);
            Gizmos.DrawLine(bezierCurveData.endPoint, bezierCurveData.endControlPoint);
            Gizmos.color = color;
        }
#endif
    }

    public abstract class BaseBezierCurve : MonoBehaviour {

#if UNITY_EDITOR
        [Header("Gizmos")]
        [SerializeField] internal float mainPointsRadius = 0.05f;
        [SerializeField] internal Color mainPointsColor = Color.cyan;
        [SerializeField] internal float controlPointsRadius = 0.03f;
        [SerializeField] internal Color controlPointsColor = Color.green;
        public float lineWidth = 3f;
        public Color lineColor = Color.blue;
#endif

        public abstract bool isReady { get; }

        public Vector3 Evaluate(float current) {

            Assert.IsTrue(isReady, "Curve is not ready, check data");
            return GetBezierCurveData().Evaluate(current);
        }

        internal abstract CurveData GetBezierCurveData();
        internal abstract void SetBezierCurveData(CurveData data);
    }
}
