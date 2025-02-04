using UnityEngine;
using System.Collections;

public class FadeInOutController : MonoBehaviour {

    [SerializeField] FloatSO _easeValue = default;
    [SerializeField] AnimationCurve _fadeInCurve = default;
    [SerializeField] AnimationCurve _fadeOutCurve = default;
    [SerializeField] float _fadeInStartDelay = 0.1f;
    [SerializeField] float _defaultFadeOutDuration = 1.3f;
    [SerializeField] float _defaultFadeInDuration = 1.0f;

    public bool inTransition { get; private set; }

    public void FadeOutInstant() {

        FadeOut(duration: 0.0f, null);
    }

    public void FadeIn() {

        FadeIn(_defaultFadeInDuration, null);
    }

    public void FadeOut() {

        FadeOut(_defaultFadeOutDuration, null);
    }

    public void FadeIn(float duration) {

        FadeIn(duration, fadeInFinishedCallback: null);
    }

    public void FadeOut(float duration) {

        FadeOut(duration, fadeOutFinishedCallback: null);
    }

    public void FadeIn(System.Action fadeInCallback) {

        FadeIn(_defaultFadeInDuration, fadeInCallback);
    }

    public void FadeOut(System.Action fadeOutCallback) {

        FadeOut(_defaultFadeInDuration, fadeOutCallback);
    }

    public void FadeIn(float duration, System.Action fadeInFinishedCallback) {

        StopAllCoroutines();

        if (duration == 0) {
            _easeValue.value = 1.0f;
        }
        else {
            _easeValue.value = 0.0f;
            StartCoroutine(Fade(0.0f, 1.0f, duration, startDelay: _fadeInStartDelay, curve: _fadeInCurve, fadeInFinishedCallback));
        }
    }

    public void FadeOut(float duration, System.Action fadeOutFinishedCallback) {

        StopAllCoroutines();

        if (duration == 0) {
            _easeValue.value = 0.0f;
        }
        else {
           StartCoroutine(Fade(_easeValue.value, 0.0f, duration, startDelay: 0.0f, curve: _fadeOutCurve, fadeOutFinishedCallback));
        }
    }

    private IEnumerator Fade(float fromValue, float toValue, float duration, float startDelay, AnimationCurve curve, System.Action fadeFinishedCallback) {

        inTransition = true;
        if (startDelay > 0.0f) {
            yield return new WaitForSeconds(startDelay);
        }

        float elapsedTime = 0.0f;
        while (elapsedTime < duration) {
            _easeValue.value = Mathf.Lerp(fromValue, toValue, curve.Evaluate(elapsedTime / duration));
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        _easeValue.value = toValue;
        inTransition = false;
        fadeFinishedCallback?.Invoke();
    }
}
