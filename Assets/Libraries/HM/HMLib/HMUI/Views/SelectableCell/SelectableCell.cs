using UnityEngine;
using UnityEngine.EventSystems;

namespace HMUI {

    [RequireComponent(typeof(RectTransform))]
    public abstract class SelectableCell : Interactable, IPointerClickHandler, ISubmitHandler, IPointerEnterHandler, IPointerExitHandler {

        [SerializeField] [SignalSender] Signal _wasPressedSignal = default;

        public event System.Action<SelectableCell, TransitionType, object> selectionDidChangeEvent;
        public event System.Action<SelectableCell, TransitionType> highlightDidChangeEvent;
        public event System.Action<SelectableCell> nonInteractableCellWasPressedEvent;

        public enum TransitionType {
            Instant,
            Animated,
        }

        public bool highlighted { get; private set; }
        public bool selected { get; private set; }

        protected virtual void Start() {

            SelectionDidChange(TransitionType.Instant);
            HighlightDidChange(TransitionType.Instant);
        }

        protected virtual void OnDisable() {

            SetHighlight(false, TransitionType.Instant, ignoreCurrentValue: false);
        }

        public void SetSelected(bool value, TransitionType transitionType, object changeOwner, bool ignoreCurrentValue) {

            if (!ignoreCurrentValue && selected == value) {
                return;
            }

            selected = value;
            SelectionDidChange(transitionType);
            selectionDidChangeEvent?.Invoke(this, transitionType, changeOwner);
        }

        public void ClearHighlight(TransitionType transitionType) {

            SetHighlight(value: false, transitionType, ignoreCurrentValue: false);
        }

        private void SetHighlight(bool value, TransitionType transitionType, bool ignoreCurrentValue) {

            if (!ignoreCurrentValue && highlighted == value) {
                return;
            }

            highlighted = value;
            HighlightDidChange(transitionType);
            highlightDidChangeEvent?.Invoke(this, transitionType);
        }

        protected abstract void InternalToggle();

        protected virtual void SelectionDidChange(TransitionType transitionType) { }

        protected virtual void HighlightDidChange(TransitionType transitionType) { }

        public virtual void OnPointerClick(PointerEventData eventData) {

            if (eventData.button != PointerEventData.InputButton.Left) {
                return;
            }

            if (!interactable) {
                nonInteractableCellWasPressedEvent?.Invoke(this);
                return;
            }

            InternalToggle();

            if (_wasPressedSignal != null) {
                _wasPressedSignal.Raise();
            }
        }

        public virtual void OnSubmit(BaseEventData eventData) {

            if (!interactable) {
                return;
            }

            InternalToggle();

            if (_wasPressedSignal != null) {
                _wasPressedSignal.Raise();
            }
        }

        public virtual void OnPointerEnter(PointerEventData eventData) {

            SetHighlight(true, TransitionType.Animated, ignoreCurrentValue: false);
        }

        public virtual void OnPointerExit(PointerEventData eventData) {

            SetHighlight(false, TransitionType.Animated, ignoreCurrentValue: false);
        }
    }
}
