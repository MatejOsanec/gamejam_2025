using System;
using System.Reflection;
using UnityEngine;

/// This needs to be here for OVRPlugin and Hand Tracking to work correctly with disabled domain reload (to workaround a bug inside OVRPlugin)
public class DomainReloadFix : MonoBehaviour {

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void DomainReload() {

        Type ovrPluginType = typeof(OVRPlugin);
        FieldInfo handSkeletonVersionField = ovrPluginType.GetField("<HandSkeletonVersion>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static);
        handSkeletonVersionField.SetValue(null, OVRHandSkeletonVersion.OVR);
    }
}
