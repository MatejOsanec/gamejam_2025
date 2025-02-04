using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using InputDevice = UnityEngine.XR.InputDevice;

public class UnityXRController {
    
    [Serializable]
    public class Configuration {

        [field: SerializeField] public InputActionReference positionActionReference { get; private set; }
        [field: SerializeField] public InputActionReference orientationActionReference { get; private set; }
        [field: SerializeField] public InputActionReference thumbstickActionReference { get; private set; }

        public UnityXRController CreateController(XRNode node) {

            return new UnityXRController(node, positionActionReference.action, orientationActionReference.action, thumbstickActionReference.action);
        }
    }

    public readonly InputAction positionAction;
    public readonly InputAction rotationAction;
    public readonly InputAction thumbstickAction;
    public readonly XRNode node;
    public IUnityXRHapticsHandler hapticsHandler => _hapticsHandler;
    public UnityXRHelper.VRControllerManufacturerName manufacturerName { get; private set; }

    private IUnityXRHapticsHandler _hapticsHandler;

    private UnityXRController(XRNode node, InputAction positionAction, InputAction rotationAction, InputAction thumbstickAction) {

        this.positionAction = positionAction;
        positionAction.Enable();
        this.rotationAction = rotationAction;
        rotationAction.Enable();
        this.thumbstickAction = thumbstickAction;
        thumbstickAction.Enable();
        manufacturerName = UnityXRHelper.VRControllerManufacturerName.Undefined;
        this.node = node;
        _hapticsHandler = new DefaultUnityXRHapticsHandler(node);
    }

    public bool SetupController(InputDevice device, MonoBehaviour coroutineRunner) {

        var result = TryToUpdateManufacturerName(device);
        UpdateHapticsHandler(coroutineRunner);
        return result;
    }

    private void UpdateHapticsHandler(MonoBehaviour coroutineRunner) {

        if (manufacturerName is UnityXRHelper.VRControllerManufacturerName.Valve or UnityXRHelper.VRControllerManufacturerName.Microsoft) {
            if (_hapticsHandler is not KnucklesUnityXRHapticsHandler) {
                _hapticsHandler = new KnucklesUnityXRHapticsHandler(node, coroutineRunner);
            }
        } else {
            if (_hapticsHandler is not DefaultUnityXRHapticsHandler) {
                _hapticsHandler.Dispose();
                _hapticsHandler = new DefaultUnityXRHapticsHandler(node);
            }
        }
    }

    private bool TryToUpdateManufacturerName(InputDevice device) {

        string manufacturer = device.manufacturer;
        if (string.IsNullOrEmpty(manufacturer)) {
            manufacturerName = UnityXRHelper.VRControllerManufacturerName.Undefined;
            return false;
        }
        manufacturer = manufacturer.ToLowerInvariant();
        if (manufacturer.Contains("oculus")) {
            manufacturerName = UnityXRHelper.VRControllerManufacturerName.Oculus;
            return true;
        }
        if (manufacturer.Contains("htc")) {
            manufacturerName = UnityXRHelper.VRControllerManufacturerName.HTC;
            return true;
        }
        if (manufacturer.Contains("valve")) {
            manufacturerName = UnityXRHelper.VRControllerManufacturerName.Valve;
            return true;
        }
        if (manufacturer.Contains("microsoft")) {
            manufacturerName = UnityXRHelper.VRControllerManufacturerName.Microsoft;
            return true;
        }
        manufacturerName = UnityXRHelper.VRControllerManufacturerName.Unknown;
        return false;
    }

    public void ResetManufacturerName() {

        manufacturerName = UnityXRHelper.VRControllerManufacturerName.Undefined;
    }
}
