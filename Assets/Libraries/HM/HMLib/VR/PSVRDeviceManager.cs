#if UNITY_PS4

using System;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using BGLib.DotnetExtension.CommandLine;
using UnityEngine.PS4.VR;
using UnityEngine.PS4;
using UnityEngine.Assertions;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using Zenject;

public class PSVRDeviceManager : ITickable {

    public event Action didRecenterEvent;

    public VRState vrState => _vrState;

    public enum PSMoveButton {
        Select = (1 << 0),
        Move = (1 << 2),
        Start = (1 << 3),
        Triangle = (1 << 4),
        Circle = (1 << 5),
        Cross = (1 << 6),
        Square = (1 << 7),
    }

    public enum VRState {
        NotStarted,
        Starting,
        ShuttingDown,
        Started
    }

    private class PSMoveDevice {
        public int handle = -1;
        public float vibationTimer = 0;
        public Vector3 position = Vector3.zero;
        public Quaternion rotation = Quaternion.identity;
    }

    private PSMoveDevice[] _psMoveDevices;
    private Action _restartPsvrSceneCallback;
    private Action _loadNextSceneCallback;
    private VRState _vrState = VRState.NotStarted;

    private CancellationTokenSource _registerMoveControllersCancellationTokenSource;
    private CancellationTokenSource _vrSetupCancellationTokenSource;

    private bool _wasDisconnectionCaptured = false;
    private bool _isDialogOpened = false;
    private bool _debugSkipHmdRequirement = false;

    public event Action moveDeviceDidDisconnectEvent;

    const float kVRSetupMaxDuration = 2.0f;

    [Inject] readonly CommandLineParserResult _commandLineParserResult;

    private class VRDeviceNames {

        public const string None = "None";
        public const string PSVR = "PSVR Display";
    }

    public PSVRDeviceManager() {

        _psMoveDevices = new PSMoveDevice[2];
        _psMoveDevices[0] = new PSMoveDevice();
        _psMoveDevices[1] = new PSMoveDevice();
        _psMoveDevices[0].handle = -1;
        _psMoveDevices[1].handle = -1;
    }

    public void Init(Action restartPsvrSceneCallback, Action loadNextSceneCallback) {

        _restartPsvrSceneCallback = restartPsvrSceneCallback;
        _loadNextSceneCallback = loadNextSceneCallback;
        _debugSkipHmdRequirement = _commandLineParserResult.Contains("fpfc") || _commandLineParserResult.Contains("-fpfc");
    }

    public async void StartVRInit() {

        if (_vrState is VRState.Started or VRState.Starting) {
            return;
        }

        _vrState = VRState.Starting;

        Assert.IsNotNull(_restartPsvrSceneCallback);
        Assert.IsNotNull(_loadNextSceneCallback);

        try {
            if (XRSettings.enabled == false) {
                await SetupHmdDeviceAsync();
            }
            else {
                await VRSetupAsync();
            }
        }
        catch (Exception e) {
            _vrState = VRState.NotStarted;
            Debug.LogError($"exception during starting VR Init {e.Message}");
        }
    }

    public void Tick() {

#if UNITY_EDITOR
        if (Application.isPlaying) {
            return;
        }
#endif
        XRDisplaySubsystem xrSubsystem = XRSubsystemHelper.GetCurrentDisplaySubsystem();

        if (xrSubsystem != null && xrSubsystem.running && XRSettings.loadedDeviceName == VRDeviceNames.PSVR) {
            UpdateMoveTransforms();
            UpdateMoveVibrations();
        }

        UpdateMoveConnectionState();
    }

    private void UpdateMoveConnectionState() {

        if (!PS4Input.MoveIsConnected(0, 0) || !PS4Input.MoveIsConnected(0, 1)) {
            if (!_wasDisconnectionCaptured) {
                _wasDisconnectionCaptured = true;
                moveDeviceDidDisconnectEvent?.Invoke();
            }
        }
        else if (_wasDisconnectionCaptured) {
            _wasDisconnectionCaptured = false;
        }
    }

    private async Task VRSetupAsync() {

        CancelVRSetupTask();
        _vrSetupCancellationTokenSource = new();
        await VRSetupAsync(_vrSetupCancellationTokenSource.Token);
    }

