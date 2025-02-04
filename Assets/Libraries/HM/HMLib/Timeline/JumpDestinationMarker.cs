using System.ComponentModel;
using UnityEngine;
using UnityEngine.Timeline;

[DisplayName("Jump/JumpDestinationMarker")]
public class JumpDestinationMarker : Marker {

#if BS_TOURS
    protected void OnValidate() {

        hideFlags &= ~HideFlags.HideInHierarchy;
    }

    protected void OnEnable() {

        hideFlags &= ~HideFlags.HideInHierarchy;
    }
#endif
}
