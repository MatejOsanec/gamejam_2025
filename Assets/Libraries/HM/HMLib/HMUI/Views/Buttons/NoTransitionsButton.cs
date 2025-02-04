using UnityEngine.UI;

namespace HMUI {

    public class NoTransitionsButton : UnityEngine.UI.Button {

        public SelectionState selectionState => _selectionState;

        public new enum SelectionState {

            Normal = 0,
            Highlighted = 1,
            Pressed = 2,
            Disabled = 3
        }

        public event System.Action<SelectionState> selectionStateDidChangeEvent;

        private SelectionState _selectionState;

#if UNITY_EDITOR

        protected override void OnValidate() {

            base.OnValidate();
            transition = Selectable.Transition.None;
        }
#endif

        protected override void DoStateTransition(Selectable.SelectionState state, bool instant) {

            SelectionState thisState = SelectionState.Normal;
            if (state == Selectable.SelectionState.Normal) {
                thisState = SelectionState.Normal;
            }
            else if (state == Selectable.SelectionState.Highlighted) {
                thisState = SelectionState.Highlighted;
            }
            else if (state == Selectable.SelectionState.Pressed) {
                thisState = SelectionState.Pressed;
            }
            else if (state == Selectable.SelectionState.Disabled) {
                thisState = SelectionState.Disabled;
            }

            _selectionState = thisState;
            selectionStateDidChangeEvent?.Invoke(thisState);
        }
    }
}
