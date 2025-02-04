using UnityEngine;

public abstract class VRControllerTransformOffset : MonoBehaviour {

    /// <summary>
    /// When true, it should apply rotation first and then translation on the offsets
    /// It also has different default poses for controllers that matches the controller pose in version 1.29.1
    /// </summary>
    public abstract bool alternativeHandling { get; }
    public abstract Vector3 leftPositionOffset { get; }
    public abstract Vector3 leftRotationOffset { get; }
    public abstract Vector3 rightPositionOffset { get; }
    public abstract Vector3 rightRotationOffset { get; }
}
