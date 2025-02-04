namespace BGLib.UnityExtension.BezierCurves {

    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Events;
    [ExecuteInEditMode]
    public class CurveAnimation : MonoBehaviour {
        
        [Serializable] public class CurveEvent : UnityEvent { }
        [Serializable] public class CurveEvent<T> : UnityEvent<T> { }

        [SerializeField] protected BaseBezierCurve _bezierCurve;
        [SerializeField] protected float _duration;
        [SerializeField] protected float _delay;
        [SerializeField] protected AnimationCurve _speedCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
        [SerializeField] protected bool _playOnAwake = true;
        [SerializeField] float _progress;
        private float _previousProgress;
        public CurveEvent onStart;
        public CurveEvent afterDelay;
        public CurveEvent<Vector3> onIterate;
        public CurveEvent onFinish;

        public bool isPlaying { get; private set; } = false;
        

        protected void LateUpdate() {

            if (_progress != _previousProgress) {
                _previousProgress = _progress;
                Animate(_progress);
            }
        }

        private void Awake() {

            if (!_playOnAwake) {
                return;
            }

            StartAnimation();
        }

        public void StartAnimation(bool withDelay = true) {

            if (isPlaying) {
                return;
            }

            StartCoroutine(Animate(withDelay));
        }

        internal IEnumerator Animate(bool withDelay) {

            isPlaying = true;
            onStart.Invoke();
            if (withDelay) {
                yield return new WaitForSeconds(_delay);
            }
            afterDelay.Invoke();
            var currentTime = 0f;
            while (currentTime < _duration) {
                Animate(currentTime / _duration);
                currentTime += Time.deltaTime;
                yield return null;
            }
            Animate(1);
            onFinish.Invoke();
            isPlaying = false;

        }

        private void Animate(float t) {

            var speedValue = _speedCurve.Evaluate(t);
            var bezierValue = _bezierCurve.Evaluate(speedValue);
            onIterate.Invoke(bezierValue);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos() {

            // Ensure continuous Update calls.
            if (Application.isPlaying) {
                return;
            }
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            UnityEditor.SceneView.RepaintAll();
        }
#endif
    }
}
