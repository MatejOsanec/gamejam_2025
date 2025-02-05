using System;
using UnityEngine;


public class DeactivateAnimatorOnInputFocusCapture: MonoBehaviour {

    [SerializeField] Animator _animator = default;

#if !UNITY_EDITOR || !BS_IGNORE_VR_FOCUS_LOST_EVENTS
   private readonly IVRPlatformHelper _vrPlatformHelper = default;
#endif

    private bool _wasEnabled = false;

#if !UNITY_EDITOR || !BS_IGNORE_VR_FOCUS_LOST_EVENTS
    protected void Start() {

        _vrPlatformHelper.inputFocusWasCapturedEvent += HandleInputFocusCaptured;
        _vrPlatformHelper.inputFocusWasReleasedEvent += HandleInputFocusReleased;

        if (!_vrPlatformHelper.hasInputFocus) {
            HandleInputFocusCaptured();
        }
    }

    protected void OnDestroy() {

        if (_vrPlatformHelper != null) {
            _vrPlatformHelper.inputFocusWasCapturedEvent -= HandleInputFocusCaptured;
            _vrPlatformHelper.inputFocusWasReleasedEvent -= HandleInputFocusReleased;
        }
    }
#endif

    private void HandleInputFocusCaptured() {

        _wasEnabled = _animator.enabled;
        _animator.enabled = false;
    }

    private void HandleInputFocusReleased() {

        _animator.enabled = _wasEnabled;
    }
}
