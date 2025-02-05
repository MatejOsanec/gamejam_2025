using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace HMUI {

    public class TextSegmentedControl : SegmentedControl, SegmentedControl.IDataSource {

        [SerializeField] float _fontSize = 4.0f;
        [SerializeField] bool _overrideCellSize = false;
        [DrawIf("_overrideCellSize", true)]
        [SerializeField] bool _fixedCellSize = false;
        [DrawIf("_fixedCellSize", true)]
        [SerializeField] float _fixedCellSizeAmount = 0.0f;
        [DrawIf("_overrideCellSize", true)]
        [SerializeField] float _padding = 4.0f;
        [SerializeField] bool _hideCellBackground = false;
        [SerializeField] bool _enableWordWrapping = true;
        [SerializeField] TextOverflowModes _textOverflowMode = TextOverflowModes.Overflow;

        [Space]
        [SerializeField] TextSegmentedControlCell _firstCellPrefab = default;
        [SerializeField] TextSegmentedControlCell _lastCellPrefab = default;
        [SerializeField] TextSegmentedControlCell _singleCellPrefab = default;
        [SerializeField] TextSegmentedControlCell _middleCellPrefab = default;

        private IReadOnlyList<string> _texts;
        private HashSet<int> _disabledIndexes;

        public void SetTexts(IReadOnlyList<string> texts, HashSet<int> disabledIndexes = null) {

            _texts = texts;
            _disabledIndexes = disabledIndexes;
            if (dataSource == null) {
                dataSource = this;
            }
            else {
                ReloadData();
            }
        }

        public int NumberOfCells() {

            if (_texts == null) {
                return 0;
            }

            return _texts.Count;
        }

        public SegmentedControlCell CellForCellNumber(int cellNumber) {

            TextSegmentedControlCell cell = null;

            if (_texts.Count == 1) {
                cell = GetReusableCell<TextSegmentedControlCell>(_singleCellPrefab);
            }
            else if (cellNumber == 0) {
                cell = GetReusableCell<TextSegmentedControlCell>(_firstCellPrefab);
            }
            else if (cellNumber == _texts.Count - 1) {
                cell = GetReusableCell<TextSegmentedControlCell>(_lastCellPrefab);
            }
            else {
                cell = GetReusableCell<TextSegmentedControlCell>(_middleCellPrefab);
            }

            cell.fontSize = _fontSize;
            cell.text = _texts[cellNumber];
            cell.hideBackgroundImage = _hideCellBackground;
            cell.enableWordWrapping = _enableWordWrapping;
            cell.textOverflowMode = _textOverflowMode;

            if (_overrideCellSize) {
                var rectTransform = (RectTransform)cell.transform;
                if (_fixedCellSize) {
                    rectTransform.sizeDelta = new Vector2(_fixedCellSizeAmount + 2 * _padding, 1.0f);
                }
                else {
                    rectTransform.sizeDelta = new Vector2(cell.preferredWidth + 2 * _padding, 1.0f);
                }
            }

            var isDisabled = _disabledIndexes?.Contains(cellNumber) ?? false;
            cell.interactable = !isDisabled;

            return cell;
        }
    }
}
