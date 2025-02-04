using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

namespace HMUI {

    [RequireComponent(typeof(RectTransform))]
    public class CircleSlider : Selectable, IBeginDragHandler, IDragHandler, IInitializePotentialDragHandler, ICanvasElement {

        [SerializeField] RectTransform _handleRect = default;

        [Space]
        [SerializeField] float _cursorRadius = 12.5f;
        [SerializeField] float _normalizedValue = default;

        public RectTransform handleRect { get { return _handleRect; } set { if (SetPropertyUtility.SetClass(ref _handleRect, value)) { UpdateCachedReferences(); UpdateVisuals(); } } }
        public Color handleColor { set { if (_handleGraphic != null) { _handleGraphic.color = value; } } }
        public float normalizedValue { get => _normalizedValue; set => SetNormalizedValue(value, sendCallback: false); }

        public event System.Action<CircleSlider, float> normalizedValueDidChangeEvent;

        private RectTransform _containerRect;
        private Graphic _handleGraphic;
        private DrivenRectTransformTracker _tracker;

#if UNITY_EDITOR
        protected override void OnValidate() {

            base.OnValidate();

            // This can be invoked before OnEnabled is called. So we shouldn't be accessing other objects, before OnEnable is called.
            if (IsActive()) {
                UpdateCachedReferences();
                SetNormalizedValue(_normalizedValue, false);
                // Update rects since other things might affect them even if value didn't change.
                UpdateVisuals();
            }

            // To prevent "has been scheduled for reimport during the Refresh loop and Loading of it has been attempted." error
            if (UnityEditor.EditorApplication.isUpdating) {
                return;
            }

            var prefabAssetType = UnityEditor.PrefabUtility.GetPrefabAssetType(this);
            if (prefabAssetType == UnityEditor.PrefabAssetType.NotAPrefab && !Application.isPlaying) {
                CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            }
        }

#endif // if UNITY_EDITOR

        public virtual void Rebuild(CanvasUpdate executing) {
#if UNITY_EDITOR
            if (executing == CanvasUpdate.Prelayout) {
                normalizedValueDidChangeEvent?.Invoke(this, normalizedValue);
            }
#endif
        }

        public virtual void LayoutComplete() { }
        public virtual void GraphicUpdateComplete() { }

        protected override void OnEnable() {

            base.OnEnable();
            UpdateCachedReferences();
            SetNormalizedValue(_normalizedValue, false);
            // Update rects since they need to be initialized correctly.
            UpdateVisuals();
        }

        protected override void OnDisable() {

            _tracker.Clear();
            base.OnDisable();
        }

        private void UpdateCachedReferences() {

            if (_handleRect && _handleRect.parent != null) {
                _containerRect = _handleRect.parent.GetComponent<RectTransform>();
            }
            else {
                _containerRect = null;
            }

            if (_handleRect) {
                _handleGraphic = _handleRect.gameObject.GetComponent<Graphic>();
            }
        }

        // Update the visible Image.
        private void SetNormalizedValue(float input) {

            SetNormalizedValue(input, sendCallback: true);
        }

        private void SetNormalizedValue(float input, bool sendCallback) {

            var currentNormalizedValue = _normalizedValue;

            // Clamp the input
            _normalizedValue = Mathf.Clamp01(input);

            // If the stepped value doesn't match the last one, it's time to update
            if (currentNormalizedValue == normalizedValue) {
                return;
            }

            UpdateVisuals();

            if (sendCallback) {
                normalizedValueDidChangeEvent?.Invoke(this, normalizedValue);
            }
        }

        protected override void OnRectTransformDimensionsChange() {

            base.OnRectTransformDimensionsChange();

            // This can be invoked before OnEnabled is called. So we shouldn't be accessing other objects, before OnEnable is called.
            if (!IsActive()) {
                return;
            }

            UpdateVisuals();
        }

        // Force-update the scroll bar. Useful if you've changed the properties and want it to update visually.
        protected virtual void UpdateVisuals() {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                UpdateCachedReferences();
            }
#endif
            _tracker.Clear();

            if (_containerRect != null) {

                float containerWidth = _containerRect.rect.width;
                float containerHeight = _containerRect.rect.height;

                _tracker.Add(this, _handleRect, DrivenTransformProperties.Pivot);
                _tracker.Add(this, _handleRect, DrivenTransformProperties.AnchoredPosition);

                var angle = _normalizedValue * Mathf.PI * 2.0f;

                _handleRect.pivot = new Vector2(0.5f, 0.5f);
                _handleRect.localPosition = new Vector2(Mathf.Cos(angle) * _cursorRadius, Mathf.Sin(angle) * _cursorRadius);
            }
        }

        // Update the scroll bar's position based on the mouse.
        private void UpdateDrag(PointerEventData eventData) {

            if (eventData.button != PointerEventData.InputButton.Left) {
                return;
            }

            if (_containerRect == null) {
                return;
            }

            Vector2 localPos;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_containerRect, eventData.position, eventData.pressEventCamera, out localPos)) {
                return;
            }

            if (float.IsNaN(localPos.x) || float.IsNaN(localPos.y)) {
                return;
            }

            var angle = Vector2.SignedAngle(new Vector2(1.0f, 0.0f), localPos);
            if (angle < 0.0f) {
                angle += 360.0f;
            }

            SetNormalizedValue(angle / 360.0f);
        }

        private bool MayDrag(PointerEventData eventData) {

            return IsActive() && IsInteractable() && eventData.button == PointerEventData.InputButton.Left;
        }

        public virtual void OnBeginDrag(PointerEventData eventData) {

            if (!MayDrag(eventData)) {
                return;
            }

            if (_containerRect == null) {
                return;
            }
        }

        public virtual void OnDrag(PointerEventData eventData) {

            if (!MayDrag(eventData)) {
                return;
            }

            if (_containerRect != null) {
                UpdateDrag(eventData);
            }
        }

        public override void OnPointerDown(PointerEventData eventData) {

            if (!MayDrag(eventData)) {
                return;
            }

            base.OnPointerDown(eventData);

            if (_containerRect != null) {
                UpdateDrag(eventData);
            }
        }

        public virtual void OnInitializePotentialDrag(PointerEventData eventData) {

            eventData.useDragThreshold = false;
        }
    }
}
