using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;

public interface IUnityXRHapticsHandler : IDisposable {

    void TriggerHapticPulse(float strength, float duration);
    void StopHaptics();
}

public class DefaultUnityXRHapticsHandler : IUnityXRHapticsHandler {

    private readonly XRNode _node;

    public DefaultUnityXRHapticsHandler(XRNode node) {

        _node = node;
    }

    public void Destroy() { }

    public void TriggerHapticPulse(float strength, float duration) {

        var device = InputDevices.GetDeviceAtXRNode(_node);
        device.SendHapticImpulse(channel: 0, strength, duration);
    }

    public void StopHaptics() {

        var device = InputDevices.GetDeviceAtXRNode(_node);
        device.StopHaptics();
    }

    public void Dispose() { }
}
public class KnucklesUnityXRHapticsHandler : IUnityXRHapticsHandler  {

    const float kRate = 1 / 80f;

    private readonly MonoBehaviour _coroutineRunner;
    private readonly Coroutine _hapticsCoroutine;
    private readonly XRNode _node;
    private float _remainingTime;
    private float _amplitude;

    public KnucklesUnityXRHapticsHandler(XRNode node, MonoBehaviour coroutineRunner) {

        _node = node;
        _coroutineRunner = coroutineRunner;
        _amplitude = 0;
        _remainingTime = 0;
        _hapticsCoroutine = coroutineRunner.StartCoroutine(HapticsCoroutine());
    }

    public void TriggerHapticPulse(float strength, float duration) {

        _remainingTime = duration * 2;
        _amplitude = Mathf.Clamp01(strength);
    }

    public void StopHaptics() {

        // Only Shorten current remaining time to minimum of haptic refresh rate instead of directly stopping
        // to avoid stopping haptic pulse too soon after it was triggered (as due to different HMD and haptics refresh rate haptic trigger can be delayed)
        _remainingTime = Mathf.Min(_remainingTime, kRate);
    }

    private IEnumerator HapticsCoroutine() {

        var device = InputDevices.GetDeviceAtXRNode(_node);
        var waiter = new WaitForSecondsRealtime(kRate);

        while (true) {

            if (device.isValid && _remainingTime > 0f) {
                device.SendHapticImpulse(0, _amplitude, _remainingTime);
            }

            _remainingTime -= kRate;

            yield return waiter;
        }
        // ReSharper disable once IteratorNeverReturns
    }

    public void Dispose() {

        if (_coroutineRunner) {
            _coroutineRunner.StopCoroutine(_hapticsCoroutine);
        }
    }
}
