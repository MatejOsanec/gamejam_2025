using System;
using UnityEngine;
using UnityEngine.XR;

public class MockVRHelper : IVRPlatformHelper {

#pragma warning disable 67
    public event Action inputFocusWasCapturedEvent;
    public event Action inputFocusWasReleasedEvent;
    public event Action vrFocusWasCapturedEvent;
    public event Action vrFocusWasReleasedEvent;
    public event Action hmdUnmountedEvent;
    public event Action hmdMountedEvent;
    public event Action controllersDidChangeReferenceEvent;
    public event Action controllersDidDisconnectEvent;
#pragma warning restore

    public bool hasInputFocus => true;
    public bool hasVrFocus => true;
    public bool isAlwaysWireless => true;
    public VRPlatformSDK vrPlatformSDK => VRPlatformSDK.Unknown;

    public void TriggerHapticPulse(XRNode node, float duration, float strength, float frequency) { }

    public void StopHaptics(XRNode node) { }

    public bool TryGetPoseOffsetForNode(XRNode node, out Pose poseOffset) {

        poseOffset = Pose.identity;
        return true;
    }

    public bool GetNodePose(XRNode nodeType, int idx, out Vector3 pos, out Quaternion rot) {

        pos = Vector3.zero;
        rot = Quaternion.identity;
        return true;
    }

    public Pose GetRootPositionOffsetForLegacyNodePose(XRNode node) {

        return Pose.identity;
    }

    public bool TryGetLegacyPoseOffsetForNode(XRNode node, out Vector3 position, out Vector3 rotation) {

        position = Vector3.zero;
        rotation = Vector3.zero;
        return true;
    }

    public Vector2 GetAnyJoystickMaxAxis() => Vector2.zero;

    public float GetTriggerValue(XRNode node) => 0.0f;

    public Vector2 GetThumbstickValue(XRNode node) => Vector2.zero;

    public bool IsAdvancedHapticsSupported(XRNode node) => false;

    public bool GetMenuButton() => false;

    public bool GetMenuButtonDown() => false;

    public void RefreshControllersReference() { }
}
