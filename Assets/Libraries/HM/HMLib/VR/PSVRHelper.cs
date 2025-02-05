using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;


public class PSVRHelper : MonoBehaviour, IVRPlatformHelper {

    private const float kContinuesRumbleImpulseStrength = 0.8f;

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

    public bool hasInputFocus => _hasInputFocus;
    public bool hasVrFocus => _hasVrFocus;
    public bool isAlwaysWireless => false;
    public VRPlatformSDK vrPlatformSDK => VRPlatformSDK.Unknown;

    private bool _didGetNodeStatesThisFrame;
    private readonly List<XRNodeState> _nodeStates = new List<XRNodeState>(10);
#pragma warning disable 414
    private bool _hasInputFocus;
    private bool _hasVrFocus = true;
    private bool _isMounted = true;
#pragma warning restore

#if UNITY_PS4
   PSVRDeviceManager _psvrDeviceManager;
#endif

    private void Start() {

#if UNITY_PS4
        _psvrDeviceManager.moveDeviceDidDisconnectEvent += HandleMoveDeviceDidDisconnectEvent;
#endif
    }

    private void OnDestroy() {

#if UNITY_PS4
        _psvrDeviceManager.moveDeviceDidDisconnectEvent -= HandleMoveDeviceDidDisconnectEvent;
#endif
    }

    private void HandleMoveDeviceDidDisconnectEvent() {

        controllersDidDisconnectEvent?.Invoke();
    }

    protected void Update() {

#if UNITY_EDITOR
        if (Application.isPlaying) {
            return;
        }
#endif
#if UNITY_PS4
        bool hadVrFocus = _hasVrFocus;
        _hasVrFocus = !UnityEngine.PS4.Utility.isInBackgroundExecution;
        if (hadVrFocus && !_hasVrFocus) {
            vrFocusWasCapturedEvent?.Invoke();
        }
        else if (!hadVrFocus && _hasVrFocus) {
            vrFocusWasReleasedEvent?.Invoke();
        }

        bool hadInputFocus = _hasInputFocus;
        _hasInputFocus = !UnityEngine.PS4.Utility.isSystemUiOverlaid;
        if (hadInputFocus && !_hasInputFocus) {
            inputFocusWasCapturedEvent?.Invoke();
        }
        else if (!hadInputFocus && _hasInputFocus) {
            inputFocusWasReleasedEvent?.Invoke();
        }

        bool wasMounted = _isMounted;
        _isMounted = UnityEngine.PS4.VR.PlayStationVR.hmdMount == UnityEngine.PS4.VR.VRHmdMountStatus.Mount;
        if (wasMounted && !_isMounted) {
            hmdUnmountedEvent?.Invoke();
        }
        else if (!wasMounted && _isMounted) {
            hmdMountedEvent?.Invoke();
        }
#endif
    }

    protected void LateUpdate() {

        _didGetNodeStatesThisFrame = false;
    }

    public void TriggerHapticPulse(XRNode node, float duration, float strength, float frequency) {

        strength *= kContinuesRumbleImpulseStrength;
#if UNITY_PS4
        _psvrDeviceManager.SetPSMoveVibration(node == XRNode.RightHand ? 0 : 1, strength);
#endif
    }

    public void StopHaptics(XRNode node) { }

    public bool TryGetPoseOffsetForNode(XRNode node, out Pose poseOffset) {

        poseOffset = Pose.identity;
        return true;
    }

    private static int XRNodeToPSDeviceIndex(XRNode node) {

        switch (node) {
            case XRNode.LeftHand: return 1;
            case XRNode.RightHand: return 0;
        }

        Debug.LogError($"Can not convert XRNode ({node}) to PSMoveDeviceIndex.");
        return -1;
    }

    public bool GetNodePose(XRNode nodeType, int idx, out Vector3 pos, out Quaternion rot) {

        pos = Vector3.zero;
        rot = Quaternion.identity;
#if UNITY_EDITOR
        if (Application.isPlaying) {
            return true;
        }
#endif

        if (nodeType == XRNode.LeftHand || nodeType == XRNode.RightHand) {
            int deviceIndex = XRNodeToPSDeviceIndex(nodeType);
#if UNITY_PS4
            pos = _psvrDeviceManager.GetMovePosition(deviceIndex);
            rot = _psvrDeviceManager.GetMoveRotation(deviceIndex);
#endif
        }
        else {
            if (!_didGetNodeStatesThisFrame) {
                InputTracking.GetNodeStates(_nodeStates);
                _didGetNodeStatesThisFrame = true;
            }

            pos = Vector3.zero;
            rot = Quaternion.identity;
            int j = 0;
            foreach (var nodeState in _nodeStates) {
                if (nodeState.nodeType == nodeType && idx == j) {
                    bool posGet = nodeState.TryGetPosition(out pos);
                    bool rotGet = nodeState.TryGetRotation(out rot);
                    return posGet && rotGet;
                }

                if (nodeState.nodeType == nodeType) {
                    j++;
                }
            }
        }

        return pos != Vector3.zero;
    }

    public Pose GetRootPositionOffsetForLegacyNodePose(XRNode node) {

        return Pose.identity;
    }

    public bool TryGetLegacyPoseOffsetForNode(XRNode node, out Vector3 position, out Vector3 rotation) {

        position = Vector3.zero;
        rotation = Vector3.zero;
        return true;
    }

    public Vector2 GetAnyJoystickMaxAxis() {

        return Vector2.zero;
    }

    public float GetTriggerValue(XRNode node) {

        return node switch {
#if UNITY_PS4
            XRNode.LeftHand => _psvrDeviceManager.GetPSMoveAnalog(1),
            XRNode.RightHand => _psvrDeviceManager.GetPSMoveAnalog(0),
#endif
            _ => 0
        };
    }

    public Vector2 GetThumbstickValue(XRNode node) {

        return Vector2.zero;
    }

    public bool IsAdvancedHapticsSupported(XRNode node) {

        return false;
    }

    public bool GetMenuButton() => VRPlatformUtils.GetMenuButtonDefaultImplementation();

    public bool GetMenuButtonDown() => VRPlatformUtils.GetMenuButtonDownDefaultImplementation();

    public void RefreshControllersReference() {

        controllersDidChangeReferenceEvent?.Invoke();
    }
}