    private async Task VRSetupAsync(CancellationToken cancellationToken) {

        // Register the callbacks needed to detect resetting the HMD
        Utility.onSystemServiceEvent += OnSystemServiceEvent;
        PlayStationVR.onDeviceEvent += OnDeviceEvent;

        // If Unity starts with the headset not started it seems Unity cannot initialize the XR itself and we have to do it manually
        XRManagerSettings xrManager = XRGeneralSettings.Instance.Manager;
        if (!xrManager.isInitializationComplete) {
            xrManager.InitializeLoaderSync();
            xrManager.StartSubsystems();
        }

        if (String.Compare(XRSettings.loadedDeviceName, VRDeviceNames.PSVR, true) != 0) {
            XRSettings.LoadDeviceByName(VRDeviceNames.PSVR);
        }

        XRSettings.enabled = true;
        XRSettings.eyeTextureResolutionScale = 1.4f;
        XRSettings.showDeviceView = true;
#pragma warning disable 618
        XRDevice.SetTrackingSpaceType(TrackingSpaceType.RoomScale);
#pragma warning restore 618

        ResetMoveControllersTrackingAsync();

        // wait (some time) until we get the tracking so it wont "jump" in the next scene
        PlayStationVRTrackingStatus status = PlayStationVRTrackingStatus.NotTracking;
        float timeoutTime = Time.time + kVRSetupMaxDuration;
        while (status != PlayStationVRTrackingStatus.Tracking && Time.time < timeoutTime && !_debugSkipHmdRequirement) {
            Tracker.GetTrackedDeviceStatus(PlayStationVR.GetHmdHandle(), out status);
            // Previously yield return null in a coroutine, so this roughly simulates that
            await Task.Delay(millisecondsDelay: 250, cancellationToken);
        }

        _vrState = VRState.Started;

        _loadNextSceneCallback();
    }

    private void VRShutdown() {

        if (_vrState is VRState.ShuttingDown or VRState.NotStarted) {
            return;
        }

        _vrState = VRState.ShuttingDown;

        XRSettings.LoadDeviceByName(VRDeviceNames.None);

        XRSettings.enabled = false;

        // Unregister the callbacks needed to detect resetting the HMD
        Utility.onSystemServiceEvent -= OnSystemServiceEvent;
        PlayStationVR.onDeviceEvent -= OnDeviceEvent;

        Debug.Log("Restart");
        _vrState = VRState.NotStarted;

        CancelVRSetupTask();
        CancelControllersSetupTask();

        _restartPsvrSceneCallback();
    }

    private async Task WaitForSubsystem(CancellationToken cancellationToken) {

        XRDisplaySubsystem xrSubsystem = XRSubsystemHelper.GetCurrentDisplaySubsystem();
        while (xrSubsystem == null || !xrSubsystem.running) {
            // previously yield return NewWaitForSeconds(1) so this should simulate that
            await Task.Delay(millisecondsDelay: 1000, cancellationToken);
        }
    }

    private async void SetupHmdDevice() {

        try {
            await SetupHmdDeviceAsync();
        }
        catch (Exception e) {
            Debug.LogError($"error when setting up device {e.Message}");
        }
    }

    private async Task SetupHmdDeviceAsync() {

        // There are multiple sources that can trigger setting up HMD device,
        // yet when we already are in the process of setting it up (dialog) there is no reason to call for that again
        if (_isDialogOpened) {
            return;
        }

        XRSettings.showDeviceView = true;
        DialogResult result;

        // If skipHmdRequirement is on, we still display hmd dialog once to notify the user
        do {
            result = await ShowSetupDialog();
        } while (result == DialogResult.UserCanceled && !_debugSkipHmdRequirement);

        if (_vrState != VRState.Started) {
            await VRSetupAsync();
        }
    }

    private Task<DialogResult> ShowSetupDialog() {

        _isDialogOpened = true;
        var tsc = new TaskCompletionSource<DialogResult>();
        HmdSetupDialog.OpenAsync(0, OnHmdSetupDialogCompleted);

        void OnHmdSetupDialogCompleted(DialogStatus status, DialogResult result) {

            _isDialogOpened = false;
            tsc.SetResult(result);
        }

        return tsc.Task;
    }

    // Unregister and re-register the controllers to reset them
    private async void ResetMoveControllersTrackingAsync() {

#if UNITY_EDITOR
        if (Application.isPlaying) {
            return;
        }
#endif
        try {
            UnregisterMoveControllers();

            CancelControllersSetupTask();
            _registerMoveControllersCancellationTokenSource = new();
            await RegisterMoveControllersAsync(_registerMoveControllersCancellationTokenSource.Token);
        }
        catch (Exception e) {
            Debug.LogError($"error when resetting move controllers tracking {e.Message}");
        }
    }

