using UnityEngine;

namespace HMUI {

    public class IconAndTextSegmentedControl : SegmentedControl, SegmentedControl.IDataSource {

        [Space]
        [SerializeField] IconAndTextSegmentedControlCell _firstCellPrefab;
        [SerializeField] IconAndTextSegmentedControlCell _lastCellPrefab;
        [SerializeField] IconAndTextSegmentedControlCell _singleCellPrefab;
        [SerializeField] IconAndTextSegmentedControlCell _middleCellPrefab;

        public class DataItem {

            public Sprite icon { get; private set; }
            public string text { get; private set; }
            public bool interactable { get; private set; }

            public DataItem(Sprite icon, string text, bool interactable = true) {

                this.icon = icon;
                this.text = text;
                this.interactable = interactable;
            }
        }

        private DataItem[] _dataItems;
        private bool _isInitialized;

        protected void Init() {

            if (_isInitialized) {
                return;
            }
            _isInitialized = true;

            dataSource = this;
        }

        public void SetData(DataItem[] dataItems) {

            Init();

            _dataItems = dataItems;
            ReloadData();
        }

        public int NumberOfCells() {

            if (_dataItems == null) {
                return 0;
            }

            return _dataItems.Length;
        }

        public SegmentedControlCell CellForCellNumber(int cellNumber) {

            IconAndTextSegmentedControlCell cell = null;

            if (_dataItems.Length == 1) {
                cell = GetReusableCell<IconAndTextSegmentedControlCell>(_singleCellPrefab);
            }
            else if (cellNumber == 0) {
                cell = GetReusableCell<IconAndTextSegmentedControlCell>(_firstCellPrefab);
            }
            else if (cellNumber == _dataItems.Length - 1) {
                cell = GetReusableCell<IconAndTextSegmentedControlCell>(_lastCellPrefab);
            }
            else {
                cell = GetReusableCell<IconAndTextSegmentedControlCell>(_middleCellPrefab);
            }

            cell.sprite = _dataItems[cellNumber].icon;
            cell.text = _dataItems[cellNumber].text;
            cell.interactable = _dataItems[cellNumber].interactable;
            return cell;
        }

        public void SetTextsActive(bool active) {

            foreach (var cell in cells) {
                ((IconAndTextSegmentedControlCell)cell).SetTextActive(active);
            }
        }
    }
}
