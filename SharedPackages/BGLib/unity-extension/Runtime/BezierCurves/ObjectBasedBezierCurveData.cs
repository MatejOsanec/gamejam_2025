namespace BGLib.UnityExtension.BezierCurves {

    using System;
    using UnityEngine;

    [Serializable]
    public class ObjectBasedBezierCurveData : BaseBezierCurveData {

        [SerializeField] Transform _startPointTransform;
        [SerializeField] Transform _endPointTransform;

        public override Vector3 startPoint => _startPointTransform.position;
        public override Vector3 endPoint => _endPointTransform.position;

        public bool hasReferences => _startPointTransform != null && _endPointTransform != null;
    }
}
