using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HMUI {

    public class SimpleTextDropdown : DropdownWithTableView, TableView.IDataSource {

        [SerializeField] TextMeshProUGUI _text = default;
        [SerializeField] SimpleTextTableCell _cellPrefab = default;
        [SerializeField] float _cellSize = 8.0f;

        private const string kCellReuseIdentifier = "Cell";

        private IReadOnlyList<string> _texts;
        private bool _initialized = false;

        private void LazyInit() {

            if (_initialized) {
                return;
            }

            _initialized = true;

            didSelectCellWithIdxEvent += HandleDidSelectCellWithIdx;

            base.Init(this);
        }

        protected override void OnDestroy() {

            base.OnDestroy();
            didSelectCellWithIdxEvent -= HandleDidSelectCellWithIdx;
        }

        public void SetTexts(IReadOnlyList<string> texts) {

            LazyInit();

            _texts = texts;
            _text.text = _texts.Count > selectedIndex ? _texts[selectedIndex] : "";
            ReloadData();
            SelectCellWithIdx(selectedIndex);
        }

        public override void SelectCellWithIdx(int idx) {

            base.SelectCellWithIdx(idx);

            if (_texts == null || _texts != null && (_texts.Count == 0 || _texts.Count <= idx)) {
                _text.text = "";
            }
            else {
                _text.text = _texts[idx];
            }
        }

        public float CellSize(int idx) => _cellSize;

        public int NumberOfCells() => _texts?.Count ?? 0;

        public TableCell CellForIdx(TableView tableView, int idx) {

            var cell = tableView.DequeueReusableCellForIdentifier(kCellReuseIdentifier) as SimpleTextTableCell;
            if (cell == null) {
                cell = Object.Instantiate(_cellPrefab);
                cell.reuseIdentifier = kCellReuseIdentifier;
            }

            cell.text = _texts[idx];

            return cell;
        }

        private void HandleDidSelectCellWithIdx(DropdownWithTableView dropdownWithTableView, int idx) {

            if (_texts == null || _texts.Count == 0) {
                return;
            }

            _text.text = _texts[idx];
        }
    }
}