    private void UpdateMoveTransforms() {

        // Perform tracking for the primary controller, if we've got a handle
        foreach (PSMoveDevice moveDevice in _psMoveDevices) {
            if (moveDevice.handle >= 0) {
                Vector3 pos;
                if (Tracker.GetTrackedDevicePosition(moveDevice.handle, out pos) == PlayStationVRResult.Ok) {
                    moveDevice.position = pos;
                }
                Quaternion rot;
                if (Tracker.GetTrackedDeviceOrientation(moveDevice.handle, out rot) == PlayStationVRResult.Ok) {
                    moveDevice.rotation = rot;
                }
            }
        }
    }

    // PS Move vibration documented at: https://ps4.siedev.net/resources/documents/SDK/5.500/Move-Reference/0012.html
    private void UpdateMoveVibrations() {

        for (int moveDeviceIndex = 0; moveDeviceIndex < _psMoveDevices.Length; moveDeviceIndex++) {
            var moveDevice = _psMoveDevices[moveDeviceIndex];
            moveDevice.vibationTimer -= Time.unscaledDeltaTime;
            if (moveDevice.vibationTimer < 0.0f) {
                PS4Input.MoveSetVibration(slot: 0, index: moveDeviceIndex, motor: 0);
                moveDevice.vibationTimer = 0.0f;
            }
        }
    }

    // Register Move device(s) to track
    private async Task RegisterMoveControllersAsync(CancellationToken cancellationToken) {

        var primaryHandles = new int[1];
        var secondaryHandles = new int[1];
        PS4Input.MoveGetUsersMoveHandles(1, primaryHandles, secondaryHandles);
        _psMoveDevices[0].handle = primaryHandles[0];
        _psMoveDevices[1].handle = secondaryHandles[0];

        for (int moveDeviceIndex = 0; moveDeviceIndex < 2; moveDeviceIndex++) {
            while (PS4Input.MoveIsConnected(0, moveDeviceIndex) == false) {
                await Task.Delay(100, cancellationToken);
            }
            PSMoveDevice moveDevice = _psMoveDevices[moveDeviceIndex];
            // Get the tracking for the Move device, and wait for it to start
            var result = Tracker.RegisterTrackedDevice(PlayStationVRTrackedDevice.DeviceMove, moveDevice.handle, PlayStationVRTrackingType.Absolute, PlayStationVRTrackerUsage.OptimizedForHmdUser);
            if (result == PlayStationVRResult.Ok) {
                var trackingStatus = new PlayStationVRTrackingStatus();

                while (trackingStatus == PlayStationVRTrackingStatus.NotStarted) {
                    Tracker.GetTrackedDeviceStatus(moveDevice.handle, out trackingStatus);
                    await Task.Delay(100, cancellationToken);
                }
            }
            else {
                Debug.LogError("Tracking failed for DeviceMove! This may be because you're trying to register too many devices at once.");
                moveDevice.handle = -1;
            }
        }
    }

    // Remove the registered Move devices from tracking and reset the transform
    private void UnregisterMoveControllers() {

        // We can only unregister tracked devices while in VR, or else a crash may occur
        if (XRSettings.enabled) {
            foreach (PSMoveDevice moveDevice in _psMoveDevices) {
                if (moveDevice.handle >= 0) {
                    Tracker.UnregisterTrackedDevice(moveDevice.handle);
                    moveDevice.handle = -1;
                    moveDevice.position = Vector3.zero;
                    moveDevice.rotation = Quaternion.identity;
                }
            }
        }
    }

    // HMD recenter happens in this event
    private void OnSystemServiceEvent(Utility.sceSystemServiceEventType eventType) {

        Debug.LogFormat("OnSystemServiceEvent: {0}", eventType);
        if (eventType == Utility.sceSystemServiceEventType.ResetVrPosition) {
            ResetMoveControllersTrackingAsync();
            didRecenterEvent?.Invoke();
        }
    }

