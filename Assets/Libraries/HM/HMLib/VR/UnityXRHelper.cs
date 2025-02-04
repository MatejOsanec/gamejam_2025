using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using InputDevice = UnityEngine.XR.InputDevice;

#if BS_OPENXR_VR
using UnityEngine.XR.OpenXR;
#endif

public class UnityXRHelper : MonoBehaviour, IVRPlatformHelper, IVerboseLogger {

    [SerializeField] UnityXRController.Configuration _leftControllerConfiguration;
    [SerializeField] UnityXRController.Configuration _rightControllerConfiguration;
    [SerializeField] InputActionReference _userPresenceActionReference;
    [SerializeField] InputActionReference _headPositionActionReference;
    [SerializeField] InputActionReference _headOrientationActionReference;
    [SerializeField] InputActionReference _pauseGameActionReference;

    public enum VRControllerManufacturerName {
        Undefined,
        HTC,
        Oculus,
        Valve,
        Microsoft,
        Unknown
    }

    public event Action inputFocusWasCapturedEvent;
    public event Action inputFocusWasReleasedEvent;
    public event Action vrFocusWasCapturedEvent;
    public event Action vrFocusWasReleasedEvent;
    public event Action hmdUnmountedEvent;
    public event Action hmdMountedEvent;
    public event Action controllersDidChangeReferenceEvent;
#pragma warning disable 00067
    public event Action controllersDidDisconnectEvent;
#pragma warning restore 00067

    public bool hasInputFocus => _userPresence;
    public bool hasVrFocus { get; private set; }
    public bool isAlwaysWireless => false;
    public VRPlatformSDK vrPlatformSDK => VRPlatformSDK.OpenXR;

    public string loggerPrefix => "XRInput";

    internal UnityXRController leftController => _leftController;
    internal UnityXRController rightController => _rightController;

    /// <summary>
    /// Default offset applied to the Oculus controller on non-alternative mode.
    /// This value was defined by trial and error after Unity Upgrade from 2019 to 2021
    /// </summary>
    [DoesNotRequireDomainReloadInit]
    private static readonly Pose kOculusOffsetDefaultPose = new Pose(
        new Vector3(x: 0.01f, y: -0.045f, z: 0.1f),
        new Quaternion(x: 0.17364818f, y: 0, z: 0, w: 0.9848077f)
    );

    /// <summary>
    /// Offset used to convert the OpenXR reference position in the controller to the legacy version.
    /// It was captured the same position of the controller in the OpenXR and the OculusVR reference position
    /// Then calculated the inverse transform of the new to the old.
    /// Result was average between the right hand and flipped left hand
    /// Finally it had the precision reduced to 6 decimals places in position and 2 in euler rotation axis
    /// Euler angles: x: 60, y: 0, z:0
    /// </summary>
    [DoesNotRequireDomainReloadInit]
    private static readonly Pose kOculusTouchOriginOffsetToLegacy = new Pose(
        new Vector3(0, -0.01964f, 0.04598f),
        new Quaternion(x: 0.5f, y: 0, z: 0, w: 0.866025388f));

    /// <summary>
    /// Default position offset applied in Oculus Touch controllers before OpenXR
    /// </summary>
    [DoesNotRequireDomainReloadInit]
    private static readonly Vector3 kOculusTouchLegacyPositionOffset = new Vector3(0, 0, 0.055f);

    /// <summary>
    /// Default rotation offset applied in Oculus Touch controllers before OpenXR
    /// </summary>
    [DoesNotRequireDomainReloadInit]
    private static readonly Vector3 kOculusTouchLegacyRotationOffset = new Vector3(-40, 0, 0);

    /// <summary>
    /// Default offset applied to the Valve Index controller on non-alternative mode.
    /// This value was defined by trial and error after Unity Upgrade from 2019 to 2021
    /// </summary>
    [DoesNotRequireDomainReloadInit]
    private static readonly Pose kValveIndexOffsetDefaultPose = new Pose(
        new Vector3(x: 0, y: 0, z: 0.12f),
        new Quaternion(x: -0.0012371338f, y: 0, z: 0, w: 0.9999992f)
    );

