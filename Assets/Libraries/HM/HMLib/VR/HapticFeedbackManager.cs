using Libraries.HM.HMLib.VR;
using UnityEngine;
using UnityEngine.XR;


public class HapticFeedbackManager : MonoBehaviour {

   RumbleHapticFeedbackPlayer _rumbleHapticFeedbackPlayer;
   IHapticFeedbackPlayer _advancedHapticFeedbackPlayer;

    public bool hapticFeedbackEnabled = false;

    public void PlayHapticFeedback(XRNode node, HapticPresetSO hapticPreset) {

        if (!hapticFeedbackEnabled) {
            return;
        }

        if (hapticPreset._useAdvancedHapticsOnSupportedPlatforms && _advancedHapticFeedbackPlayer.CanPlayHapticPreset(hapticPreset, node)) {
            _advancedHapticFeedbackPlayer.PlayHapticFeedback(node,hapticPreset);
            return;
        }

        _rumbleHapticFeedbackPlayer.PlayHapticFeedback(node,hapticPreset);
    }
}
