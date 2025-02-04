using UnityEngine;

namespace HMUI {

    public class PanelAnimationSO : ScriptableObject {

        [SerializeField] float _duration = 0.3f;
        [SerializeField] AnimationCurve _scaleXAnimationCurve = AnimationCurve.EaseInOut(timeStart: 0.0f, valueStart: 0.0f, timeEnd: 0.0f, valueEnd: 0.0f);
        [SerializeField] AnimationCurve _scaleYAnimationCurve = AnimationCurve.EaseInOut(timeStart: 0.0f, valueStart: 0.0f, timeEnd: 0.0f, valueEnd: 0.0f);
        [SerializeField] AnimationCurve _alphaAnimationCurve = AnimationCurve.EaseInOut(timeStart: 0.0f, valueStart: 0.0f, timeEnd: 0.0f, valueEnd: 0.0f);
        [SerializeField] AnimationCurve _parentAlphaAnimationCurve = AnimationCurve.EaseInOut(timeStart: 0.0f, valueStart: 0.0f, timeEnd: 0.0f, valueEnd: 0.0f);

        public void ExecuteAnimation(GameObject go) {

            ExecuteAnimation(go, parentCanvasGroup: null, instant: false, finishedCallback: null);
        }

        public void ExecuteAnimation(GameObject go, System.Action finishedCallback) {

            ExecuteAnimation(go, parentCanvasGroup: null, instant: false, finishedCallback);
        }

        public void ExecuteAnimation(GameObject go, CanvasGroup parentCanvasGroup, System.Action finishedCallback) {

            ExecuteAnimation(go, parentCanvasGroup, instant: false, finishedCallback);
        }

        public void ExecuteAnimation(GameObject go, CanvasGroup parentCanvasGroup, bool instant, System.Action finishedCallback) {

            var canvasGroup = EssentialHelpers.GetOrAddComponent<CanvasGroup>(go);
            var animationExecutor = EssentialHelpers.GetOrAddComponent<PanelAnimation>(go);
            animationExecutor.StartAnimation(
                canvasGroup,
                parentCanvasGroup,
                instant ? 0.0f : _duration,
                _scaleXAnimationCurve,
                _scaleYAnimationCurve,
                _alphaAnimationCurve,
                _parentAlphaAnimationCurve,
                finishedCallback: finishedCallback
            );
        }
    }
}