    /// <summary>
    /// Offset used to convert the OpenXR reference position in the controller to the legacy version.
    /// It was captured the same position of the controller in the OpenXR and the OpenVR legacy reference position
    /// Then calculated the inverse transform of the new to the old.
    /// Result was average between the right hand and flipped left hand
    /// Finally it had the precision reduced to 6 decimals places in position and 2 in euler rotation axis
    /// Euler angles: x: 15.4f, y: 2.14f, z:0.86f
    /// </summary>
    [DoesNotRequireDomainReloadInit]
    private static readonly Pose kValveIndexOriginOffsetToLegacy = new Pose(
        new Vector3(x: 0.004575f, y: -0.01995f, z: 0.129387f),
        new Quaternion(x: 0.134097934f, y: 0.0174996667f, z:0.00493389927f, w:0.990801275f));

    /// <summary>
    /// Default position offset applied in Valve Index controllers before OpenXR
    /// </summary>
    [DoesNotRequireDomainReloadInit]
    private static readonly Vector3 kValveIndexLegacyPositionOffset = new Vector3(0, 0.022f, -0.01f);

    /// <summary>
    /// Default rotation offset applied in Valve Index controllers before OpenXR
    /// </summary>
    [DoesNotRequireDomainReloadInit]
    private static readonly Vector3 kValveIndexLegacyRotationOffset = new Vector3(-16.3f, 0, 0);

    /// <summary>
    /// Default offset applied to the HTC Vive controller on non-alternative mode.
    /// This value was defined by trial and error after Unity Upgrade from 2019 to 2021
    /// </summary>
    [DoesNotRequireDomainReloadInit]
    private static readonly Pose kHTCViveOffsetDefaultPose = new Pose(
        new Vector3(x: 0, y: 0.008f, z: 0.095f),
        new Quaternion(x: -0.00032738686f, y: 0, z: 0, w: 0.99999994f)
    );

    /// <summary>
    /// Offset used to convert the OpenXR reference position in the controller to the legacy version.
    /// It was captured the same position of the controller in the OpenXR and the OpenVR legacy reference position
    /// Then calculated the inverse transform of the new to the old.
    /// Result was average between the right hand and flipped left hand
    /// Finally it had the precision reduced to 6 decimals places in position and 2 in euler rotation axis
    /// Euler angles: x: 5.0f, y: 0f, z:0f
    /// </summary>
    [DoesNotRequireDomainReloadInit]
    private static readonly Pose kHTCViveOriginOffsetToLegacy = new Pose(
        new Vector3(x: 0.000484f, y: 0.00692f, z: 0.09826f),
        new Quaternion(x: 0.0436193869f, y: 0, z:0, w: 0.999048233f));

    /// <summary>
    /// Default position offset applied in HTC Vive controllers before OpenXR
    /// </summary>
    [DoesNotRequireDomainReloadInit]
    private static readonly Vector3 kHTCViveLegacyPositionOffset = new Vector3(0, -0.008f, 0f);

    /// <summary>
    /// Default rotation offset applied in HTC Vive controllers before OpenXR
    /// </summary>
    [DoesNotRequireDomainReloadInit]
    private static readonly Vector3 kHTCViveLegacyRotationOffset = new Vector3(-4.3f, 0, 0);

    private UnityXRController _leftController;
    private UnityXRController _rightController;
#if BS_OPENXR_VR
    private BeatSaberXRFeature _beatSaberXRFeature;
#endif
    private InputAction _headPositionAction;
    private InputAction _headOrientationAction;
    private InputAction _userPresenceAction;
    private InputAction _pauseGameAction;
    private bool _scrollingLastFrame;
    private bool _userPresence;
    private bool _isPausePressed;
    private bool _wasPausePressedThisFrame;
    private bool userPresence {
        set {
            if (value == _userPresence) {
                return;
            }
            _userPresence = value;
            if (_userPresence) {
                inputFocusWasReleasedEvent?.Invoke();
                hmdMountedEvent?.Invoke();
            }
            else {
                inputFocusWasCapturedEvent?.Invoke();
                hmdUnmountedEvent?.Invoke();
            }
        }
    }

