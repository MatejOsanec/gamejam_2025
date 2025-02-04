using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public static class XRSubsystemHelper {

    [DoesNotRequireDomainReloadInit]
    private static List<XRDisplaySubsystem> s_displaySubsystems;
    [DoesNotRequireDomainReloadInit]
    private static List<XRDisplaySubsystemDescriptor> s_displaySubsystemDescriptors;
    [DoesNotRequireDomainReloadInit]
    private static List<XRInputSubsystem> s_inputSubsystems;

    public static XRDisplaySubsystem GetCurrentDisplaySubsystem() {

        if (s_displaySubsystems == null) {
            s_displaySubsystems = new List<XRDisplaySubsystem>();
        }

        SubsystemManager.GetInstances(s_displaySubsystems);
        if (s_displaySubsystems.Count > 0) {
            return s_displaySubsystems[0];
        }

        return null;
    }

    public static XRDisplaySubsystemDescriptor GetCurrentDisplaySubsystemDescriptor() {

        if (s_displaySubsystemDescriptors == null) {
            s_displaySubsystemDescriptors = new List<XRDisplaySubsystemDescriptor>();
        }

        SubsystemManager.GetSubsystemDescriptors(s_displaySubsystemDescriptors);
        if (s_displaySubsystemDescriptors.Count > 0) {
            return s_displaySubsystemDescriptors[0];
        }

        return null;
    }

    public static XRInputSubsystem GetCurrentInputSubsystem() {

        if (s_inputSubsystems == null) {
            s_inputSubsystems = new List<XRInputSubsystem>();
        }

        SubsystemManager.GetInstances(s_inputSubsystems);
        if (s_inputSubsystems.Count > 0) {
            return s_inputSubsystems[0];
        }

        return null;
    }

}
