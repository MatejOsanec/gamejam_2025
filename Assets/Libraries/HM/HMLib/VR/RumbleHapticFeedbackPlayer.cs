using System.Collections.Generic;
using Libraries.HM.HMLib.VR;
using UnityEngine;
using UnityEngine.XR;


public class RumbleHapticFeedbackPlayer : MonoBehaviour, IHapticFeedbackPlayer {

   readonly IVRPlatformHelper _vrPlatformHelper = default;

    private class RumbleData {
        public bool active;
        public bool continuous;
        public float strength;
        public float endTime;
        public float frequency;
    }

    private const float kContinuousRumbleFrameDuration = 1.0f / 60.0f;

    private readonly Dictionary<XRNode, Dictionary<object, RumbleData>> _rumblesByNode = new Dictionary<XRNode, Dictionary<object, RumbleData>>();

    public void PlayHapticFeedback(XRNode node, HapticPresetSO hapticPreset) {

        var rumble = GetRumble(node, hapticPreset);
        if (rumble == null || hapticPreset == null) {
            return;
        }

        rumble.active = true;
        rumble.continuous = hapticPreset._continuous;
        rumble.strength = hapticPreset._strength;
        rumble.endTime = Time.time + hapticPreset._duration;
        rumble.frequency = hapticPreset._frequency;
    }

    public bool CanPlayHapticPreset(HapticPresetSO hapticPreset, XRNode node) {

        return hapticPreset._duration > 0.0f && hapticPreset._frequency > 0.0f && hapticPreset._strength > 0.0f;
    }

    private void LateUpdate() {

        UpdateRumbles();
    }

    private void UpdateRumbles() {

        foreach (var rumbles in _rumblesByNode) {

            var node = rumbles.Key;
            bool applyRumble = false;
            float strength = 0.0f;
            float duration = 0.0f;
            float frequency = 0.0f;

            foreach (var rumble in rumbles.Value.Values) {

                if (!rumble.active) {
                    continue;
                }

                if (rumble.strength < strength) {
                    continue;
                }

                if (rumble.continuous) {
                    rumble.active = false;
                    duration = kContinuousRumbleFrameDuration;
                }
                else {
                    if (rumble.endTime < Time.time) {
                        rumble.active = false;
                        continue;
                    }

                    duration = rumble.endTime - Time.time;
                }

                applyRumble = true;
                strength = rumble.strength;
                frequency = rumble.frequency;
            }

            if (applyRumble) {
                _vrPlatformHelper.TriggerHapticPulse(node, duration, strength, frequency);
            }
            else {
                // TODO - Do not call StopHaptics for each rumble that is not active each frame
                // Call it only once when the rumble should end
                _vrPlatformHelper.StopHaptics(node);
            }
        }
    }

    private RumbleData GetRumble(XRNode node, object preset) {

        if (!_rumblesByNode.TryGetValue(node, out var rumbles)) {
            rumbles = new Dictionary<object, RumbleData>();
            _rumblesByNode[node] = rumbles;
        }

        rumbles.TryGetValue(preset, out var rumble);
        if (rumble == null) {
            rumble = new RumbleData();
            rumbles[preset] = rumble;
        }

        return rumble;
    }
}
