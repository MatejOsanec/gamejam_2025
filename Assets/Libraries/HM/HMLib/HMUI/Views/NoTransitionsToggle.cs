using System;
using UnityEngine.UI;

namespace HMUI {

    public class NoTransitionsToggle : Toggle {

        public UISelectionState selectionState => _selectionState;

        public event Action<UISelectionState> selectionStateDidChangeEvent;

        private UISelectionState _selectionState;

        protected override void Start() {

            base.Start();
            // Workaround for this not doing DoStateTransition when isOn is changed via code
            onValueChanged.AddListener(isOn => DoStateTransition(currentSelectionState, instant: true));
        }

#if UNITY_EDITOR

        protected override void OnValidate() {

            base.OnValidate();
            transition = Transition.None;
        }
#endif

        protected override void DoStateTransition(SelectionState state, bool instant) {

            var thisState = UISelectionState.Normal;
            switch (state) {
                case SelectionState.Normal:
                    thisState = isOn ? UISelectionState.Selected : UISelectionState.Normal;
                    break;
                case SelectionState.Highlighted:
#if BS_TOURS
                    thisState = isOn ? UISelectionState.SelectedAndHighlighted : UISelectionState.Highlighted;
#else
                    thisState = UISelectionState.Highlighted;
#endif
                    break;
                case SelectionState.Pressed:
                    thisState = UISelectionState.Pressed;
                    break;
                case SelectionState.Selected:
                    thisState = UISelectionState.Selected;
                    break;
                case SelectionState.Disabled:
                    thisState = UISelectionState.Disabled;
                    break;
            }

            if (_selectionState == thisState) {
                return;
            }

            SetSelectionState(thisState);
        }

        protected void SetSelectionState(UISelectionState state) {

            _selectionState = state;
            selectionStateDidChangeEvent?.Invoke(state);
        }
    }
}
