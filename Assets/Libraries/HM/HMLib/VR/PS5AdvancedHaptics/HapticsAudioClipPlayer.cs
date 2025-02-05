using System;
using System.Collections;
using Libraries.HM.HMLib.VR;
using UnityEngine;
using UnityEngine.XR;


public class HapticsAudioClipPlayer : MonoBehaviour {

    [SerializeField] AudioSource _audioSource;
    private float _baseVolume;
    private bool _triggeredThisFrame = false;
    private float _lastTriggerTime;

    private const float kContinuousRumbleFadeDuration = 1.0f / 60.0f;

    public class Pool : MemoryPool<HapticsAudioClipPlayer>
    {
        protected override void OnCreated(HapticsAudioClipPlayer item)
        {
            item.Initialize();
        }

        protected override void OnDestroyed(HapticsAudioClipPlayer item)
        {
            GameObject.Destroy(item.gameObject);
        }

        protected override void OnDespawned(HapticsAudioClipPlayer item) {

            item.ForceStopPlaying();
        }

        protected override void Reinitialize(HapticsAudioClipPlayer clipPlayer)
        {
            clipPlayer.Reset();
        }
    }

    public void PlayHapticsPreset(XRNode onNode, HapticPresetSO preset, Action<HapticsAudioClipPlayer> onComplete) {

        if (_audioSource.isPlaying) {

            Debug.LogError("Trying to play haptics on HapticsAudioClipPlayer that is already playing something. Should not happen.");
            StopAllCoroutines();
            Reset();
        }

        _audioSource.panStereo = GetPanForNode(onNode);
        _audioSource.clip = preset._ps5HapticsClip;
        _audioSource.loop = preset._continuous;
#if UNITY_PS5
        _audioSource.PlayOnGamepad(0);
#endif

        _triggeredThisFrame = true;
        _lastTriggerTime = Time.time;

        StartCoroutine(preset._continuous ? HandleContinuousAudioCoroutine(onComplete) : HandleOneShotPlayEndCoroutine(onComplete));
    }

    public void TriggerContinuousHaptic() {

        _triggeredThisFrame = true;
        _lastTriggerTime = Time.time;
        _audioSource.volume = _baseVolume;
    }


    public void RestartHaptic() {

#if UNITY_PS5
        _audioSource.PlayOnGamepad(0);
#endif
    }

    private IEnumerator HandleContinuousAudioCoroutine(Action<HapticsAudioClipPlayer> onComplete) {

        yield return null;
        float timeSinceEnd = 0;
        WaitForEndOfFrame waitForLateUpdate = new WaitForEndOfFrame();
        while (timeSinceEnd < kContinuousRumbleFadeDuration) {

            yield return waitForLateUpdate;

            timeSinceEnd = Time.time - _lastTriggerTime;

            if (!_triggeredThisFrame) {
                UpdateFadeVolume(timeSinceEnd);
            }
            _triggeredThisFrame = false;
        }

        _audioSource.Stop();
        onComplete.Invoke(this);
    }

    private void UpdateFadeVolume(float timeSinceEnd) {

        float volumeFadeValue = 1.0f - Mathf.Clamp01(timeSinceEnd / kContinuousRumbleFadeDuration);
        _audioSource.volume = volumeFadeValue * _baseVolume;
    }

    private IEnumerator HandleOneShotPlayEndCoroutine(Action<HapticsAudioClipPlayer> onComplete) {

        yield return null;
        yield return new WaitUntil(() => !_audioSource.isPlaying);
        onComplete.Invoke(this);
    }

    private void Reset() {

        _audioSource.Stop();
        _audioSource.loop = false;
        _audioSource.volume = _baseVolume;
        _triggeredThisFrame = false;
        _lastTriggerTime = 0;
    }

    private void ForceStopPlaying() {

        _audioSource.Stop();
        StopAllCoroutines();
    }

    private void Initialize() {

        _baseVolume = _audioSource.volume;
        _audioSource.bypassReverbZones = true;
#if UNITY_PS5
        _audioSource.gamepadSpeakerOutputType = GamepadSpeakerOutputType.SecondaryVibration;
#endif
    }

    private float GetPanForNode(XRNode node) {

        switch (node) {
            case XRNode.LeftHand:
                return -1.0f;
            case XRNode.RightHand:
                return 1.0f;
            //Both hands
            case XRNode.GameController:
                return 0.0f;
            default:
                Debug.LogError($"Unsupported node for advanced haptics {node.ToString()}");

                return 0.0f;
        }
    }
}
