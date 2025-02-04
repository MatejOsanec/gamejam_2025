using System;
using UnityEngine.UI;

namespace HMUI {

    public class ToggleWithCallbacks : Toggle {

        // Copy of Toggle.SelectionState because it's not public and we want to expose it in public event
        // DO NOT EDIT THIS unless you know what you are doing
        public new enum SelectionState {
            Normal,
            Highlighted,
            Pressed,
            Selected,
            Disabled,
        }

        public event Action<SelectionState> stateDidChangeEvent;

        public SelectionState selectionState {
            get {
                if (!IsInteractable()) {
                    return SelectionState.Disabled;
                }
                else if (IsPressed()) {
                    return SelectionState.Pressed;
                }
                else if (IsHighlighted()) {
                    return SelectionState.Highlighted;
                }

                return SelectionState.Normal;
            }
        }

        protected override void DoStateTransition(Toggle.SelectionState state, bool instant) {

            base.DoStateTransition(state, instant);
            stateDidChangeEvent?.Invoke((SelectionState)state);
        }
    }
}
