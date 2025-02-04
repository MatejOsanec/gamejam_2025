using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomizedPitch : MonoBehaviour {

    [SerializeField] AudioSource _audioSource = default;
    [SerializeField] float _minPitchMultiplier = 0.8f;
    [SerializeField] float _maxPitchMultiplier = 1.2f;
    [SerializeField] bool _playOnAwake = false;
    
    float _originalPitch = 1.0f;
    private Coroutine _restoringCoroutine;

    protected void OnEnable() {

        if (_audioSource.playOnAwake) {
            _playOnAwake = true;
            _audioSource.playOnAwake = false;
        }
        if (_playOnAwake) {
            Play();
        }
    }

    public void Play() {

        if (_restoringCoroutine != null) {
            StopCoroutine(_restoringCoroutine);
            _audioSource.pitch = _originalPitch;
        }
        
        _originalPitch = _audioSource.pitch;   
        _audioSource.pitch = _originalPitch * Random.Range(_minPitchMultiplier, _maxPitchMultiplier);
        _audioSource.Play();
        _restoringCoroutine = StartCoroutine(RestorePitchWithDelay(_audioSource.clip.length));
    }

    public void PlayDelayed(float delay) {
        
        if (delay > 0.0f) {
            StartCoroutine(PlayDelayedCoroutine(delay));
        }
        else {
            Play();
        }
    }

    private IEnumerator PlayDelayedCoroutine(float delay) {
        
        yield return new WaitForSeconds(delay);
        Play();
    }
    
    private IEnumerator RestorePitchWithDelay(float delay) {
        
        yield return new WaitForSeconds(delay);
        _audioSource.pitch = _originalPitch;
    }
}
