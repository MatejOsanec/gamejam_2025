using UnityEngine;

namespace HMUI {

    public class ButtonSpriteSwapToggle : ButtonSpriteSwap  {

        [SerializeField] bool _resetToggleOnEnable = true;
        [SerializeField] bool _ignoreHighlight = true;

        private bool _isToggled;


        public bool isToggled {

            get => _isToggled;
            set {

                if (_isToggled == value) {
                    return;
                }

                _isToggled = value;
                RefreshVisualState();
            }
        }

        protected override void OnEnable() {

            if (_resetToggleOnEnable) {
                _isToggled = false;
            }
            base.OnEnable();
        }

        protected override void HandleButtonSelectionStateDidChange(NoTransitionsButton.SelectionState state) {

            if (!_didStart || !isActiveAndEnabled) {
                return;
            }
            Sprite sprite = null;
            switch (state) {
                case NoTransitionsButton.SelectionState.Pressed:
                    _isToggled = !_isToggled;
                    sprite = _isToggled ? _pressedStateSprite : _normalStateSprite;
                    break;
                case NoTransitionsButton.SelectionState.Normal:
                    sprite = _isToggled ? _pressedStateSprite : _normalStateSprite;
                    break;
                case NoTransitionsButton.SelectionState.Highlighted:
                    if (!_ignoreHighlight) {
                        sprite = _highlightStateSprite;
                    }
                    break;
                case NoTransitionsButton.SelectionState.Disabled:
                    sprite = _disabledStateSprite;
                    break;
            }
            if (sprite != null) {
                foreach (var image in _images) {
                    image.sprite = sprite;
                }
            }
        }
    }
}
