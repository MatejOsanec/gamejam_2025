using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HMUI {

    public class IconSegmentedControl : SegmentedControl, SegmentedControl.IDataSource {

        [SerializeField] float _iconSize = 4.0f;
        [SerializeField] bool _overrideCellSize = default;
        [DrawIf("_overrideCellSize", true)]
        [SerializeField] float _padding = 4.0f;
        [SerializeField] bool _hideCellBackground = default;

        [Space]
        [SerializeField] IconSegmentedControlCell _firstCellPrefab = default;
        [SerializeField] IconSegmentedControlCell _lastCellPrefab = default;
        [SerializeField] IconSegmentedControlCell _middleCellPrefab = default;
        [SerializeField] IconSegmentedControlCell _singleCellPrefab = default;

        public class DataItem {

            public Sprite icon { get; private set; }
            public string hintText { get; private set; }
            public bool interactable { get; private set; }

            public DataItem(Sprite icon, string hintText, bool interactable = true) {

                this.icon = icon;
                this.hintText = hintText;
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

            IconSegmentedControlCell cell = null;

            if (_dataItems.Length == 1) {
                cell = GetReusableCell<IconSegmentedControlCell>(_singleCellPrefab);
            }
            else if (cellNumber == 0) {
                cell = GetReusableCell<IconSegmentedControlCell>(_firstCellPrefab);
            }
            else if (cellNumber == _dataItems.Length - 1) {
                cell = GetReusableCell<IconSegmentedControlCell>(_lastCellPrefab);
            }
            else {
                cell = GetReusableCell<IconSegmentedControlCell>(_middleCellPrefab);
            }

            cell.sprite = _dataItems[cellNumber].icon;
            cell.hintText = _dataItems[cellNumber].hintText;
            cell.hideBackgroundImage = _hideCellBackground;
            cell.interactable = _dataItems[cellNumber].interactable;

            if (_overrideCellSize) {
                var rectTransform = (RectTransform)cell.transform;
                cell.iconSize = _iconSize;
                rectTransform.sizeDelta = new Vector2(_iconSize + 2 * _padding, _iconSize + 2 * _padding);
            }

            return cell;
        }
    }
}