    protected void Start() {

        hasVrFocus = true;

        _leftController = _leftControllerConfiguration.CreateController(XRNode.LeftHand);
        _rightController = _rightControllerConfiguration.CreateController(XRNode.RightHand);

        _headPositionAction = _headPositionActionReference.action;
        _headPositionAction.Enable();

        _headOrientationAction = _headOrientationActionReference.action;
        _headOrientationAction.Enable();

        // We have to keep this along with _beatSaberXRFeature.sessionStateChangedEvent as this is working
        // on Steam OpenXR runtime while _beatSaberXRFeature works on Oculus OpenXR runtime
        _userPresenceAction = _userPresenceActionReference.action;
        _userPresenceAction.started += OnUserPresenceStarted;
        _userPresenceAction.canceled += OnUserPresenceCanceled;
        _userPresenceAction.Enable();

        _pauseGameAction = _pauseGameActionReference.action;
        _pauseGameAction.performed += OnPauseGamePerformed;
        _pauseGameAction.canceled += OnPauseGameCancelled;
        _pauseGameAction.Enable();

        InputTracking.nodeAdded += HandleNewXRNode;
        InputTracking.nodeRemoved += HandleRemovedXRNode;
        var subsystems = new List<XRInputSubsystem>();
        SubsystemManager.GetInstances(subsystems);
        var firstSubsystem = subsystems[0];
        firstSubsystem.boundaryChanged += OnboundaryChanged;
        firstSubsystem.trackingOriginUpdated += OnTrackingOriginUpdated;
        UpdateManufacturerOnNode(XRNode.LeftHand);
        UpdateManufacturerOnNode(XRNode.RightHand);

#if BS_OPENXR_VR
        OpenXRSettings settings = OpenXRSettings.Instance;
        if (settings == null) {
            Debug.LogWarning($"Unable to find OpenXRSettings.Instance");
            return;
        }
        _beatSaberXRFeature = settings.GetFeature<BeatSaberXRFeature>();
        if (_beatSaberXRFeature == null) {
            Debug.LogWarning($"Unable to find BeatSaberXRFeature");
            return;
        }

        // Duplicating behaviour of _userPresenceAction because it's bugged in Unity's OpenXR
        // We have to keep this along with _userPresenceAction as this is working
        // on Oculus OpenXR runtime while _userPresenceAction works on Steam OpenXR runtime
        _beatSaberXRFeature.sessionStateChangedEvent += HandleBeatSaberXRFeatureSessionStateChanged;
        UpdateUserPresence(_beatSaberXRFeature.currentSessionState);
#endif
    }

    private void OnTrackingOriginUpdated(XRInputSubsystem inputSystem) {

        RefreshControllersReference();
    }

    private void OnboundaryChanged(XRInputSubsystem inputSystem) {

        RefreshControllersReference();
    }

    protected void OnDestroy() {

        if (_userPresenceAction != null) {
            _userPresenceAction.Disable();
            _userPresenceAction.started -= OnUserPresenceStarted;
            _userPresenceAction.canceled -= OnUserPresenceCanceled;
        }

        if (_pauseGameAction != null) {
            _pauseGameAction.Disable();
            _pauseGameAction.performed -= OnPauseGamePerformed;
            _pauseGameAction.canceled -= OnPauseGameCancelled;
        }

#if BS_OPENXR_VR
        if (_beatSaberXRFeature != null) {
            _beatSaberXRFeature.sessionStateChangedEvent -= HandleBeatSaberXRFeatureSessionStateChanged;
        }
#endif

        InputTracking.nodeAdded -= HandleNewXRNode;
        InputTracking.nodeRemoved -= HandleRemovedXRNode;
    }

    private void OnUserPresenceCanceled(InputAction.CallbackContext context) {

        userPresence = false;
    }

    private void OnUserPresenceStarted(InputAction.CallbackContext context) {

        userPresence = true;
    }

#if BS_OPENXR_VR
    private void HandleBeatSaberXRFeatureSessionStateChanged(BeatSaberXRFeature.SessionState oldState, BeatSaberXRFeature.SessionState newState) {

        UpdateUserPresence(newState);
    }

    private void UpdateUserPresence(BeatSaberXRFeature.SessionState sessionState) {

        userPresence = sessionState is
            BeatSaberXRFeature.SessionState.Focused or
            BeatSaberXRFeature.SessionState.Visible or
            BeatSaberXRFeature.SessionState.Synchronized;
    }
#endif

    protected void OnApplicationPause(bool pauseStatus) {

        hasVrFocus = !pauseStatus;
        if (pauseStatus) {
            vrFocusWasCapturedEvent?.Invoke();
        } else {
            vrFocusWasReleasedEvent?.Invoke();
        }
    }

    private void OnPauseGamePerformed(InputAction.CallbackContext context) {

        _wasPausePressedThisFrame = true;
        _isPausePressed = true;
    }

