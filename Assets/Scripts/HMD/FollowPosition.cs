using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FollowPosition : MonoBehaviour
{
    public Transform target; // The target transform to follow
    public float delay = 0.1f; // The delay in seconds
    private Vector3 velocity = Vector3.zero;
    void LateUpdate()
    {
        // Calculate the position we want to be at, based on the delay
        float smoothTime = delay;
        Vector3 targetPosition = Vector3.SmoothDamp(transform.position, target.position, ref velocity, smoothTime);
        // Move this transform to match the target position
        transform.position = targetPosition;
    }
}
