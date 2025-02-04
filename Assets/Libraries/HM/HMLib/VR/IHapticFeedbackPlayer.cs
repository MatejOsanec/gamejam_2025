using Libraries.HM.HMLib.VR;
using UnityEngine.XR;

public interface IHapticFeedbackPlayer {

    public void PlayHapticFeedback(XRNode node, HapticPresetSO hapticPreset);

    public bool CanPlayHapticPreset(HapticPresetSO hapticPreset, XRNode node);
}