    // This handles disabling VR in the event that the HMD has been disconnected
    private bool OnDeviceEvent(PlayStationVR.deviceEventType eventType, int value) {

        var handledEvent = false;
        switch (eventType) {
            case PlayStationVR.deviceEventType.deviceStarted:
                Debug.LogFormat("### OnDeviceEvent: deviceStarted: {0}", value);
                break;
            case PlayStationVR.deviceEventType.deviceStopped:
                Debug.LogFormat("### OnDeviceEvent: deviceStopped: {0}", value);
                VRShutdown();
                break;
            case PlayStationVR.deviceEventType.StatusChanged: // e.g. HMD unplugged
                var devstatus = (VRDeviceStatus)value;
                Debug.LogFormat("### OnDeviceEvent: VRDeviceStatus: {0}", devstatus);
                if (devstatus != VRDeviceStatus.Ready) {
                    // TRC R4026 suggests showing the HMD Setup Dialog if the device status becomes non-ready
                    SetupHmdDevice();
                }
                handledEvent = true;
                break;
            case PlayStationVR.deviceEventType.MountChanged:
                var status = (VRHmdMountStatus)value;
                Debug.LogFormat("### OnDeviceEvent: VRHmdMountStatus: {0}", status);
                handledEvent = true;
                break;
            case PlayStationVR.deviceEventType.CameraChanged:
                // If the event is for the camera and the value is 0, the camera has been disconnected
                Debug.LogFormat("### OnDeviceEvent: CameraChanged: {0}", value);
                if (value == 0) {
                    SetupHmdDevice();
                }
                handledEvent = true;
                break;
            case PlayStationVR.deviceEventType.HmdHandleInvalid:
                // Unity will handle this automatically, please see API documentation
                Debug.LogFormat("### OnDeviceEvent: HmdHandleInvalid: {0}", value);
                if (value == 0) {
                    // This event handles uss-1855 issue, when user turns off their PSVR device during unity logo screen
                    SetupHmdDevice();
                    handledEvent = true;
                }
                break;
            case PlayStationVR.deviceEventType.DeviceRestarted:
                // Unity will handle this automatically, please see API documentation
                Debug.LogFormat("### OnDeviceEvent: DeviceRestarted: {0}", value);
                break;
            case PlayStationVR.deviceEventType.DeviceStartedError:
                // Device started error, because we're allowing hmd skip
                if (_debugSkipHmdRequirement) {
                    handledEvent = true;
                    break;
                }
                throw new ArgumentOutOfRangeException("eventType", eventType, null);
            default:
                throw new ArgumentOutOfRangeException("eventType", eventType, null);
        }

        return handledEvent;
    }

    public void SetPSMoveVibration(int moveDeviceIndex, float strength) {

#if !UNITY_EDITOR
        if (moveDeviceIndex >= 0 && moveDeviceIndex < _psMoveDevices.Length && _psMoveDevices[moveDeviceIndex] != null) {
            var moveDevice = _psMoveDevices[moveDeviceIndex];
            moveDevice.vibationTimer = 0.035f;
            strength = Mathf.Clamp(strength, 0.0f, 0.9f);
            PS4Input.MoveSetVibration(slot: 0, index: moveDeviceIndex, motor: 63 + (int)(strength * 192.0f)); // 0->63 = no vibration, 64->255 = vibration
        }
#endif
    }

    public Vector3 GetMovePosition(int moveDeviceIndex) {

        if (moveDeviceIndex >= 0 && moveDeviceIndex < _psMoveDevices.Length && _psMoveDevices[moveDeviceIndex] != null) {
            return _psMoveDevices[moveDeviceIndex].position;
        }

        return Vector3.zero;
    }

    public Quaternion GetMoveRotation(int moveDeviceIndex) {

        if (moveDeviceIndex >= 0 && moveDeviceIndex < _psMoveDevices.Length && _psMoveDevices[moveDeviceIndex] != null) {
            return _psMoveDevices[moveDeviceIndex].rotation;
        }

        return Quaternion.identity;
    }

    public bool GetPSMoveButtonDown(int moveDeviceIndex, PSMoveButton button) {

        int buttonsMask = PS4Input.MoveGetButtons(0, moveDeviceIndex);
        return (buttonsMask & (int)button) != 0;
    }

    public float GetPSMoveAnalog(int moveDeviceIndex) {

#if UNITY_EDITOR
        return 0;
#else
        int analog = PS4Input.MoveGetAnalogButton(0, moveDeviceIndex);
        return (float)analog / 255;
#endif
    }

    public void Recenter() {

        ResetMoveControllersTrackingAsync();
        didRecenterEvent?.Invoke();
    }

    private void CancelVRSetupTask() {

        if (_vrSetupCancellationTokenSource != null) {
            _vrSetupCancellationTokenSource.Cancel();
            _vrSetupCancellationTokenSource = null;
        }
    }

    private void CancelControllersSetupTask() {

        if (_registerMoveControllersCancellationTokenSource != null) {
            _registerMoveControllersCancellationTokenSource.Cancel();
            _registerMoveControllersCancellationTokenSource = null;
        }
    }
}
#endif
