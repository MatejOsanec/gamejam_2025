using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR;

//TODO: MOVE_TO_OCULUS_SPECIFIC_ASSEMBLY
// This will require a big refactor because there is a reference to the MonoBehaviour in the MainSystemInit
public class OculusVRHelper : MonoBehaviour, IVRPlatformHelper, IVerboseLogger {

    /// <summary>
    /// Default offset applied to the Oculus controller on non-alternative mode
    /// It was chosen by trial and error
    /// </summary>
    [DoesNotRequireDomainReloadInit]
    private static readonly Pose kOculusTouchControllerOffsetDefaultPose = new Pose(
        new Vector3(x: 0.005f, y: 0.03f, z: 0.055f),
        new Quaternion(x: -0.34202012f, y: 0, z: 0, w: 0.9396926f)
    );

    /// <summary>
    /// Default position offset applied in Oculus Touch controllers before OpenXR
    /// Used for the Alternative Handling
    /// </summary>
    [DoesNotRequireDomainReloadInit]
    private static readonly Vector3 kLegacyTouchPositionOffset = new Vector3(0, 0, 0.055f);

    /// <summary>
    /// Default rotation offset applied in Oculus Touch controllers before OpenXR
    /// Used for the Alternative Handling
    /// </summary>
    [DoesNotRequireDomainReloadInit]
    private static readonly Vector3 kLegacyTouchRotationOffset = new Vector3(-40, 0, 0);



#pragma warning disable 0067
    public event Action inputFocusWasCapturedEvent;
    public event Action inputFocusWasReleasedEvent;
    public event Action vrFocusWasCapturedEvent;
    public event Action vrFocusWasReleasedEvent;
    public event Action hmdUnmountedEvent;
    public event Action hmdMountedEvent;
    public event Action controllersDidChangeReferenceEvent;
    public event Action controllersDidDisconnectEvent;
#pragma warning restore 0067

#if BS_OCULUS_VR
    public bool hasInputFocus => OVRPlugin.hasInputFocus;
#else
    public bool hasInputFocus => false;
#endif

#if BS_OCULUS_VR
    public bool hasVrFocus => OVRPlugin.hasVrFocus;
#else
    public bool hasVrFocus => false;
#endif

    public bool isAlwaysWireless {
        get {
#if BS_OCULUS_PLATFORM && UNITY_ANDROID
            return true;
#else
            return false;
#endif
        }
    }
    public VRPlatformSDK vrPlatformSDK => VRPlatformSDK.Oculus;
    public string loggerPrefix => "OculusVRHelper";

    private bool _hasInputFocus;
    private bool _hasVrFocus;
    private bool _userPresent;
    private int _lastButtonMenuButtonDownFrame;
    private bool _leftControllerConnected;
    private bool _rightControllerConnected;

#if BS_OCULUS_VR
        private bool _isOVRManagerPresent => OVRManager.instance != null;
#endif

    private EventSystem _disabledEventSystem;

    private const string kVerticalLeftHand = "VerticalLeftHand";
    private const string kVerticalRightHand = "VerticalRightHand";
    private const string kHorizontalLeftHand = "HorizontalLeftHand";
    private const string kHorizontalRightHand = "HorizontalRightHand";

