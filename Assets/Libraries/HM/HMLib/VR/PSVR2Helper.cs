using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem.Utilities;
#if UNITY_PS5
using UnityEngine.XR.PSVR2;
using UnityEngine.XR.PSVR2.Input;
#endif

//TODO: Move this class to PSVR2 specific package
public class PSVR2Helper : MonoBehaviour, IVRPlatformHelper {

    [SerializeField] UnityXRController.Configuration _leftController;
    [SerializeField] UnityXRController.Configuration _rightController;
    [SerializeField] InputActionReference _pauseGameActionReference;
    [SerializeField] InputActionReference _leftTriggerActionReference;
    [SerializeField] InputActionReference _rightTriggerActionReference;
    [SerializeField] Pose _defaultPose;

    private readonly struct HeadsetHapticFrequencyLimit {

        public readonly float maxTimePlayed;
        // Maximum frequency after max time played has been breached
        public readonly int maximumFrequency;

        public HeadsetHapticFrequencyLimit(float maxTimePlayed, int maximumFrequency) {

            this.maxTimePlayed = maxTimePlayed;
            this.maximumFrequency = maximumFrequency;
        }
    }

    private Dictionary<XRNode, UnityXRController> _controllers = null;
    private Dictionary<XRNode, XRControllerWithRumble> _controllersWithRumble = null;

    private const string kRightControllerName = "PSVR2ControllerRight";
    private const string kLeftControllerName = "PSVR2ControllerLeft";

    private const int kMaxHMDFrequency = 25;

    private InputAction _pauseGameAction;
    private InputAction _leftTriggerAction;
    private InputAction _rightTriggerAction;
    private float _timeWhenStartedPlayingHaptic;
    private float _lastTimeWhenTriggeredHaptic;
    private bool _hasInputFocus = true;
    private bool _menuButtonDown = false;
    private bool _menuButtonDownThisFrame = false;

    // Do not change values unless TRCs update
    private readonly List<HeadsetHapticFrequencyLimit> _maximumHapticFrequencyLimits = new List<HeadsetHapticFrequencyLimit>() {
        new HeadsetHapticFrequencyLimit(9.0f, 13),
        new HeadsetHapticFrequencyLimit(1.4f, 18),
    };

#if UNITY_PS5
    private PSVR2HMD assignedHMD {
        get {
            if (_assignedHMD == null) {
                _assignedHMD = InputSystem.devices.FirstOrDefault(x => x is PSVR2HMD) as PSVR2HMD;
            }

            return _assignedHMD;
        }
    }
    private PSVR2HMD _assignedHMD;
#endif

    public event Action controllersDidChangeReferenceEvent;
    public event Action vrControllersDisconnectedOnStartupEvent;

    public bool hasInputFocus => _hasInputFocus;

    public bool hasVrFocus => _hasVrFocus;

    public bool isAlwaysWireless => false;

    public VRPlatformSDK vrPlatformSDK => VRPlatformSDK.Unknown;

    private bool _hasVrFocus = true;
#if UNITY_PS5
    private bool _hasUserPresence = true;
#endif

#pragma warning disable 67
    public event Action inputFocusWasCapturedEvent;
    public event Action inputFocusWasReleasedEvent;
    public event Action vrFocusWasCapturedEvent;
    public event Action vrFocusWasReleasedEvent;
    public event Action hmdUnmountedEvent;
    public event Action hmdMountedEvent;
    public event Action controllersDidDisconnectEvent;
#pragma warning restore

    private const float kRumbleMinimalDuration = 0.05f;

    public bool HasAnyVRControllerConnected()
        => InputSystem.devices.Any(device => device.name == kRightControllerName || device.name == kLeftControllerName);

