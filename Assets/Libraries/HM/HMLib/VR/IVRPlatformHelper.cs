using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public enum VRPlatformSDK {
    OpenXR,
    Oculus,
    Unknown,
}

public interface IVRPlatformHelper {

    /// <summary>
    /// Other app took the focus of the input, for example, when player click on the system menu button.
    /// The game is visible in the background, but player is interacting with a different view/overlay.
    /// </summary>
    event Action inputFocusWasCapturedEvent;

    /// <summary>
    /// Focus return to the game and the inputs should be captured by the game.
    /// For example, when player return to the game from the system menu.
    /// </summary>
    event Action inputFocusWasReleasedEvent;

    /// <summary>
    /// Other app take the VR environment from the game or player remove the headset.
    /// The game is still running, but some process take the VR environment rendered by the game.
    /// For example, player opened Beat Saber and tries to transition to a different app.
    /// The system will show a confirmation window and take the VR environment from the game.
    /// </summary>
    event Action vrFocusWasCapturedEvent;

    /// <summary>
    /// VR environment rendering returns the game.
    /// For example, player tries to transition to a different app, but give up.
    /// When the game starts to be rendered again this event will trigger.
    /// </summary>
    event Action vrFocusWasReleasedEvent;

    /// <summary>
    /// When player remove the headset from his/her head.
    /// On Quest, it means that the proximity sensor can't find anything in the expected range.
    /// </summary>
    event Action hmdUnmountedEvent;

    /// <summary>
    /// When player put the headset back on his/her head.
    /// On Quest, it means that the proximity sensor found something blocking in the expected range.
    /// </summary>
    event Action hmdMountedEvent;
    /// <summary>
    /// When player changes controller or change settings in the controller adjustments and it should update the reference
    /// </summary>
    event Action controllersDidChangeReferenceEvent;
    /// <summary>
    /// When a player controller is disconnected
    /// </summary>
    event Action controllersDidDisconnectEvent;

    bool hasInputFocus { get; }
    bool hasVrFocus { get; }
    bool isAlwaysWireless { get; }
    VRPlatformSDK vrPlatformSDK { get; }
    void TriggerHapticPulse(XRNode node, float duration, float strength, float frequency);
    void StopHaptics(XRNode node);
    bool TryGetPoseOffsetForNode(XRNode node, out Pose poseOffset);
    bool GetNodePose(XRNode nodeType, int idx, out Vector3 pos, out Quaternion rot);
    Pose GetRootPositionOffsetForLegacyNodePose(XRNode node);
    bool TryGetLegacyPoseOffsetForNode(XRNode node, out Vector3 position, out Vector3 rotation);

    Vector2 GetAnyJoystickMaxAxis();

    public float GetTriggerValue(XRNode node);

    Vector2 GetThumbstickValue(XRNode node);
    bool IsAdvancedHapticsSupported(XRNode node);

    bool GetMenuButton();

    bool GetMenuButtonDown();

    void RefreshControllersReference();
}

public static class VRPlatformUtils {

    private const string kTriggerLeftHand = "TriggerLeftHand";
    private const string kTriggerRightHand = "TriggerRightHand";
    public const string kMenuButtonLeftHand = "OpenXRPrimaryButtonLeftHand";
    public const string kMenuButtonRightHand = "OpenXRPrimaryButtonRightHand";
    public const string kMenuButtonOculusTouch = "MenuButtonOculusTouch";

    public static float TriggerValueDefaultImplementation(XRNode node) {

        return node switch {
            XRNode.LeftHand => Input.GetAxis(kTriggerLeftHand),
            XRNode.RightHand => Input.GetAxis(kTriggerRightHand),
            _ => 0
        };
    }

    internal static bool GetMenuButtonDefaultImplementation() {

        return Input.GetButton(kMenuButtonLeftHand) || Input.GetButton(kMenuButtonRightHand);
    }

    internal static bool GetMenuButtonDownDefaultImplementation() {

        return Input.GetButtonDown(kMenuButtonLeftHand) || Input.GetButtonDown(kMenuButtonRightHand);
    }

    public static Vector2 GetAnyJoystickMaxAxisDefaultImplementation(this IVRPlatformHelper vrPlatformHelper) {

        Vector2 left = vrPlatformHelper.GetThumbstickValue(XRNode.LeftHand);
        Vector2 right = vrPlatformHelper.GetThumbstickValue(XRNode.RightHand);
        return new Vector2(MathfExtra.MaxAbs(left.x, right.x), MathfExtra.MaxAbs(left.y, right.y));
    }

    public static void StopXR(IVerboseLogger logger) {

        logger.Log("Stopping XR...");
        var xrManager = XRGeneralSettings.Instance?.Manager;
        if (xrManager == null || !xrManager.isInitializationComplete) {
            logger.Log("XR not initialized.");
            return;
        }
        xrManager.StopSubsystems();
        xrManager.DeinitializeLoader();
        logger.Log("XR stopped completely.");
    }
}