    protected void Update() {

#if BS_OCULUS_VR
        if (OVRPlugin.shouldQuit) {
            Application.Quit();
        }

        if (OVRPlugin.shouldRecenter) {
            OVRPlugin.RecenterTrackingOrigin(OVRPlugin.RecenterFlags.Default);
            controllersDidChangeReferenceEvent?.Invoke();
        }

        // user present
        bool currentlyUserPresent = OVRPlugin.userPresent;
#if !UNITY_EDITOR || !BS_IGNORE_VR_FOCUS_LOST_EVENTS
        if (_userPresent && !currentlyUserPresent) {
            this.Log("hmd unmounted");
            hmdUnmountedEvent?.Invoke();
        }
        else if (!_userPresent && currentlyUserPresent) {
            this.Log("hmd mounted");
            hmdMountedEvent?.Invoke();
        }
#endif
        _userPresent = currentlyUserPresent;

        // input focus
        bool currentlyHasInputFocus = OVRPlugin.hasInputFocus;
#if !UNITY_EDITOR || !BS_IGNORE_VR_FOCUS_LOST_EVENTS
        if (_hasInputFocus && !currentlyHasInputFocus) {
            this.Log("input focus lost");
            inputFocusWasCapturedEvent?.Invoke();
            DisableEventSystem();
        }
        else if (!_hasInputFocus && currentlyHasInputFocus) {
            this.Log("input focus acquired");
            inputFocusWasReleasedEvent?.Invoke();
            EnableEventSystem();
        }
#endif
        _hasInputFocus = currentlyHasInputFocus;

        // vr focus
        bool currentlyHasVrFocus = OVRPlugin.hasVrFocus;
#if !UNITY_EDITOR || !BS_IGNORE_VR_FOCUS_LOST_EVENTS
        if (_hasVrFocus && !currentlyHasVrFocus) {
            this.Log("vr focus lost");
            vrFocusWasCapturedEvent?.Invoke();
        }
        else if (!_hasVrFocus && currentlyHasVrFocus) {
            this.Log("vr focus acquired");
            vrFocusWasReleasedEvent?.Invoke();
        }
#endif
        _hasVrFocus = currentlyHasVrFocus;

        bool newLeftControllerConnected = OVRInput.IsControllerConnected(OVRInput.Controller.LTouch);
        if (_leftControllerConnected && !newLeftControllerConnected) {
            controllersDidDisconnectEvent?.Invoke();
        }
        _leftControllerConnected = newLeftControllerConnected;

        bool newRightControllerConnected = OVRInput.IsControllerConnected(OVRInput.Controller.RTouch);
        if (_rightControllerConnected && !newRightControllerConnected) {
            controllersDidDisconnectEvent?.Invoke();
        }
        _rightControllerConnected = newRightControllerConnected;

        if (!_isOVRManagerPresent) {
            OVRInput.Update();
        }
#endif
    }

    protected void FixedUpdate() {

#if BS_OCULUS_VR
        if (!_isOVRManagerPresent) {
            OVRInput.FixedUpdate();
        }
#endif
    }

    protected void LateUpdate() {

#if BS_OCULUS_VR
        if (!_isOVRManagerPresent) {
            OVRHaptics.Process();
        }
#endif
    }

    public void TriggerHapticPulse(XRNode node, float duration, float strength, float frequency) {

#if BS_OCULUS_VR
        var controller = node == XRNode.LeftHand ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
        OVRPlugin.SetControllerVibration((uint)controller, frequency, strength);
#endif
    }

    public void StopHaptics(XRNode node) {

#if BS_OCULUS_VR
        var controller = node == XRNode.LeftHand ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
        OVRPlugin.SetControllerVibration((uint)controller, 0, 0);
#endif
    }

    public bool TryGetPoseOffsetForNode(XRNode node, out Pose poseOffset) {

        if (node is XRNode.LeftHand or XRNode.RightHand) {
            poseOffset = kOculusTouchControllerOffsetDefaultPose;
            return true;
        }
        poseOffset = Pose.identity;
        return false;
    }

    public bool GetNodePose(XRNode nodeType, int idx, out Vector3 pos, out Quaternion rot) {

        pos = Vector3.zero;
        rot = Quaternion.identity;
#if BS_OCULUS_VR
        var node = XRNodeToOVRNode(nodeType);
        if (!OVRPlugin.GetNodePositionValid(node) ||
            !OVRPlugin.GetNodeOrientationValid(node)) {
            return false;
        }
        var pose = OVRPlugin.GetNodePose(node, OVRPlugin.Step.Render).ToOVRPose();
        pos = pose.position;
        rot = pose.orientation;
        return true;
#else
        return false;
#endif

    }

    public Pose GetRootPositionOffsetForLegacyNodePose(XRNode node) {

        return Pose.identity;
    }

