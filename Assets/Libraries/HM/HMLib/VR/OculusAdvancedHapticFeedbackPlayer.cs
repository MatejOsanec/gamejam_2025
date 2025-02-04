#if BS_OCULUS_VR

using System;
using System.Collections.Generic;
using Libraries.HM.HMLib.VR;
using Oculus.Haptics;
using UnityEngine;
using UnityEngine.XR;
using Zenject;

public class OculusAdvancedHapticFeedbackPlayer: IHapticFeedbackPlayer, IInitializable, IDisposable, ITickable {

    [Inject] readonly IVRPlatformHelper _vrPlatformHelper;
    // This can theoretically be null on Oculus with OpenXR = Oculus PC
    // TODO: Change this if we want to support Touch controllers via Link after SDK v62 update
    [InjectOptional] readonly OculusVRHelper _oculusVRHelper;

    private readonly Dictionary<(HapticPresetSO, XRNode), HapticPlayerState> _hapticPlayerStatesDictionary = new();
    // Optimization, so we don't loop through all _hapticPlayerStatesDictionary entries
    // every frame even if there is no looping clips in there
    private bool _hasAtLeastOneLoopingClipPlaying = false;
    private bool? _isLeftHandSupported;
    private bool? _isRightHandSupported;

    private class HapticPlayerState {

        public HapticClipPlayer player;
        public int lastFrameTriggered;
        public bool isPlayingLoopingClip;
    }

    public void PlayHapticFeedback(XRNode node, HapticPresetSO hapticPreset) {

        var dictionaryKey = (hapticPreset, node);
        if (!_hapticPlayerStatesDictionary.TryGetValue(dictionaryKey, out var hapticPlayerState)) {
            hapticPlayerState = new HapticPlayerState() {
                player = new HapticClipPlayer(GetHapticClip(node, hapticPreset)),
                lastFrameTriggered = -1,
                isPlayingLoopingClip = false
            };
            _hapticPlayerStatesDictionary[dictionaryKey] = hapticPlayerState;
            hapticPlayerState.player.priority = hapticPreset._priority;
            hapticPlayerState.player.isLooping = hapticPreset._continuous;
        }

        Controller controller = Controller.Right;
        switch (node) {
            case XRNode.LeftHand:
                controller = Controller.Left;
                break;
            case XRNode.RightHand:
                controller = Controller.Right;
                break;
            default:
                Debug.LogError($"Cannot convert node {node} to Controller");
                return;
        }
        if (hapticPlayerState.player.isLooping) {
            // Do not re-play looping effect, just store last triggered frame
            var dontRestartLoopingEffect = hapticPlayerState.isPlayingLoopingClip;
            hapticPlayerState.lastFrameTriggered = Time.frameCount;
            if (dontRestartLoopingEffect) {
                return;
            }
            _hasAtLeastOneLoopingClipPlaying = true;
            hapticPlayerState.isPlayingLoopingClip = true;
        }
        hapticPlayerState.player.Play(controller);
    }

    public bool CanPlayHapticPreset(HapticPresetSO hapticPreset, XRNode node) {

        // Only left and right controllers are currently supported
        if (node != XRNode.LeftHand && node != XRNode.RightHand) {
            return false;
        }
        if (node == XRNode.LeftHand && !_isLeftHandSupported.HasValue) {
            _isLeftHandSupported = _vrPlatformHelper.IsAdvancedHapticsSupported(node);
        } else if (node == XRNode.RightHand && !_isRightHandSupported.HasValue) {
            _isRightHandSupported = _vrPlatformHelper.IsAdvancedHapticsSupported(node);
        }
        bool isHandSupported = (node == XRNode.LeftHand ? _isLeftHandSupported : _isRightHandSupported) ?? false;
        return hapticPreset.hasOculusHapticsClip && isHandSupported;
    }

    public void Initialize() {

        Application.quitting += HandleApplicationQuitting;
    }

    public void Dispose() {

        Application.quitting -= HandleApplicationQuitting;
        foreach (var kvp in _hapticPlayerStatesDictionary) {
            kvp.Value.player.Dispose();
        }
        _hapticPlayerStatesDictionary.Clear();
    }

    public void Tick() {

        if (!_hasAtLeastOneLoopingClipPlaying) {
            return;
        }

        _hasAtLeastOneLoopingClipPlaying = false;
        foreach (var kvp in _hapticPlayerStatesDictionary) {
            var hapticPlayerState = kvp.Value;
            if (!hapticPlayerState.isPlayingLoopingClip) {
                continue;
            }

            var frameDiff = Time.frameCount - hapticPlayerState.lastFrameTriggered;
            // 2 because we trigger continuous haptic every frame = missed frame => stop it.
            // 2 and not 1 is due to execution order, if this goes before the script updating the continuous haptic
            // it would be always 1
            if (frameDiff >= 2) {
                hapticPlayerState.player.Stop();
                hapticPlayerState.isPlayingLoopingClip = false;
            }
            else {
                _hasAtLeastOneLoopingClipPlaying = true;
            }
        }
    }

    private HapticClip GetHapticClip(XRNode node, HapticPresetSO hapticPreset) {

        if (hapticPreset._overrideForTouchController &&
            _oculusVRHelper != null &&
            _oculusVRHelper.GetInteractionProfile(node) == OVRInput.InteractionProfile.Touch) {

            return hapticPreset._touchControllerOverrideHapticsClip;
        }

        return hapticPreset._oculusHapticsClip;
    }

    private void HandleApplicationQuitting() {

        // We don't want to dispose on unsupported platform or if we never used = never initialized
        if ((_isLeftHandSupported ?? false) || (_isRightHandSupported ?? false)) {
            Haptics.Instance.Dispose();
        }
    }
}

#endif
