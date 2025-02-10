using Gameplay;
using UnityEngine;

public class FlyingBaddieMovement : BeatmapCallbackListener
{
    [Header("Aim Controls")]
    public Transform targetTransform;
    public float smoothSpeed = 0.5f;

    [Header("Movement Controls")]
    [Range(0,1)]
    public float customOffset = 0;
    public float movementScale = 1;
    [SerializeField]
    private ModulationDefSo modX;
    [SerializeField]
    private ModulationDefSo modY;
    [SerializeField]
    private ModulationDefSo modZ;

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

        transform.position = _basePosition + new Vector3(modX.GetProgressWithCustomOffset(customOffset) * movementScale, modY.GetProgressWithCustomOffset(customOffset) * movementScale, modZ.GetProgressWithCustomOffset(customOffset) * movementScale);


        //aim at player
        Vector3 direction = targetTransform.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
    }
}
