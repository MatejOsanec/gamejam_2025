using UnityEngine;

namespace HMUI {

    [RequireComponent(typeof(RectTransform))]
    public class TableCell : SelectableCell {

        /// <summary> Identifier for a group of table cells of the same type. EG normal content will have the same identifier and a sub-header might have a different identifier. </summary>
        public string reuseIdentifier { get => _reuseIdentifier; set => _reuseIdentifier = value; }

        public int idx { get; private set; }

        protected ITableCellOwner tableCellOwner => _tableCellOwner;

        private string _reuseIdentifier;
        private ITableCellOwner _tableCellOwner;

        public virtual void TableViewSetup(ITableCellOwner tableCellOwner, int idx) {

            _tableCellOwner = tableCellOwner;
            this.idx = idx;
        }

        public void MoveIdx(int offset) {

            idx += offset;
        }

        protected override void InternalToggle() {

            if (_tableCellOwner.selectionType == TableViewSelectionType.None) {
                return;
            }

            // We can deselect only if table view supports multiple selection.
            if (selected && (_tableCellOwner.selectionType == TableViewSelectionType.Multiple || _tableCellOwner.selectionType == TableViewSelectionType.DeselectableSingle)) {
                SetSelected(!selected, TransitionType.Animated, changeOwner: this, ignoreCurrentValue: false);
            }
            else if (!selected || _tableCellOwner.canSelectSelectedCell) {
                SetSelected(true, TransitionType.Animated, changeOwner: this, ignoreCurrentValue: true);
            }
        }

        public void __WasPreparedForReuse() {
            WasPreparedForReuse();
        }

        protected virtual void WasPreparedForReuse() { }
    }
}
