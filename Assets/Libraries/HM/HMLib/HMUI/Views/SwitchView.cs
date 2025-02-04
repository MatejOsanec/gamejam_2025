using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HMUI {

    [DisallowMultipleComponent]
    [RequireComponent(typeof(ToggleWithCallbacks))]
    public class SwitchView : MonoBehaviour {

        [SerializeField] AnimationType _animationType = AnimationType.OnOff;

        [SerializeField] AnimationClip _normalAnimationClip = default;
        [SerializeField] AnimationClip _highlightedAnimationClip = default;
        [SerializeField] AnimationClip _pressedAnimationClip = default;
        [SerializeField] AnimationClip _disabledAnimationClip = default;

        [Space]
        [DrawIf("_animationType", value: AnimationType.OnOff)]
        [NullAllowedIf(propertyName: "_animationType", ComparisonOperation.NotEqual, AnimationType.OnOff)]
        [SerializeField] AnimationClip _onAnimationClip = default;
        
        [DrawIf("_animationType", value: AnimationType.OnOff)]
        [NullAllowedIf(propertyName: "_animationType", ComparisonOperation.NotEqual, AnimationType.OnOff)]
        [SerializeField] AnimationClip _offAnimationClip = default;

        [DrawIf("_animationType", value: AnimationType.SelectedState)]
        [NullAllowedIf(propertyName: "_animationType", ComparisonOperation.NotEqual, AnimationType.SelectedState)]
        [SerializeField] AnimationClip _selectedAnimationClip = default;
        
        [DrawIf("_animationType", value: AnimationType.SelectedState)]
        [NullAllowedIf(propertyName: "_animationType", ComparisonOperation.NotEqual, AnimationType.SelectedState)]
        [SerializeField] AnimationClip _selectedAndHighlightedAnimationClip = default;

        private enum AnimationType {
            OnOff,
            SelectedState,
        }

        private ToggleWithCallbacks _toggle = default;

        protected void Awake() {

            _toggle = GetComponent<ToggleWithCallbacks>();
        }

        protected void Start() {

            _toggle.onValueChanged.AddListener(HandleOnValueChanged);
            _toggle.stateDidChangeEvent += HandleStateDidChange;

            RefreshVisuals();
        }

        protected void OnDestroy() {

            _toggle.onValueChanged.RemoveListener(HandleOnValueChanged);
            _toggle.stateDidChangeEvent -= HandleStateDidChange;
        }

        private void HandleOnValueChanged(bool value) {

            RefreshVisuals();
        }

        private void HandleStateDidChange(ToggleWithCallbacks.SelectionState value) {

            RefreshVisuals();
        }

        private void RefreshVisuals() {

            if (_animationType == AnimationType.OnOff) {
                if (_toggle.isOn) {
                    _onAnimationClip.SampleAnimation(gameObject, time: 0.0f);
                }
                else {
                    _offAnimationClip.SampleAnimation(gameObject, time: 0.0f);
                }
            }

            switch (_toggle.selectionState) {
                case ToggleWithCallbacks.SelectionState.Normal:
                    if (_animationType == AnimationType.SelectedState && _toggle.isOn) {
                        _selectedAnimationClip.SampleAnimation(gameObject, 0.0f);
                    }
                    else {
                        _normalAnimationClip.SampleAnimation(gameObject, 0.0f);
                    }
                    break;
                case ToggleWithCallbacks.SelectionState.Highlighted:
                    if (_animationType == AnimationType.SelectedState && _toggle.isOn) {
                        _selectedAndHighlightedAnimationClip.SampleAnimation(gameObject, 0.0f);
                    }
                    else {
                        _highlightedAnimationClip.SampleAnimation(gameObject, 0.0f);
                    }
                    break;
                case ToggleWithCallbacks.SelectionState.Pressed:
                    _pressedAnimationClip.SampleAnimation(gameObject, 0.0f);
                    break;
                case ToggleWithCallbacks.SelectionState.Disabled:
                    _disabledAnimationClip.SampleAnimation(gameObject, 0.0f);
                    break;
            }
        }
    }
}
