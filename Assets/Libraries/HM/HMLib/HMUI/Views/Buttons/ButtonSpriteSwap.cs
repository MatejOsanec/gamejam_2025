using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HMUI {

    public class ButtonSpriteSwap : MonoBehaviour {

        [SerializeField] protected Sprite _normalStateSprite = default;
        [SerializeField] protected Sprite _highlightStateSprite = default;
        [SerializeField] protected Sprite _pressedStateSprite = default;
        [SerializeField] protected Sprite _disabledStateSprite = default;

        [Space]
        [SerializeField] HMUI.NoTransitionsButton _button = default;
        [SerializeField] protected Image[] _images = default;

        protected bool _didStart = false;

        protected void Awake() {

            _button.selectionStateDidChangeEvent += HandleButtonSelectionStateDidChange;
        }

        protected void Start() {

            _didStart = true;
            RefreshVisualState();
        }

        protected virtual void OnEnable() {

            RefreshVisualState();
        }

        protected void OnDestroy() {

            if (_button != null) {
                _button.selectionStateDidChangeEvent -= HandleButtonSelectionStateDidChange;
            }
        }

        protected virtual void HandleButtonSelectionStateDidChange(NoTransitionsButton.SelectionState state) {

            if (!_didStart || !isActiveAndEnabled) {
                return;
            }

            Sprite sprite = null;

            switch (state) {
                case NoTransitionsButton.SelectionState.Normal:
                    sprite = _normalStateSprite;
                    break;
                case NoTransitionsButton.SelectionState.Highlighted:
                    sprite = _highlightStateSprite;
                    break;
                case NoTransitionsButton.SelectionState.Pressed:
                    sprite = _pressedStateSprite;
                    break;
                case NoTransitionsButton.SelectionState.Disabled:
                    sprite = _disabledStateSprite;
                    break;
            }

            foreach (var image in _images) {
                image.sprite = sprite;
            }
        }

        protected void RefreshVisualState() {

            HandleButtonSelectionStateDidChange(_button.selectionState);
        }
    }
}