    private void Awake() {

        _controllers = new Dictionary<XRNode, UnityXRController>() {
            { XRNode.LeftHand, _leftController.CreateController(XRNode.LeftHand) },
            { XRNode.RightHand, _rightController.CreateController(XRNode.RightHand)},
        };

        _controllersWithRumble = new Dictionary<XRNode, XRControllerWithRumble>() {
            { XRNode.LeftHand, XRController.leftHand as XRControllerWithRumble },
            { XRNode.RightHand, XRController.rightHand as XRControllerWithRumble },
        };

        InputSystem.onDeviceChange += InputDeviceChangeTriggered;

        _leftTriggerAction = _leftTriggerActionReference.action;
        _rightTriggerAction = _rightTriggerActionReference.action;
        _pauseGameAction = _pauseGameActionReference.action;
        _pauseGameAction.performed += OnPauseGamePerformed;
        _pauseGameAction.canceled += OnPauseGameCancelled;
        _pauseGameAction.Enable();

#if UNITY_PS5
        PSVR2Haptics.basicVibrationMode = PSVR2Haptics.Mode.Compatible;
        StartCoroutine(CheckControllerConnectionOnStartup(delayInSeconds: 5));
#endif
    }

#if UNITY_PS5
    private void Update() {

#if UNITY_EDITOR
        if (Application.isPlaying) {
            return;
        }
#endif

        bool hadUserPresence = _hasUserPresence;
        _hasUserPresence = assignedHMD.userPresence.ReadValue() > 0.5f;
        if (hadUserPresence && !_hasUserPresence) {
            hmdUnmountedEvent?.Invoke();
        }
        else if (!hadUserPresence && _hasUserPresence) {
            hmdMountedEvent?.Invoke();
        }
    }
#endif

    public bool TryGetPoseOffsetForNode(XRNode node, out Pose poseOffset) {

        poseOffset = _defaultPose;
        return true;
    }

    public bool GetNodePose(XRNode nodeType, int idx, out Vector3 pos, out Quaternion rot) {

#if UNITY_PS5
        if (nodeType == XRNode.Head) {
            pos = assignedHMD.centerEyePosition.ReadValue();
            rot = assignedHMD.centerEyeRotation.ReadValue();
            return true;
        }
#endif

        if (_controllers.TryGetValue(nodeType, out var controller)) {
            pos = controller.positionAction.ReadValue<Vector3>();
            rot = controller.rotationAction.ReadValue<Quaternion>();
            return true;
        }

        pos = Vector3.zero;
        rot = Quaternion.identity;
        return false;
    }

    public Pose GetRootPositionOffsetForLegacyNodePose(XRNode node) {

        return Pose.identity;
    }

    public bool TryGetLegacyPoseOffsetForNode(XRNode node, out Vector3 position, out Vector3 rotation) {

        position = Vector3.zero;
        rotation = Vector3.zero;
        return true;
    }

    public float GetTriggerValue(XRNode node) {
        return node switch {
            XRNode.LeftHand  => _leftTriggerAction.ReadValue<float>(),
            XRNode.RightHand => _rightTriggerAction.ReadValue<float>(),
            _                => 0
        };
    }
    
    public Vector2 GetAnyJoystickMaxAxis() => this.GetAnyJoystickMaxAxisDefaultImplementation();

    public Vector2 GetThumbstickValue(XRNode nodeType) {

        return _controllers.TryGetValue(nodeType, out var controller) ? controller.thumbstickAction.ReadValue<Vector2>() : Vector2.zero;
    }

    public bool IsAdvancedHapticsSupported(XRNode node) {

        return true;
    }

    public bool GetMenuButton() => _menuButtonDown;

    public bool GetMenuButtonDown() => _menuButtonDownThisFrame;
    public void RefreshControllersReference() {

        controllersDidChangeReferenceEvent?.Invoke();
    }

    public void StopHaptics(XRNode node) {

#if UNITY_PS5
        // Only supported for HMD vibration
        if (assignedHMD != null && node == XRNode.Head) {
            assignedHMD.StopHaptics();
        }
#endif
    }

    public void TriggerHapticPulse(XRNode node, float duration, float strength, float frequency) {

        if (_controllersWithRumble.TryGetValue(node, out XRControllerWithRumble hand)) {

            if (hand != null) {

                duration = Mathf.Max(duration, kRumbleMinimalDuration);
                hand.SendImpulse(strength, duration);
            }
        }
#if UNITY_PS5
        else if (node == XRNode.Head && assignedHMD != null) {
            // If time last time started playing haptics was not approximately last frame, then we can consider this a new haptic instance
            if (!Mathf.Approximately(_lastTimeWhenTriggeredHaptic, Time.time - Time.deltaTime)) {
                _timeWhenStartedPlayingHaptic = Time.time;
            }
            _lastTimeWhenTriggeredHaptic = Time.time;
            assignedHMD.SendImpulse(GetTRCCompliantHeadsetHapticFrequency(frequency));
        }
#endif
    }