    private void OnPauseGameCancelled(InputAction.CallbackContext context) {

        _isPausePressed = false;
    }

    private void HandleNewXRNode(XRNodeState state) {

        UpdateManufacturerOnNode(state.nodeType);
    }

    private void UpdateManufacturerOnNode(XRNode node) {

        bool updateController = false;
        var inputDevice = InputDevices.GetDeviceAtXRNode(node);
        switch (node) {
            case XRNode.LeftHand:
                updateController = _leftController.SetupController(inputDevice, this);
                this.Log($"Left controller setup was '{updateController}': {node} with input device manufacturer string: {inputDevice.manufacturer} mapped to {_leftController.manufacturerName}");
                break;
            case XRNode.RightHand:
                updateController = _rightController.SetupController(inputDevice, this);
                this.Log($"Right controller setup was '{updateController}': {node} with input device manufacturer string: {inputDevice.manufacturer} mapped to {_rightController.manufacturerName}");
                break;
            default:
                this.Log($"Did not setup node: {node}");
                break;
        }
        if (updateController) {
            RefreshControllersReference();
        }
    }

    private void HandleRemovedXRNode(XRNodeState state) {

        switch (state.nodeType) {
            case XRNode.LeftHand:
                _leftController.ResetManufacturerName();
                controllersDidDisconnectEvent?.Invoke();
                this.Log($"Removing tracking from left controller: {state.nodeType}");
                break;
            case XRNode.RightHand:
                _rightController.ResetManufacturerName();
                controllersDidDisconnectEvent?.Invoke();
                this.Log($"Removing tracking from right controller: {state.nodeType}");
                break;
            default:
                this.Log($"Removing tracking from: {state.nodeType}");
                break;
        }
    }

    private UnityXRController ControllerFromNode(XRNode node) {

        switch (node) {
            case XRNode.LeftHand:
                return _leftController;
            case XRNode.RightHand:
                return _rightController;
            default:
                return null;
        }
    }

    public void TriggerHapticPulse(XRNode node, float duration, float strength, float frequency) {

        var controller = ControllerFromNode(node);

        if (controller == null) {
            Debug.LogWarning($"Couldn't find controller for node {node.ToString()}");
            return;
        }

        controller.hapticsHandler.TriggerHapticPulse(strength, duration);
    }

    public void StopHaptics(XRNode node) {

        var controller = ControllerFromNode(node);

        if (controller == null) {
            Debug.LogWarning($"Couldn't find controller for node {node.ToString()}");
            return;
        }

        controller.hapticsHandler.StopHaptics();
    }
    public bool TryGetPoseOffsetForNode(XRNode node, out Pose poseOffset) {

        var controller = ControllerFromNode(node);
        if (controller == null || controller.manufacturerName is VRControllerManufacturerName.Unknown or VRControllerManufacturerName.Undefined) {
            poseOffset = Pose.identity;
            return false;
        }
        poseOffset = GetPoseOffsetForManufacturer(controller.manufacturerName);
        return true;
    }

    private Pose GetPoseOffsetForManufacturer(VRControllerManufacturerName manufacturerName) {

        switch (manufacturerName) {
            case VRControllerManufacturerName.Oculus:
                return kOculusOffsetDefaultPose;
            case VRControllerManufacturerName.Valve:
                return kValveIndexOffsetDefaultPose;
            case VRControllerManufacturerName.HTC:
                return kHTCViveOffsetDefaultPose;
            case VRControllerManufacturerName.Microsoft:
            case VRControllerManufacturerName.Undefined:
            case VRControllerManufacturerName.Unknown:
                break;
            default:
                //TODO: implement static analysis to check if all enums are handled in a switch statement
                Debug.LogWarning($"Unexpected manufacturer name: {manufacturerName}");
                break;
        }
        return Pose.identity;
    }

    public bool GetNodePose(XRNode nodeType, int idx, out Vector3 pos, out Quaternion rot) {

        if (nodeType == XRNode.Head) {
            ReadHeadPose(out pos, out rot);
            return true;
        }
        var controller = ControllerFromNode(nodeType);
        pos = Vector3.zero;
        rot = Quaternion.identity;
        if (controller is null) {
            Debug.LogError($"Invalid controller: {nodeType}");
            return false;
        }
        pos = controller.positionAction.ReadValue<Vector3>();
        rot = controller.rotationAction.ReadValue<Quaternion>();
        return true;
    }

