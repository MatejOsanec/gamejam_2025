using Core;
using Gameplay;
using UnityEngine;

public class FlyingBaddieMovement : BeatmapCallbackListener
{
    // Aim Controls
    public Transform targetTransform;
    public float smoothSpeed = 1;

    // Movement Controls
    public float offset = 0;
    public Vector3 syncMultiplier = new Vector3(4,1,8);
    public AnimationCurve curveMovementX;
    public AnimationCurve curveMovementY;
    public AnimationCurve curveMovementZ;

    private Vector3 _basePosition;

    protected override void OnGameInit()
    {
        _basePosition = transform.position;
    }

    public void Update()
    {
        if (!_initialized)
        {
            return;
        }

        var xOffset = Locator.BeatModel.GetCurvedBeatProgress(curveMovementX, syncMultiplier.x, syncMultiplier.x * offset);
        var yOffset = Locator.BeatModel.GetCurvedBeatProgress(curveMovementY, syncMultiplier.y, syncMultiplier.y * offset);
        var zOffset = Locator.BeatModel.GetCurvedBeatProgress(curveMovementZ, syncMultiplier.z, syncMultiplier.z * offset);

        transform.position = _basePosition + new Vector3(xOffset, yOffset, zOffset);


        //aim at player
        Vector3 direction = targetTransform.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
    }
}
