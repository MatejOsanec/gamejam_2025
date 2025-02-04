using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

namespace HMUI {

    [RequireComponent(typeof(RectTransform))]
    public class TextSlider : Selectable, IBeginDragHandler, IDragHandler, IInitializePotentialDragHandler, ICanvasElement {

        [SerializeField] TextMeshProUGUI _valueText = default;
        [SerializeField] RectTransform _handleRect = default;

        [Space]
        [SerializeField] bool _enableDragging = true;
        [SerializeField] float _handleSize = 2.0f;
        [SerializeField] float _valueSize = 10.0f;
        [SerializeField] float _separatorSize = 1.5f;
        [SerializeField] int _numberOfSteps = 0;

        [Space]
        [Range(0f, 1f)]
        [SerializeField] float _normalizedValue = default;

        public Color valueTextColor { set => _valueText.color = value; }

        public RectTransform handleRect {
            get => _handleRect;
            set {
                if (SetPropertyUtility.SetClass(ref _handleRect, value)) {
                    UpdateCachedReferences();
                    UpdateVisuals();
                }
            }
        }

        public Color handleColor {
            set {
                if (_handleGraphic != null) {
                    _handleGraphic.color = value;
                }
            }
        }

        public float handleSize {
            get => _handleSize;
            set {
                if (SetPropertyUtility.SetStruct(ref _handleSize, value)) UpdateVisuals();
            }
        }

        public float valueSize {
            get => _valueSize;
            set {
                if (SetPropertyUtility.SetStruct(ref _valueSize, value)) UpdateVisuals();
            }
        }

        public float separatorSize {
            get => _separatorSize;
            set {
                if (SetPropertyUtility.SetStruct(ref _separatorSize, value)) UpdateVisuals();
            }
        }

        public int numberOfSteps {
            get => _numberOfSteps;
            set {
                if (SetPropertyUtility.SetStruct(ref _numberOfSteps, value)) {
                    SetNormalizedValue(_normalizedValue);
                    UpdateVisuals();
                }
            }
        }

        public float normalizedValue {
            get {
                float val = _normalizedValue;
                if (_numberOfSteps > 1)
                    val = Mathf.Round(val * (_numberOfSteps - 1)) / (_numberOfSteps - 1);
                return val;
            }
            set => SetNormalizedValue(value, sendCallback: false);
        }

        public event System.Action<TextSlider, float> normalizedValueDidChangeEvent;

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

        public void Refresh() {

            UpdateVisuals();
        }

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
        protected void SetNormalizedValue(float input) {

            SetNormalizedValue(input, sendCallback: true);
        }

        private void SetNormalizedValue(float input, bool sendCallback) {

            float currentNormalizedValue = _normalizedValue;
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

            if (_containerRect != null && _valueText != null) {

                float containerWidth = _containerRect.rect.width;

                Vector2 anchorMin = new Vector2(0.0f, 0.0f);
                Vector2 anchorMax = new Vector2(0.0f, 1.0f);

                float movement = normalizedValue * (containerWidth - handleSize);

                _tracker.Add(this, _handleRect, DrivenTransformProperties.AnchorMax);
                _tracker.Add(this, _handleRect, DrivenTransformProperties.AnchorMin);
                _tracker.Add(this, _handleRect, DrivenTransformProperties.SizeDelta);
                _tracker.Add(this, _handleRect, DrivenTransformProperties.Pivot);
                _tracker.Add(this, _handleRect, DrivenTransformProperties.AnchoredPosition);
                _handleRect.anchorMin = anchorMin;
                _handleRect.anchorMax = anchorMax;
                _handleRect.sizeDelta = new Vector2(handleSize, 0.0f);
                _handleRect.pivot = new Vector2(0.0f, 0.5f);
                _handleRect.anchoredPosition = new Vector2(movement, 0.0f);

                var valueTextRect = (RectTransform)_valueText.transform;
                _tracker.Add(this, valueTextRect, DrivenTransformProperties.AnchorMax);
                _tracker.Add(this, valueTextRect, DrivenTransformProperties.AnchorMin);
                _tracker.Add(this, valueTextRect, DrivenTransformProperties.SizeDelta);
                _tracker.Add(this, valueTextRect, DrivenTransformProperties.Pivot);
                _tracker.Add(this, valueTextRect, DrivenTransformProperties.AnchoredPosition);
                valueTextRect.anchorMin = anchorMin;
                valueTextRect.anchorMax = anchorMax;
                valueTextRect.sizeDelta = new Vector2(valueSize, 0.0f);

                if (movement + separatorSize + valueSize >= _containerRect.rect.width) {
                    valueTextRect.pivot = new Vector2(1.0f, 0.5f);
                    valueTextRect.anchoredPosition = new Vector2(movement - separatorSize, 0.0f);
                    _valueText.alignment = TMPro.TextAlignmentOptions.CaplineRight;
                }
                else {
                    valueTextRect.pivot = new Vector2(0.0f, 0.5f);
                    valueTextRect.anchoredPosition = new Vector2(movement + handleSize + separatorSize, 0.0f);
                    _valueText.alignment = TMPro.TextAlignmentOptions.CaplineLeft;
                }

                _valueText.text = TextForNormalizedValue(normalizedValue);
            }
        }

        // Update the scroll bar's position based on the mouse.
        private void UpdateDrag(PointerEventData eventData) {

            if (eventData.button != PointerEventData.InputButton.Left) {
                return;
            }

            if (_containerRect == null || eventData.hovered.Count == 0) {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_containerRect, eventData.position, eventData.pressEventCamera, out var localPos)) {
                return;
            }

            if (float.IsNaN(localPos.x) || float.IsNaN(localPos.y)) {
                return;
            }

            var handleCenterRelativeToContainerCorner = localPos - _containerRect.rect.position - new Vector2(_handleRect.rect.width * 0.5f, 0.0f);
            var handleCorner = handleCenterRelativeToContainerCorner - (_handleRect.rect.size - _handleRect.sizeDelta) * 0.5f;

            float remainingSize = _containerRect.rect.width * (1 - handleSize / _containerRect.rect.width);
            if (remainingSize <= 0) {
                return;
            }

            SetNormalizedValue(handleCorner.x / remainingSize);
        }

        private bool MayDrag(PointerEventData eventData) {

            return IsActive() && IsInteractable() && eventData.button == PointerEventData.InputButton.Left;
        }

        public virtual void OnBeginDrag(PointerEventData eventData) {

            if (!MayDrag(eventData) || !_enableDragging) {
                return;
            }

            if (_containerRect == null) {
                return;
            }
        }

        public virtual void OnDrag(PointerEventData eventData) {

            if (!MayDrag(eventData) || !_enableDragging) {
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

        protected virtual string TextForNormalizedValue(float normalizedValue) {

            return normalizedValue.ToString();
        }
    }
}
