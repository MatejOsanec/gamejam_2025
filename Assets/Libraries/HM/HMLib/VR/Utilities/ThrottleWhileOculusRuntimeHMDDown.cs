#if BS_OPENXR_VR && UNITY_STANDALONE

using System;
using System.Linq;
using UnityEngine;


// Fix (workaround) of https://beatgames.atlassian.net/browse/USS-1601
// The problem was, that while using Oculus OpenXR Runtime at PC, if HMD is Down, game tries to render as many frames
// as it can, resulting in 100% GPU usage. So this piece of code aims to temp fix it
public class ThrottleWhileOculusRuntimeHMDDown: IInitializable, ITickable, IDisposable {

   readonly IVRPlatformHelper _platformHelper;
   readonly TickableManager _tickableManager;

    public int overrideThrottlingRefreshRate = -1;

    private const string kOculusRuntimeName = "Oculus";
    private const int kDefaultRefreshRate = 72;

    public void Initialize() {

        if (UnityEngine.XR.OpenXR.OpenXRRuntime.name != kOculusRuntimeName) {
            return;
        }
        _platformHelper.inputFocusWasReleasedEvent += HandlePlatformHelperInputFocusWasReleased;
        _tickableManager.Add(this);
    }

    public void Dispose() {

        if (_platformHelper != null) {
            _platformHelper.inputFocusWasReleasedEvent -= HandlePlatformHelperInputFocusWasReleased;
        }
    }

    public void Tick() {

        // Based on my testing this needs to happen every frame, if done once, it doesn't work.
        // It seems it's constantly being overriden somewhere else as well
        if (!_platformHelper.hasInputFocus) {
            Application.targetFrameRate = overrideThrottlingRefreshRate > 0 ? overrideThrottlingRefreshRate : GetMaxCurrentResolutionRefreshRate();
        }
    }

    private void HandlePlatformHelperInputFocusWasReleased() {

        Application.targetFrameRate = -1;
    }

    private int GetMaxCurrentResolutionRefreshRate() {

        var currentResolution = Screen.currentResolution;
        var maxRefreshRate = 0;
        foreach (var resolution in Screen.resolutions) {
            if (resolution.width != currentResolution.width || resolution.height != currentResolution.height) {
                continue;
            }
            maxRefreshRate = Mathf.Max(Mathf.RoundToInt((float)resolution.refreshRateRatio.value), maxRefreshRate);
        }
        return maxRefreshRate > 0 ? maxRefreshRate : kDefaultRefreshRate;
    }
}
#endif
