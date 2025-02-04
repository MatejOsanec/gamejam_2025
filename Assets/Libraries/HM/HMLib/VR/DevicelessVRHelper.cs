using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR;

public class DevicelessVRHelper : MonoBehaviour, IVRPlatformHelper, IVerboseLogger {

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
    public bool isAlwaysWireless => true;
    public VRPlatformSDK vrPlatformSDK => VRPlatformSDK.Unknown;

    public string loggerPrefix => "DevicelessVRHelper";

#if UNITY_EDITOR
    private MonoBehaviour _firstPersonFlyingController = default;
    private MonoBehaviour _controllersRecorder = default;
    private PropertyInfo _controllersRecorderGetPosesPropertyInfo;
    private bool _triedToFindHead = false;
#endif

    private bool _hasInputFocus = true;
    private bool _hasVrFocus = true;
    private bool _scrollingLastFrame;

    protected void Update() {

        if (Input.GetKeyDown(KeyCode.U)) {
            hmdUnmountedEvent?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.M)) {
            hmdMountedEvent?.Invoke();
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.I)) {
            _hasInputFocus = false;
            inputFocusWasCapturedEvent?.Invoke();
        }
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.O)) {
            _hasInputFocus = true;
            inputFocusWasReleasedEvent?.Invoke();
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.V)) {
            _hasVrFocus = false;
            vrFocusWasCapturedEvent?.Invoke();
        }
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.B)) {
            _hasVrFocus = true;
            vrFocusWasReleasedEvent?.Invoke();
        }
    }

    public void TriggerHapticPulse(XRNode node, float duration, float strength, float frequency) {  }

    public void StopHaptics(XRNode node) { }

    public bool TryGetPoseOffsetForNode(XRNode node, out Pose poseOffset) {

        poseOffset = Pose.identity;
        return true;
    }

    public bool GetNodePose(XRNode nodeType, int idx, out Vector3 pos, out Quaternion rot) {

        pos = Vector3.zero;
        rot = Quaternion.identity;
#if UNITY_EDITOR
        // Very, Very, Very ugly, but we don't care it's editor only and we don't even care if it breaks
        if (_controllersRecorder == null) {
            var controllersRecorderType = Type.GetType("VRControllersRecorder, Main");
            if (controllersRecorderType != null) {
                _controllersRecorderGetPosesPropertyInfo = (PropertyInfo)controllersRecorderType.GetMember("currentPoses").Single();
                _controllersRecorder = FindObjectOfType(controllersRecorderType) as MonoBehaviour;
            }
            else {
                Debug.LogWarning("Unable to find FirstPersonFlyingController Type which we were searching for via reflection, will be returning 0s all the time");
            }
        }

        if (!_triedToFindHead) {
            // Very, Very, Very ugly, but we don't care it's editor only and we don't even care if it breaks
            Type firstPersonFlyingControllerType = Type.GetType("FirstPersonFlyingController, Main");
            if (firstPersonFlyingControllerType != null) {
                _firstPersonFlyingController = FindObjectOfType(firstPersonFlyingControllerType) as MonoBehaviour;
            }
            else {
                Debug.LogWarning("Unable to find FirstPersonFlyingController Type which we were searching for via reflection, will be returning 0s all the time");
            }
            _triedToFindHead = true;
        }
        if (_controllersRecorder != null) {
            var currentPoses = (ValueTuple<Pose, Pose, Pose>)_controllersRecorderGetPosesPropertyInfo.GetValue(_controllersRecorder);
            switch (nodeType) {
                case XRNode.Head:
                    pos = currentPoses.Item1.position;
                    rot = currentPoses.Item1.rotation;
                    break;
                case XRNode.LeftHand:
                    pos = currentPoses.Item2.position;
                    rot = currentPoses.Item2.rotation;
                    break;
                case XRNode.RightHand:
                    pos = currentPoses.Item3.position;
                    rot = currentPoses.Item3.rotation;
                    break;
            }
        }
        else if (_firstPersonFlyingController != null) {
            var t = _firstPersonFlyingController.transform;
            pos = t.position;
            rot = t.rotation;
        }
#endif

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

    public Vector2 GetAnyJoystickMaxAxis() {

        return new Vector2(Input.GetAxis("Mouse ScrollWheel"), Input.GetAxis("Mouse ScrollWheel"));
    }

    public float GetTriggerValue(XRNode node) => Input.GetMouseButton(0) ? 1.0f : 0.0f;

    public Vector2 GetThumbstickValue(XRNode node) {

        return new Vector2(Input.GetAxis("Mouse ScrollWheel"), Input.GetAxis("Mouse ScrollWheel"));
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
