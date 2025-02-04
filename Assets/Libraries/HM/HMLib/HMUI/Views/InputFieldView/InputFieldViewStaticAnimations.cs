using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HMUI {

    public class InputFieldViewStaticAnimations : MonoBehaviour {

        [SerializeField] InputFieldView _inputFieldView = default;
        
        [Space]
        [SerializeField] AnimationClip _normalClip = default;
        [SerializeField] AnimationClip _highlightedClip = default;
        [SerializeField] AnimationClip _pressedClip = default;
        [SerializeField] AnimationClip _disabledClip = default;
        [SerializeField] AnimationClip _selectedClip = default;

        private bool _didStart = false;

        protected void Awake() {

            _inputFieldView.selectionStateDidChangeEvent += HandleInputFieldViewSelectionStateDidChange;
        }

        protected void Start() {
            
            _didStart = true;
            HandleInputFieldViewSelectionStateDidChange(_inputFieldView.selectionState);
        }

        protected void OnEnable() {

            HandleInputFieldViewSelectionStateDidChange(_inputFieldView.selectionState);
        }

        protected void OnDestroy() {

            if (_inputFieldView != null) {
                _inputFieldView.selectionStateDidChangeEvent -= HandleInputFieldViewSelectionStateDidChange;
            }
        }

        private void HandleInputFieldViewSelectionStateDidChange(InputFieldView.SelectionState state) {

            if (!_didStart || !isActiveAndEnabled) {
                return;
            }

            AnimationClip clip = null;
            switch (state) {
                case InputFieldView.SelectionState.Normal:
                    clip = _normalClip;
                    break;
                case InputFieldView.SelectionState.Highlighted:
                    clip = _highlightedClip;
                    break;
                case InputFieldView.SelectionState.Pressed:
                    clip = _pressedClip;
                    break;
                case InputFieldView.SelectionState.Disabled:
                    clip = _disabledClip;
                    break;
                case InputFieldView.SelectionState.Selected:
                    clip = _selectedClip;
                    break;
            }

            if (clip != null) {
                clip.SampleAnimation(gameObject, 0.0f);
            }
        }
    }
}