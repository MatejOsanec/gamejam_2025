using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class RotateTransform : MonoBehaviour
{
    public float rotationSpeed = 100f; // Speed of rotation in degrees per second
    void Update()
    {
        // Rotate the transform around its up axis (y-axis)
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
