#if BS_OPENXR_VR

using System;
using UnityEngine.XR.OpenXR.Features;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

/// <summary>
/// Beat Saber Feature for OpenXR
/// </summary>
#if UNITY_EDITOR
[OpenXRFeature(UiName = "Beat Saber XR Feature",
    BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.Android },
    Company = "Beat Saber",
    Desc = "Beat Saber Feature for OpenXR.",
    Version = "1.0.0",
    FeatureId = featureId)]
#endif
public class BeatSaberXRFeature: OpenXRFeature {

    // https://registry.khronos.org/OpenXR/specs/1.0/man/html/XrSessionState.html
    public enum SessionState {
        Unknown = 0,
        Idle = 1,
        Ready = 2,
        Synchronized = 3,
        Visible = 4,
        Focused = 5,
        Stopping = 6,
        Pending = 7,
        Exiting = 8
    }

    public SessionState currentSessionState { get; private set; }

    public event Action<SessionState, SessionState> sessionStateChangedEvent;

    /// <summary>
    /// The feature id string. This is used to give the feature a well known id for reference.
    /// </summary>
    private const string featureId = "com.beatgames.beatsaber.feature.beatsaberxr";

    protected override void OnSessionStateChange(int oldState, int newState) {

        base.OnSessionStateChange(oldState, newState);
        var oldValue = (SessionState)oldState;
        var newValue = (SessionState)newState;
        currentSessionState = newValue;
        sessionStateChangedEvent?.Invoke(oldValue, newValue);
    }
}
#endif
