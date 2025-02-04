using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HMUI {

    public class ButtonStaticAnimations : MonoBehaviour {

        [SerializeField] HMUI.NoTransitionsButton _button = default;
        
        [Space]
        [SerializeField] AnimationClip _normalClip = default;
        [SerializeField] AnimationClip _highlightedClip = default;
        [SerializeField] AnimationClip _pressedClip = default;
        [SerializeField] AnimationClip _disabledClip = default;

        private bool _didStart = false;

        protected void Awake() {

            _button.selectionStateDidChangeEvent += HandleButtonSelectionStateDidChange;
        }

        protected void Start() {
            
            _didStart = true;
            HandleButtonSelectionStateDidChange(_button.selectionState);
        }

        protected void OnEnable() {

            HandleButtonSelectionStateDidChange(_button.selectionState);
        }

        protected void OnDestroy() {

            if (_button != null) {
                _button.selectionStateDidChangeEvent -= HandleButtonSelectionStateDidChange;
            }
        }

        private void HandleButtonSelectionStateDidChange(NoTransitionsButton.SelectionState state) {

            if (!_didStart || !isActiveAndEnabled) {
                return;
            }

            AnimationClip clip = null;
            switch (state) {
                case NoTransitionsButton.SelectionState.Normal:
                    clip = _normalClip;
                    break;
                case NoTransitionsButton.SelectionState.Highlighted:
                    clip = _highlightedClip;
                    break;
                case NoTransitionsButton.SelectionState.Pressed:
                    clip = _pressedClip;
                    break;
                case NoTransitionsButton.SelectionState.Disabled:
                    clip = _disabledClip;
                    break;
            }

            if (clip != null) {
                clip.SampleAnimation(gameObject, 0.0f);
            }
        }
    }
}