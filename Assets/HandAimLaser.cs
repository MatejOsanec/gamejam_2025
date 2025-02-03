using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandAimLaser : MonoBehaviour
{
    [SerializeField] Transform origin;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] float downMagnitude = .2f;
    [SerializeField] float leftMagnitude = .0f;
    [SerializeField] Vector3 offsetOriginPosition = Vector3.zero;
    [SerializeField] bool isInversed;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var originPos = origin.position + offsetOriginPosition;
        var originRot = origin.rotation;
        var endLaserPosition = originPos + (originRot * Vector3.up * ((isInversed ? -1 : 1) * 2));
        lineRenderer.SetPosition(0, originPos);
        lineRenderer.SetPosition(1, endLaserPosition);
        transform.position = originPos;
        transform.forward = endLaserPosition - originPos;
    }
}
