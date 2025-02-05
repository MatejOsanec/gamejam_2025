using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;


namespace HMUI {

    public class SegmentedControl : MonoBehaviour {

        [SerializeField] Transform _separatorPrefab = default;

       readonly DiContainer _container = default;

        public event System.Action<SegmentedControl, int> didSelectCellEvent;
        public event System.Action<SegmentedControl, int> didPressNonInteractableCellEvent;

        public interface IDataSource {
            int NumberOfCells();
            SegmentedControlCell CellForCellNumber(int cellNumber);
        }

        public IDataSource dataSource {
            get => _dataSource;
            set {
                _dataSource = value;
                ReloadData();
            }
        }

        public int selectedCellNumber => _selectedCellNumber;
        public IReadOnlyList<SegmentedControlCell> cells => _cells;

        private int _numberOfCells;
        private readonly List<SegmentedControlCell> _cells = new List<SegmentedControlCell>();
        private readonly List<GameObject> _separators = new List<GameObject>();
        private IDataSource _dataSource;
        private int _selectedCellNumber = -1;

        private readonly Dictionary<int, System.Action<int>> _callbacks = new Dictionary<int, System.Action<int>>();
        private readonly Dictionary<Object, Queue<SegmentedControlCell>> _reusableCellPools = new();
        private readonly Dictionary<SegmentedControlCell, Object> _cellToPrefabMap = new();

        private void CreateCells() {

            Assert.IsTrue(_cells.Count == 0, "Creating new cells in segmented control when some cells were created already.");

            var thisTransform = transform;

            for (int cellNumber = 0; cellNumber < _numberOfCells; cellNumber++) {

                // New cell.
                var cell = _dataSource.CellForCellNumber(cellNumber);
                _cells.Add(cell);

                cell.gameObject.SetActive(true);
                cell.SegmentedControlSetup(this, cellNumber);
                cell.selectionDidChangeEvent -= HandleCellSelectionDidChange;
                cell.selectionDidChangeEvent += HandleCellSelectionDidChange;
                cell.nonInteractableCellWasPressedEvent -= HandleNonInteractableCellWasPressed;
                cell.nonInteractableCellWasPressedEvent += HandleNonInteractableCellWasPressed;
                cell.SetSelected(_selectedCellNumber == cellNumber, SelectableCell.TransitionType.Instant, changeOwner: this, ignoreCurrentValue: true);
                cell.ClearHighlight(SelectableCell.TransitionType.Instant);

                var cellTransform = cell.transform;

                if (cellTransform.parent != thisTransform) {
                    cellTransform.SetParent(thisTransform, false);
                }
                else {
                    cellTransform.SetAsLastSibling();
                }
                cellTransform.localPosition = Vector3.zero;
                cellTransform.localRotation = Quaternion.identity;

                // Separator
                if (cellNumber < _numberOfCells - 1 && _separatorPrefab != null) {
                    var separatorTransform = Instantiate(_separatorPrefab, thisTransform, worldPositionStays: false);
                    separatorTransform.localPosition = Vector3.zero;
                    separatorTransform.localRotation = Quaternion.identity;

                    _separators.Add(separatorTransform.gameObject);
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate(cellTransform as RectTransform);
            }
        }

        private void HandleCellSelectionDidChange(SelectableCell selectableCell, SelectableCell.TransitionType transitionType, object changeOwner) {

            if (ReferenceEquals(this, changeOwner)) {
                return;
            }

            var segmentedControlCell = (SegmentedControlCell)selectableCell;

            Assert.IsTrue(segmentedControlCell.selected); // This should be called only for selection.
            Assert.IsTrue(segmentedControlCell.cellNumber != _selectedCellNumber); // Can't select already selected cell.

            // Deselect old cell.
            _cells[_selectedCellNumber].SetSelected(false, SelectableCell.TransitionType.Instant, changeOwner: this, ignoreCurrentValue: false);
            _selectedCellNumber = segmentedControlCell.cellNumber;

            didSelectCellEvent?.Invoke(this, segmentedControlCell.cellNumber);

            if (_callbacks.TryGetValue(segmentedControlCell.cellNumber, out System.Action<int> callback)) {
                callback?.Invoke(segmentedControlCell.cellNumber);
            }
        }

        private void HandleNonInteractableCellWasPressed(SelectableCell selectableCell) {

            var segmentedControlCell = (SegmentedControlCell)selectableCell;
            var segmentedControlCellIndex = segmentedControlCell.cellNumber;
            didPressNonInteractableCellEvent?.Invoke(this, segmentedControlCellIndex);
        }

        public void SetCallbackForCell(int cellNumber, System.Action<int> callback) {

            _callbacks[cellNumber] = callback;
        }

        public void ReloadData() {

            // Delete old data
            foreach (var cell in _cells) {
                if (cell != null && cell.gameObject != null) {
                    if (_cellToPrefabMap.TryGetValue(cell, out var prefab)) {
                        cell.gameObject.SetActive(false);
                        _reusableCellPools[prefab].Enqueue(cell);
                    }
                    else {
                        Destroy(cell.gameObject);
                    }
                }
            }
            _cells.Clear();

            foreach (var separator in _separators) {
                Destroy(separator);
            }
            _separators.Clear();

            _numberOfCells = _dataSource.NumberOfCells();
            _selectedCellNumber = 0;

            CreateCells();
        }

        public void SelectCellWithNumber(int selectCellNumber) {

            _selectedCellNumber = selectCellNumber;
            for (int cellNumber = 0; cellNumber < _numberOfCells; cellNumber++) {
                _cells[cellNumber].SetSelected(cellNumber == selectCellNumber, SelectableCell.TransitionType.Instant, changeOwner: this, ignoreCurrentValue: false);
            }
        }

        public T GetReusableCell<T>(Object prefab) where T: SegmentedControlCell {

            if (!_reusableCellPools.TryGetValue(prefab, out var pool)) {
                pool = new Queue<SegmentedControlCell>();
                _reusableCellPools[prefab] = pool;
            }
            if (!pool.TryDequeue(out var cell)) {
                cell = _container.InstantiatePrefabForComponent<T>(prefab, Vector3.zero, Quaternion.identity, transform);
                _cellToPrefabMap[cell] = prefab;
            }

            cell.gameObject.SetActive(true);
            cell.transform.localScale = Vector3.one;
            return (T)cell;
        }
    }
}
