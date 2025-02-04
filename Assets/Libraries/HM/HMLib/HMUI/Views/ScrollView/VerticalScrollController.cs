namespace HMUI {

    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;

    [RequireComponent(typeof(VerticalScrollIndicator))]
    public class VerticalScrollController : MonoBehaviour, IPointerDownHandler, IDragHandler, IInitializePotentialDragHandler {

        public event Action<float> updateScrollPositionEvent;

        [SerializeField] private VerticalScrollIndicator _verticalScrollIndicator;
        [SerializeField] private RectTransform _scrollRectTransform;

        private float _dragPosition;
        private RectTransform _handleRectTransform;

        protected void Awake() {

            _handleRectTransform = _verticalScrollIndicator.handle;
        }

        protected void OnValidate() {

            if (_verticalScrollIndicator == null) {
                _verticalScrollIndicator = GetComponent<VerticalScrollIndicator>();
            }

            if (_scrollRectTransform == null) {
                _scrollRectTransform = GetComponent<RectTransform>();
            }
        }

        public void OnPointerDown(PointerEventData eventData) {

            var handleRect = _handleRectTransform.GetWorldRect();
            var scrollRect = _scrollRectTransform.GetWorldRect();

            _dragPosition = 1 - ((eventData.position.y - scrollRect.y - (handleRect.height * 0.5f)) / (scrollRect.height - handleRect.height));

            if (!handleRect.Contains(eventData.position)) {
                updateScrollPositionEvent?.Invoke(_dragPosition);
            }
        }

        public void OnDrag(PointerEventData eventData) {

            var scrollRect = _scrollRectTransform.GetWorldRect();
            _dragPosition -= eventData.delta.y / scrollRect.height;
            updateScrollPositionEvent?.Invoke(_dragPosition);
        }

        public void OnInitializePotentialDrag(PointerEventData eventData) {

            eventData.useDragThreshold = false;
        }
    }
}