    public Pose GetRootPositionOffsetForLegacyNodePose(XRNode node) {

        var controller = ControllerFromNode(node);
        return controller == null ? Pose.identity : GetRootPositionOffsetForLegacyNodePose(controller.manufacturerName);
    }

    private Pose GetRootPositionOffsetForLegacyNodePose(
        VRControllerManufacturerName manufacturerName
    ) {
        switch (manufacturerName) {
            case VRControllerManufacturerName.Oculus:
                return kOculusTouchOriginOffsetToLegacy;
            case VRControllerManufacturerName.Valve:
                return kValveIndexOriginOffsetToLegacy;
            case VRControllerManufacturerName.HTC:
                return kHTCViveOriginOffsetToLegacy;
            case VRControllerManufacturerName.Microsoft:
            case VRControllerManufacturerName.Undefined:
            case VRControllerManufacturerName.Unknown:
                break;
            default:
                //TODO: implement static analysis to check if all enums are handled in a switch statement
                Debug.LogWarning($"Unexpected manufacturer name: {manufacturerName}");
                break;
        }
        return Pose.identity;
    }

    public bool TryGetLegacyPoseOffsetForNode(XRNode node, out Vector3 position, out Vector3 rotation) {

        var controller = ControllerFromNode(node);
        if (controller != null) {
            return TryGetLegacyPoseOffsetForNode(controller.manufacturerName, out position, out rotation);
        }
        position = Vector3.zero;
        rotation = Vector3.zero;
        return false;
    }

    private static bool TryGetLegacyPoseOffsetForNode(
        VRControllerManufacturerName manufacturerName,
        out Vector3 position,
        out Vector3 rotation
    ) {
        position = Vector3.zero;
        rotation = Vector3.zero;
        switch (manufacturerName) {
            case VRControllerManufacturerName.Oculus:
                position = kOculusTouchLegacyPositionOffset;
                rotation = kOculusTouchLegacyRotationOffset;
                return true;
            case VRControllerManufacturerName.Valve:
                position = kValveIndexLegacyPositionOffset;
                rotation = kValveIndexLegacyRotationOffset;
                return true;
            case VRControllerManufacturerName.HTC:
                position = kHTCViveLegacyPositionOffset;
                rotation = kHTCViveLegacyRotationOffset;
                return true;
            case VRControllerManufacturerName.Microsoft:
            case VRControllerManufacturerName.Undefined:
            case VRControllerManufacturerName.Unknown:
                return true;
            default:
                //TODO: implement static analysis to check if all enums are handled in a switch statement
                Debug.LogWarning($"Unexpected manufacturer name: {manufacturerName}");
                return false;
        }
    }

    public Vector2 GetAnyJoystickMaxAxis() => this.GetAnyJoystickMaxAxisDefaultImplementation();

    //TODO: Get trigger from New Input System
    public float GetTriggerValue(XRNode node) =>
        VRPlatformUtils.TriggerValueDefaultImplementation(node);

    public Vector2 GetThumbstickValue(XRNode node) {

        return ControllerFromNode(node).thumbstickAction.ReadValue<Vector2>();
    }

    public bool IsAdvancedHapticsSupported(XRNode node) {

        // Oculus Advanced Haptics currently does not work on Rift
        // and we don't have any other non-Oculus, non-PS5 advanced haptics

        // Rift+Link+Quest should be supported in Oculus sdk v62
        // To differentiate controllers, use GetControllerSampleRateHz() from OVRPlugin.cs
        // and use that to determine controller - >=1950Hz Quest3, >=2000Hz Quest Pro (Quest 2 is 500Hz)
        return false;
    }

    public bool GetMenuButton() {

        return _isPausePressed;
    }

    public bool GetMenuButtonDown() {

        return _wasPausePressedThisFrame;
    }

    public void RefreshControllersReference() {

        controllersDidChangeReferenceEvent?.Invoke();
    }

    private void ReadHeadPose(out Vector3 pos, out Quaternion rot) {

        pos = _headPositionAction.ReadValue<Vector3>();
        rot = _headOrientationAction.ReadValue<Quaternion>();
    }

    //TODO: Remove during XRHelpers unification
    public void LateUpdate() {

        _wasPausePressedThisFrame = false;
    }
}