    int GetTRCCompliantHeadsetHapticFrequency(float initialFrequency) {

        int resultFrequency = Mathf.FloorToInt(initialFrequency * kMaxHMDFrequency);

        // If the frequency is below the lowest we do not need to check anything
        if (resultFrequency > _maximumHapticFrequencyLimits[0].maximumFrequency) {
            float timeSpentContinuouslyPlayingHaptics = Time.time - _timeWhenStartedPlayingHaptic;
            foreach (HeadsetHapticFrequencyLimit maximumHapticFrequencyLimit in _maximumHapticFrequencyLimits) {
                if (timeSpentContinuouslyPlayingHaptics > maximumHapticFrequencyLimit.maxTimePlayed) {
                    resultFrequency = maximumHapticFrequencyLimit.maximumFrequency;
                    break;
                }
            }
        }

        return resultFrequency;
    }

    public void HandleApplicationVRFocusLost() {

        _hasVrFocus = false;
        vrFocusWasCapturedEvent?.Invoke();
    }

    public void HandleApplicationVRFocusResumed() {

        _hasVrFocus = true;
        vrFocusWasReleasedEvent?.Invoke();
    }

    public void HandleApplicationInputFocusLost() {

        _hasInputFocus = false;
        inputFocusWasCapturedEvent?.Invoke();
    }

    public void HandleApplicationInputFocusResumed() {

        _hasInputFocus = true;
        inputFocusWasReleasedEvent?.Invoke();
    }

#if UNITY_PS5
    public void HandleSystemServiceEvent(UnityEngine.PS5.Utility.SystemServiceEventType type, IntPtr paramData) {

        if (type == UnityEngine.PS5.Utility.SystemServiceEventType.ResetVrPosition) {

            XRInputSubsystem inputSubsystem = XRSubsystemHelper.GetCurrentInputSubsystem();
            if (inputSubsystem != null) {
                inputSubsystem.TryRecenter();
            }
        }
    }
#endif

    private void InputDeviceChangeTriggered(UnityEngine.InputSystem.InputDevice inputDevice, InputDeviceChange inputDeviceChange) {

        // TODO - Find a proper fix for the reconnect bug that doesn't feel hacky.
        // For some reason the ActionMap devices do not update automatically on PS5.
        // This fix manually adds new controllers into the devices array of ActionMap
        // which solves the issue.
        switch (inputDeviceChange) {
            case InputDeviceChange.Added:
                switch (inputDevice.name) {
                    case kRightControllerName:
                        AddControllerToMap(XRNode.RightHand, inputDevice);
                        _controllersWithRumble[XRNode.RightHand] = XRController.rightHand as XRControllerWithRumble;
                        break;

                    case kLeftControllerName:
                        AddControllerToMap(XRNode.LeftHand, inputDevice);
                        _controllersWithRumble[XRNode.LeftHand] = XRController.leftHand as XRControllerWithRumble;
                        break;
                }
                break;

            case InputDeviceChange.Removed:
                InputSystem.FlushDisconnectedDevices();
                break;

            case InputDeviceChange.Disconnected:
                foreach (var (xrNode, xrControllerWithRumble) in _controllersWithRumble) {
                    if (xrControllerWithRumble != null && xrControllerWithRumble.deviceId == inputDevice.deviceId) {
                        controllersDidDisconnectEvent?.Invoke();
                        break;
                    }
                }
                break;
        }
    }

    private IEnumerator CheckControllerConnectionOnStartup(float delayInSeconds) {

        yield return new WaitForSeconds(delayInSeconds);

        if (!HasAnyVRControllerConnected()) {
            vrControllersDisconnectedOnStartupEvent?.Invoke();
        }
    }

    private void AddControllerToMap(XRNode forNode, UnityEngine.InputSystem.InputDevice device) {

        if (!_controllers.TryGetValue(forNode, out UnityXRController controller)) {
            Debug.LogError("Controller array is not initialized properly in PSVR2Helper. Should not happen, could be a race condition.");
            return;
        }

        InputActionMap actionMap = controller.positionAction.actionMap;
        ReadOnlyArray<UnityEngine.InputSystem.InputDevice> currentDevices = actionMap.devices.GetValueOrDefault();
        List<UnityEngine.InputSystem.InputDevice> newDevicesList = new List<UnityEngine.InputSystem.InputDevice>(currentDevices);
        newDevicesList.Add(device);
        actionMap.devices = newDevicesList.ToArray();
    }

    protected void OnDestroy() {

        InputSystem.onDeviceChange -= InputDeviceChangeTriggered;
    }

    private void OnPauseGamePerformed(InputAction.CallbackContext context) {

        _menuButtonDownThisFrame = true;
        _menuButtonDown = true;
    }

    private void OnPauseGameCancelled(InputAction.CallbackContext context) {

        _menuButtonDown = false;
    }

    public void LateUpdate() {

        _menuButtonDownThisFrame = false;
    }
}
