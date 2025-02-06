using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FollowOrientation : MonoBehaviour
{
    public Transform target; // The target transform to follow
    public float delay = 0.1f; // The delay in seconds
    private Quaternion previousTargetRotation;
    void Start()
    {
        // Initialize the previous target rotation
        previousTargetRotation = target.rotation;
    }
    void LateUpdate()
    {
        // Calculate the rotation we want to be at, based on the delay
        Quaternion targetRotation = Quaternion.Slerp(previousTargetRotation, target.rotation, Time.deltaTime / delay);
        // Rotate this transform to match the target rotation
        transform.rotation = targetRotation;
        // Update the previous target rotation for next frame
        previousTargetRotation = targetRotation;
    }
}
