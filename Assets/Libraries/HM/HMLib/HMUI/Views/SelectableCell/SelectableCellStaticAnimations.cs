using System;
using UnityEngine;

namespace HMUI {

    public class SelectableCellStaticAnimations : MonoBehaviour {

        [SerializeField] SelectableCell _selectableCell = default;

        [Space]
        [SerializeField] AnimationClip _normalAnimationClip = default;
        [SerializeField] AnimationClip _highlightedAnimationClip = default;
        [SerializeField] AnimationClip _selectedAnimationClip = default;
        [SerializeField] AnimationClip _selectedAndHighlightedAnimationClip = default;

        protected void Awake() {

            _selectableCell.selectionDidChangeEvent += HandleSelectionDidChange;
            _selectableCell.highlightDidChangeEvent += HandleHighlightDidChange;
        }

        protected void Start() {

            RefreshVisuals();
        }

        protected void OnDestroy() {

            _selectableCell.selectionDidChangeEvent -= HandleSelectionDidChange;
            _selectableCell.highlightDidChangeEvent -= HandleHighlightDidChange;
        }

        private void HandleSelectionDidChange(SelectableCell selectableCell, SelectableCell.TransitionType transitionType, object changeOwner) {

            RefreshVisuals();
        }

        private void HandleHighlightDidChange(SelectableCell selectableCell, SelectableCell.TransitionType transitionType) {

            RefreshVisuals();
        }

        private void RefreshVisuals() {

            if (!_selectableCell.selected && !_selectableCell.highlighted) {
                _normalAnimationClip.SampleAnimation(gameObject, time: 0.0f);
            }
            else if (!_selectableCell.highlighted) {
                _selectedAnimationClip.SampleAnimation(gameObject, time: 0.0f);
            }
            else if (!_selectableCell.selected) {
                _highlightedAnimationClip.SampleAnimation(gameObject, time: 0.0f);
            }
            else {
                _selectedAndHighlightedAnimationClip.SampleAnimation(gameObject, time: 0.0f);
            }
        }
    }
}
