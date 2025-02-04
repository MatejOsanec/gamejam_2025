using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

namespace HMUI {

    public class PanelAnimation : MonoBehaviour {

        public void StartAnimation(CanvasGroup canvasGroup, CanvasGroup parentCanvasGroup, float duration, AnimationCurve scaleXAnimationCurve, AnimationCurve scaleYAnimationCurve, AnimationCurve alphaAnimationCurve, AnimationCurve parentAlphaAnimationCurve, System.Action finishedCallback) {

            if (!gameObject.activeInHierarchy) {
                Destroy(this);
                finishedCallback?.Invoke();
                return;
            }

            StopAllCoroutines();
            StartCoroutine(AnimationCoroutine(duration: duration, canvasGroup, parentCanvasGroup, scaleXAnimationCurve, scaleYAnimationCurve, alphaAnimationCurve, parentAlphaAnimationCurve, finishedCallback));
        }

        public IEnumerator AnimationCoroutine(float duration, CanvasGroup canvasGroup, CanvasGroup parentCanvasGroup, AnimationCurve scaleXAnimationCurve, AnimationCurve scaleYAnimationCurve, AnimationCurve alphaAnimationCurve, AnimationCurve parentAlphaAnimationCurve, System.Action finishedCallback) {

            Transform canvasTransform = canvasGroup.transform;

            float elapsedTime = 0;
            while (elapsedTime < duration) {

                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;

                if (parentCanvasGroup != null) {
                    parentCanvasGroup.alpha = parentAlphaAnimationCurve.Evaluate(t);
                }
                canvasGroup.alpha = alphaAnimationCurve.Evaluate(t);
                float scaleX = scaleXAnimationCurve.Evaluate(t);
                float scaleY = scaleYAnimationCurve.Evaluate(t);
                canvasTransform.localScale = new Vector3(scaleX, scaleY, 1.0f);

                yield return null;
            }

            if (parentCanvasGroup != null) {
                parentCanvasGroup.alpha = parentAlphaAnimationCurve.Evaluate(1.0f);
            }
            canvasGroup.alpha = alphaAnimationCurve.Evaluate(1.0f);
            canvasTransform.localScale = new Vector3(scaleXAnimationCurve.Evaluate(1.0f), scaleYAnimationCurve.Evaluate(1.0f), 1.0f);

            Destroy(this);

            finishedCallback?.Invoke();
        }
    }
}