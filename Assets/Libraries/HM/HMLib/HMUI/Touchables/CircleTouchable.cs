using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace HMUI {

    public class CircleTouchable : Touchable {

        [SerializeField] float _minRadius = 10.0f;
        [SerializeField] float _maxRadius = 15.0f;

        private RectTransform _containerRect;

        protected override void OnEnable() {

            base.OnEnable();

            UpdateCachedReferences();
        }

#if UNITY_EDITOR
        protected override void OnValidate() {

            base.OnValidate();
            UpdateCachedReferences();

            // This can be invoked before OnEnabled is called. So we shouldn't be accessing other objects, before OnEnable is called.
            if (IsActive()) {
                _maxRadius = Mathf.Clamp(_maxRadius, 0.0f, Mathf.Min(_containerRect.rect.width * 0.5f, _containerRect.rect.height * 0.5f));
                _minRadius = Mathf.Clamp(_minRadius, 0.0f, Mathf.Min(_containerRect.rect.width * 0.5f, _containerRect.rect.height * 0.5f));
            }

        }
#endif // if UNITY_EDITOR

        private void UpdateCachedReferences() {

            _containerRect = transform as RectTransform;
        }

        public override bool Raycast(Vector2 sp, Camera eventCamera) {

            Vector2 localPos;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_containerRect, sp, eventCamera, out localPos)) {
                return false;
            }

            var sqrMagnitude = localPos.sqrMagnitude;

            if (sqrMagnitude > _maxRadius * _maxRadius || sqrMagnitude < _minRadius * _minRadius) {
                return false;
            }

            return true;
        }

        private void OnDrawGizmosSelected() {

            Gizmos.matrix = transform.localToWorldMatrix;
            DrawGizmoCircle(_containerRect.rect.center, _minRadius, steps: 32);
            DrawGizmoCircle(_containerRect.rect.center, _maxRadius, steps: 32);
            Gizmos.matrix = Matrix4x4.identity;
        }

        private void DrawGizmoCircle(Vector3 center, float radius, int steps) {

            Vector3 prevPos = new Vector3(radius, 0.0f, 0.0f) + center;
            for (int i = 1; i <= steps; i++) {

                float angle = Mathf.PI * 2.0f * (float)i / steps;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0.0f);
                pos += center;
                Gizmos.DrawLine(prevPos, pos);
                prevPos = pos;
            }
        }
    }

}