    public bool TryGetLegacyPoseOffsetForNode(XRNode node, out Vector3 position, out Vector3 rotation) {

        position = kLegacyTouchPositionOffset;
        rotation = kLegacyTouchRotationOffset;
        return true;
    }

#if BS_OCULUS_VR
    private static OVRPlugin.Node XRNodeToOVRNode(XRNode node) {

        switch (node) {
            case XRNode.Head: return OVRPlugin.Node.Head;
            case XRNode.LeftHand: return OVRPlugin.Node.HandLeft;
            case XRNode.RightHand: return OVRPlugin.Node.HandRight;
        }

        Debug.LogError($"Can not convert XRNode ({node}) to OVRPlugin.Node.");
        return OVRPlugin.Node.None;
    }
#endif
    public Vector2 GetAnyJoystickMaxAxis() => this.GetAnyJoystickMaxAxisDefaultImplementation();

    public float GetTriggerValue(XRNode node) {

#if BS_OCULUS_VR
        switch (node) {
            case XRNode.LeftHand:
                return OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
            case XRNode.RightHand:
                return OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
        }
#endif
        return VRPlatformUtils.TriggerValueDefaultImplementation(node);
    }

    public Vector2 GetThumbstickValue(XRNode node) {

        return node switch {
            XRNode.LeftHand => new Vector2(Input.GetAxis(kHorizontalLeftHand), -Input.GetAxis(kVerticalLeftHand)),
            XRNode.RightHand => new Vector2(Input.GetAxis(kHorizontalRightHand), -Input.GetAxis(kVerticalRightHand)),
            _ => Vector2.zero
        };
    }

#if BS_OCULUS_VR
    public OVRInput.InteractionProfile GetInteractionProfile(XRNode node) {

        // Only left and right controllers are currently supported
        if (node != XRNode.LeftHand && node != XRNode.RightHand) {
            return OVRInput.InteractionProfile.None;
        }

        return OVRInput.GetCurrentInteractionProfile(node == XRNode.LeftHand ? OVRInput.Hand.HandLeft : OVRInput.Hand.HandRight);
    }
#endif

    /// Not cached, subsequent calls of this API will result in repetitive calls of system API which might be expensive
    public bool IsAdvancedHapticsSupported(XRNode node) {

        // Only left and right controllers are currently supported
        return node == XRNode.LeftHand || node == XRNode.RightHand;
    }

    public bool GetMenuButton() {

#if BS_OCULUS_VR
        return OVRInput.Get(OVRInput.RawButton.Start);
#else
        return Input.GetButton(VRPlatformUtils.kMenuButtonOculusTouch);
#endif
    }

    public bool GetMenuButtonDown() {

#if BS_OCULUS_VR
        var isButtonDown = OVRInput.GetDown(OVRInput.RawButton.Start);
#else
        var isButtonDown = Input.GetButtonDown(VRPlatformUtils.kMenuButtonOculusTouch);
#endif

        if (!isButtonDown) {
            return false;
        }

        // This is here due to OVRInput returning true for 2 frames in a row
        if (Time.frameCount - 1 == _lastButtonMenuButtonDownFrame) {
            return false;
        }

        _lastButtonMenuButtonDownFrame = Time.frameCount;
        return true;
    }

    public void RefreshControllersReference() {

        controllersDidChangeReferenceEvent?.Invoke();
    }

    private void EnableEventSystem() {

        if (_disabledEventSystem != null) {
            _disabledEventSystem.enabled = true;
            _disabledEventSystem = null;
        }
    }

    private void DisableEventSystem() {

        if (_disabledEventSystem == null) {
            _disabledEventSystem = EventSystem.current;
            if (_disabledEventSystem != null) {
                // if someone else already disabled event system then leave it as is
                if (!_disabledEventSystem.enabled) {
                    _disabledEventSystem = null;
                }
                else {
                    _disabledEventSystem.enabled = false;
                }
            }
        }
    }
}
