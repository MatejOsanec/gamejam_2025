using System.Collections;
using UnityEngine;


public class BloomFogParamsAnimator: MonoBehaviour {

   readonly BloomFogSO _bloomFog = default;

    public void AnimateBloomFogParamsChange(BloomFogEnvironmentParams envFogParams, float duration) {

        StopAllCoroutines();
        _bloomFog.transition = 0.0f;
        _bloomFog.transitionFogParams = envFogParams;
        StartCoroutine(AnimationCoroutine(envFogParams, duration));
    }

    private IEnumerator AnimationCoroutine(BloomFogEnvironmentParams envFogParams, float duration) {

        float elapsedTime = 0.0f;

        while (elapsedTime < duration) {

            float t = elapsedTime / duration;
            _bloomFog.transition = t;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _bloomFog.transition = 0.0f;
        _bloomFog.defaultForParams = envFogParams;
    }
    
    public void SetBloomFogParamsChange(BloomFogEnvironmentParams envFogParams, float transition) {

        _bloomFog.transition = transition;
        _bloomFog.transitionFogParams = envFogParams;
    }

    public BloomFogEnvironmentParams GetDefaultBloomFogParams() {
        return _bloomFog.defaultForParams;
    }
    
    public void SetDefaultBloomFogParams(BloomFogEnvironmentParams newDefaultBloomFogParams) {
        _bloomFog.defaultForParams = newDefaultBloomFogParams ;
    }
}
