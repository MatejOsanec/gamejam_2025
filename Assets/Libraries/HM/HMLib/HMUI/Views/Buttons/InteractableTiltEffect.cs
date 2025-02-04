namespace HMUI {
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class InteractableTiltEffect : MonoBehaviour, IPointerEnterHandler, IPointerMoveHandler {

    [Tooltip("This rect transform should be a child of this game object. Otherwise it might mess up raycasting.")]
    [SerializeField] RectTransform _rectTransform;
    [SerializeField] private float _maxHorizontalRotation = 5.0f;
    [SerializeField] private float _maxVerticalRotation = 5.0f;

    private Vector2 _prevLocalPoint;

    public float effectStrengthMultiplier {
        set  {
            _effectStrengthMultiplier = value;
            _rectTransform.localRotation = ComputeNewTargetRotation(_prevLocalPoint);
        }
        get => _effectStrengthMultiplier;
    }

    private float _effectStrengthMultiplier = 1.0f;

    public void OnPointerEnter(PointerEventData eventData)  {

        var localPoint = (Vector2) _rectTransform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition);
        _rectTransform.localRotation = ComputeNewTargetRotation(localPoint);
    }

    public void OnPointerMove(PointerEventData eventData) {

        var localPoint = (Vector2) _rectTransform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition);
        _rectTransform.localRotation = ComputeNewTargetRotation(localPoint);
    }

    private Quaternion ComputeNewTargetRotation(Vector2 localPoint) {

        _prevLocalPoint = localPoint;

        var rect = _rectTransform.rect;
        Vector2 normalizedPosition = new Vector2(
            (localPoint.x - rect.x) / rect.width,
            (localPoint.y - rect.y) / rect.height
        );

        normalizedPosition.x = -normalizedPosition.x * 2.0f + 1.0f;
        normalizedPosition.y = normalizedPosition.y * 2.0f - 1.0f;

        return Quaternion.Euler(normalizedPosition.y * _maxVerticalRotation * _effectStrengthMultiplier, normalizedPosition.x * _maxHorizontalRotation * _effectStrengthMultiplier, 0.0f);
    }
}

}
