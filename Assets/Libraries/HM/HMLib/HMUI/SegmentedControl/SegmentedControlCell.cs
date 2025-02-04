using UnityEngine;

namespace HMUI {

    [RequireComponent(typeof(RectTransform))]
    public class SegmentedControlCell : SelectableCell {

        public int cellNumber { get; private set; }
        private SegmentedControl _segmentedControl;

        public void SegmentedControlSetup(SegmentedControl segmentedControl, int cellNumber) {

            _segmentedControl = segmentedControl;
            this.cellNumber = cellNumber;
        }

        protected override void InternalToggle() {
            
            if (!selected) {
                SetSelected(value: true, TransitionType.Animated, changeOwner: this, ignoreCurrentValue: false);
            }
        }
    }
}
