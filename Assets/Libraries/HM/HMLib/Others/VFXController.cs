using System.Collections;
using UnityEngine;

public class VFXController : MonoBehaviour {

    [SerializeField] [NullAllowed] ParticleSystem[] _particleSystems = default;
    [SerializeField] [NullAllowed] Animation _animation = default;
    [SerializeField] bool _deactivateAfterAnimationDuration = default;

#pragma warning disable 109
    public new Animation animation => _animation;
#pragma warning  restore 109
    public ParticleSystem[] particleSystems => _particleSystems;

    protected void Awake() {

        gameObject.SetActive(false);
    }

    public void Play() {

        gameObject.SetActive(true);

        if (_deactivateAfterAnimationDuration && _animation != null && _animation.clip != null) {
            StartCoroutine(MainCoroutine(deactivateAfterDuration: true, duration: _animation.clip.length));
        }
        else {
            StartCoroutine(MainCoroutine(deactivateAfterDuration: false, duration: 0.0f));
        }
    }

    private IEnumerator MainCoroutine(bool deactivateAfterDuration, float duration) {

        if (_particleSystems.Length > 0) {
            foreach (var ps in _particleSystems) {
                ps.Play(withChildren: false);
            }
        }

        if (_animation != null) {
            _animation.Rewind();
            _animation.Play();
        }

        if (deactivateAfterDuration) {
            yield return new WaitForSeconds(duration);
            gameObject.SetActive(false);
        }
    }
}
