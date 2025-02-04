namespace BGLib.UnityExtension.BezierCurves {

    using System;
    using UnityEngine;

    [Serializable]
    public abstract class BaseBezierCurveData {

        [SerializeField] protected Vector3 _startControlPointDelta = Vector3.left/2;
        [SerializeField] protected Vector3 _endControlPointDelta = Vector3.right/2;

        public abstract Vector3 startPoint { get; }
        public abstract Vector3 endPoint { get; }

        public Vector3 startControlPoint {
            get => startPoint + _startControlPointDelta;
            set => _startControlPointDelta = value - startPoint;
        }
        public Vector3 endControlPoint {
            get => endPoint + _endControlPointDelta;
            set => _endControlPointDelta = value - endPoint;
        }
    }
}
