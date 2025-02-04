using UnityEngine;
#if BS_OCULUS_VR
using Oculus.Haptics;
#endif

namespace Libraries.HM.HMLib.VR {

    public class HapticPresetSO : ScriptableObject {

        [DrawIf("_continuous", value: false)]
        public float _duration = 0.05f;
        public float _strength = 1.0f;
        public float _frequency = 0.5f;
        public bool _continuous;
        public bool _useAdvancedHapticsOnSupportedPlatforms = true;

        [Space]
        [Header("PS5 Haptics")]
        [DrawIf("_useAdvancedHapticsOnSupportedPlatforms", value: true)]
        [NullAllowedIf(nameof(_useAdvancedHapticsOnSupportedPlatforms), equalsTo: false)]
        public AudioClip _ps5HapticsClip;

        public bool hasPS5HapticsClip {
            get {

                _hasPS5HapticsClip ??= _ps5HapticsClip != null;
                return _hasPS5HapticsClip.Value;
            }
        }
        private bool? _hasPS5HapticsClip;

#if BS_OCULUS_VR
        [Space]
        [Header("Oculus Advanced Haptics")]
        [DrawIf("_useAdvancedHapticsOnSupportedPlatforms", value: true)]
        [NullAllowedIf(nameof(_useAdvancedHapticsOnSupportedPlatforms), equalsTo: false)]
        public HapticClip _oculusHapticsClip;

        [DrawIf("_useAdvancedHapticsOnSupportedPlatforms", value: true)]
        [Tooltip("Priority values can be on the range of 0 (high priority) to 255 (low priority)")]
        [Range(0, 255)]
        public uint _priority = 128;

        // Generalise this for n overrides if needed in the future
        public bool _overrideForTouchController = false;
        [DrawIf("_overrideForTouchController", value: true)]
        [NullAllowedIf(nameof(_overrideForTouchController), equalsTo: false)]
        public HapticClip _touchControllerOverrideHapticsClip;

        public bool hasOculusHapticsClip {
            get {

                _hasOculusHapticsClip ??= _oculusHapticsClip != null;
                return _hasOculusHapticsClip.Value;
            }
        }
        private bool? _hasOculusHapticsClip;
#endif
    }
}
