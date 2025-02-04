using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace HMUI {

    [RequireComponent(typeof(ScrollView))]
    public class TableView : MonoBehaviour, ITableCellOwner {

        [SerializeField] ScrollView _scrollView = default;

        [Space]
        [SerializeField] bool _scrollToTopOnEnable = default;
        [SerializeField] bool _alignToCenter = default;
        [SerializeField] protected float _spacing = 0.0f;
        [SerializeField] protected FloatRectOffset _padding;
        [SerializeField] protected TableType _tableType = TableType.Vertical;

        [Space]
        [SerializeField] TableViewSelectionType _selectionType = TableViewSelectionType.Single;
        [SerializeField] bool _canSelectSelectedCell = false;
        [SerializeField] bool _spawnCellsThatAreNotVisible = false;

        [Space]
        [SerializeField] [NullAllowed] CellsGroup[] _preallocatedCells = default;

        public enum TableType {

            Vertical,
            Horizontal
        }

        public enum ScrollPositionType {

            Beginning,
            Center,
            End
        }

        [System.Serializable]
        public class CellsGroup {

            [SerializeField] string _reuseIdentifier = default;
            [SerializeField] List<TableCell> _cells = default;
            public string reuseIdentifier => _reuseIdentifier;
            public List<TableCell> cells => _cells;
        }

        /// <summary> Interface the data container for the table view must inherit from. </summary>
        public interface IDataSource {

            float CellSize(int idx);
            int NumberOfCells();
            TableCell CellForIdx(TableView tableView, int idx);
        }

        public TableViewSelectionType selectionType { get => _selectionType; set => _selectionType = value; }
        public bool canSelectSelectedCell { get => _canSelectSelectedCell; }

        public event Action<TableView, int> didSelectCellWithIdxEvent;
        public event Action<TableView, int> didDeselectCellWithIdxEvent;
        public event Action<TableView> didReloadDataEvent;
        public event Action<TableView> didInsertCellsEvent;
        public event Action<TableView> didDeleteCellsEvent;
        public event Action<TableView> didChangeRectSizeEvent;

        public IDataSource dataSource => _dataSource;

        public IEnumerable<TableCell> visibleCells => _visibleCells;
        public RectTransform viewportTransform => _viewportTransform;
        public RectTransform contentTransform => _contentTransform;
        public int numberOfCells => _numberOfCells;
        public float cellSize => _cellSize;
        public float spacing => _spacing;
        public TableType tableType => _tableType;
        public ScrollView scrollView => _scrollView;

        public const int kFixedCellSizeIndex = -1;

        protected float totalPadding => (tableType == TableType.Horizontal ? _padding.left + _padding.right : _padding.top + _padding.bottom);
        protected float paddingStart => (tableType == TableType.Horizontal ? _padding.left : _padding.top);
        protected float paddingEnd => (tableType == TableType.Horizontal ? _padding.right : _padding.bottom);
        protected virtual float contentSize => _numberOfCells * _cellSize + _spacing * (_numberOfCells - 1) + totalPadding;

        private RectTransform _contentTransform;
        private RectTransform _viewportTransform;
        protected IDataSource _dataSource;

        private int _numberOfCells;
        private float _cellSize;

        private readonly List<TableCell> _visibleCells = new List<TableCell>();
        private Dictionary<string, List<TableCell>> _reusableCells;
        private HashSet<int> _selectedCellIdxs = new HashSet<int>();

        private int _prevMinIdx = -1;
        private int _prevMaxIdx = -1;
        private bool _isInitialized;
        private bool _refreshCellsOnEnable;

        protected void Awake() {

            if (!_isInitialized) {
                LazyInit();
            }
        }

        protected void OnDestroy() {

            _scrollView.scrollPositionChangedEvent -= HandleScrollRectValueChanged;
        }

        protected void OnEnable() {

            if (_refreshCellsOnEnable) {
                RefreshCells(forcedVisualsRefresh: true, forcedContentRefresh: false);
                _refreshCellsOnEnable = false;
            }

            ClearHighlights();
            if (_scrollToTopOnEnable) {
                ScrollToCellWithIdx(0, ScrollPositionType.Beginning, animated: false);
            }
        }

        private void LazyInit() {

            if (_isInitialized) {
                return;
            }

            _isInitialized = true;

            // Populate already created cells.
            _reusableCells = new Dictionary<string, List<TableCell>>();
            foreach (var cellsGroup in _preallocatedCells) {
                _reusableCells[cellsGroup.reuseIdentifier] = cellsGroup.cells;
                foreach (var cell in cellsGroup.cells) {
                    cell.reuseIdentifier = cellsGroup.reuseIdentifier;
                }
            }

            _contentTransform = _scrollView.contentTransform;
            _viewportTransform = _scrollView.viewportTransform;

            _scrollView._scrollType = ScrollView.ScrollType.FixedCellSize;
            _scrollView.scrollPositionChangedEvent += HandleScrollRectValueChanged;

            // Set up content rect.
            if (_tableType == TableType.Vertical) {
                _contentTransform.anchorMin = new Vector2(0.0f, 1.0f);
                _contentTransform.anchorMax = new Vector2(1.0f, 1.0f);
                _contentTransform.pivot = new Vector2(0.5f, 1.0f);
            }
            else {
                _contentTransform.anchorMin = new Vector2(0.0f, 0.0f);
                _contentTransform.anchorMax = new Vector2(0.0f, 1.0f);
                _contentTransform.pivot = new Vector2(0.0f, 0.5f);
            }

            _contentTransform.sizeDelta = new Vector2(0.0f, 0.0f);
            _contentTransform.anchoredPosition = new Vector2(0.0f, 0.0f);
        }

        public void Hide() {

            gameObject.SetActive(false);
        }

        public void Show() {

            gameObject.SetActive(true);
        }

        private void RefreshContentSize() {

            if (_tableType == TableType.Vertical) {
                _contentTransform.sizeDelta = new Vector2(0.0f, contentSize);
            }
            else {
                _contentTransform.sizeDelta = new Vector2(contentSize, 0.0f);
            }

            _scrollView.UpdateContentSize();
        }

        public void RefreshCellsContent() {

            RefreshCells(forcedVisualsRefresh: false, forcedContentRefresh: true);
        }

        /// <summary> Attempts to find the cell within visible range </summary>
        /// <returns> null if cell is not visible </returns>
        public TableCell GetCellAtIndex(int index) {

            (int minIdx, int maxIdx) = GetVisibleCellsIdRange();
            if (index > maxIdx || index < minIdx) {
                return null;
            }
            return _visibleCells[index - minIdx];
        }

        protected Tuple<int, int> GetVisibleCellsIdRange() {

            if (_spawnCellsThatAreNotVisible) {
                return new Tuple<int, int>(0, _numberOfCells - 1);
            }
            return new Tuple<int, int>(GetMinVisibleIdx(), GetMaxVisibleIdx());
        }

        protected virtual int GetMinVisibleIdx() {

            float pos = _tableType == TableType.Vertical ? _scrollView.position : -_scrollView.position;
            // Spacing added to position since when we are in the space after a cell, it is not visible anymore
            int minIdx = Mathf.FloorToInt((pos + _spacing - paddingStart) / (_cellSize + _spacing) + _cellSize * 0.001f); // 0.001 is offset which helps to hide cells which are barely in range.
            if (minIdx < 0) {
                minIdx = 0;
            }
            return minIdx;
        }

        protected virtual int GetMaxVisibleIdx() {

            float pos = _tableType == TableType.Vertical ? _scrollView.position : -_scrollView.position;
            var rect = _viewportTransform.rect;
            float scrollRectSize = _tableType == TableType.Vertical ? rect.height : rect.width;
            int maxIdx = Mathf.FloorToInt((pos + scrollRectSize - _cellSize * 0.001f - paddingStart) / (_cellSize + _spacing));
            if (maxIdx > _numberOfCells - 1) {
                maxIdx = _numberOfCells - 1;
            }
            return maxIdx;
        }

        protected virtual float GetCellSize(int idx) {

            return _cellSize;
        }

        protected virtual float GetCellPosition(int idx) {

            return idx * (_cellSize + _spacing) + paddingStart;
        }

        private void RefreshCells(bool forcedVisualsRefresh, bool forcedContentRefresh) {

            LazyInit();

            // Compute current min and max idxs.
            (int minIdx, int maxIdx) = GetVisibleCellsIdRange();

            // Nothing has changed.
            if (minIdx == _prevMinIdx && maxIdx == _prevMaxIdx && !forcedVisualsRefresh && !forcedContentRefresh) {
                return;
            }

            // Store invisible cells into the cache.
            for (int i = _visibleCells.Count - 1; i >= 0; i--) {
                var cell = _visibleCells[i];
                if (cell.idx < minIdx || cell.idx > maxIdx || forcedContentRefresh) {
                    cell.gameObject.SetActive(false);
                    _visibleCells.RemoveAt(i);
                    AddCellToReusableCells(cell);
                }
            }

            var rect = _viewportTransform.rect;
            float scrollRectSize = _tableType == TableType.Vertical ? rect.height : rect.width;
            float centerAlignOffset = 0.0f;
            if (_alignToCenter && _scrollView.scrollableSize == 0.0f) {
                centerAlignOffset = (scrollRectSize - contentSize) * 0.5f;
            }

            // Get new cells from data source.
            for (int idx = minIdx; idx <= maxIdx; idx++) {

                // Do we need a cell for this idx?
                TableCell cell = null;
                for (int i = 0; i < _visibleCells.Count; i++) {
                    if (_visibleCells[i].idx == idx) {
                        cell = _visibleCells[i];
                        break;
                    }
                }
                if (cell != null && !forcedVisualsRefresh && !forcedContentRefresh) {
                    continue;
                }

                // New cell.
                bool newCell = false;
                if (cell == null) {
                    newCell = true;
                    cell = _dataSource.CellForIdx(tableView: this, idx);
                    _visibleCells.Add(cell);
                }
                cell.gameObject.SetActive(true);
                cell.TableViewSetup(this, idx);
                cell.selectionDidChangeEvent -= HandleCellSelectionDidChange;
                cell.selectionDidChangeEvent += HandleCellSelectionDidChange;
                if (newCell) {
                    cell.ClearHighlight(SelectableCell.TransitionType.Instant);
                }
                cell.SetSelected(_selectedCellIdxs.Contains(idx), SelectableCell.TransitionType.Instant, changeOwner: this, ignoreCurrentValue: newCell);
                if (cell.transform.parent != _contentTransform) {
                    cell.transform.SetParent(_contentTransform, false);
                }
                LayoutCellForIdx(cell, idx, centerAlignOffset);

                // All cells are there.
                if (_visibleCells.Count == maxIdx - minIdx + 1 && !forcedVisualsRefresh) {
                    break;
                }
            }

            _prevMinIdx = minIdx;
            _prevMaxIdx = maxIdx;
        }

        private void LayoutCellForIdx(TableCell cell, int idx, float offset) {

            var rectTransform = (RectTransform)cell.transform;

            if (_tableType == TableType.Vertical) {
                rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
                rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
                rectTransform.pivot = new Vector2(0.5f, 1.0f);
                rectTransform.sizeDelta = new Vector2(0.0f, GetCellSize(idx));
                rectTransform.anchoredPosition = new Vector2(0.0f, -(GetCellPosition(idx) + offset));
                rectTransform.offsetMin = new Vector2(_padding.left, rectTransform.offsetMin.y);
                rectTransform.offsetMax = new Vector2(-_padding.right, rectTransform.offsetMax.y);
            }
            else {
                rectTransform.anchorMax = new Vector2(0.0f, 1.0f);
                rectTransform.anchorMin = new Vector2(0.0f, 0.0f);
                rectTransform.pivot = new Vector2(0.0f, 0.5f);
                rectTransform.sizeDelta = new Vector2(GetCellSize(idx), 0.0f);
                rectTransform.anchoredPosition = new Vector2(GetCellPosition(idx) + offset, 0.0f);
                rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, -_padding.top);
                rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, _padding.bottom);
            }
        }

        private void AddCellToReusableCells(TableCell cell) {

            if (!_reusableCells.TryGetValue(cell.reuseIdentifier, out var reusableCellsForIdentifier)) {
                reusableCellsForIdentifier = new List<TableCell>();
                _reusableCells.Add(cell.reuseIdentifier, reusableCellsForIdentifier);
                cell.__WasPreparedForReuse();
            }
            reusableCellsForIdentifier.Add(cell);
        }

        private void HandleScrollRectValueChanged(float f) {

            RefreshCells(forcedVisualsRefresh: false, forcedContentRefresh: false);
        }

        private void HandleCellSelectionDidChange(SelectableCell selectableCell, SelectableCell.TransitionType transitionType, object changeOwner) {

            // We initiated the change, so we ignore it
            if (ReferenceEquals(this, changeOwner)) {
                return;
            }

            if (selectionType == TableViewSelectionType.None) {
                return;
            }

            var tableCell = (TableCell)selectableCell;

            if (selectionType != TableViewSelectionType.Multiple) {
                // Deselect all other visible cells.
                foreach (var cell in _visibleCells) {
                    if (tableCell == cell) {
                        continue;
                    }
                    cell.SetSelected(false, SelectableCell.TransitionType.Instant, changeOwner: this, ignoreCurrentValue: false);
                }
            }

            if (tableCell.selected) {
                if (selectionType != TableViewSelectionType.Multiple) {
                    _selectedCellIdxs.Clear();
                }
                _selectedCellIdxs.Add(tableCell.idx);
                DidSelectCellWithIdx(tableCell.idx);
            }
            else {
                _selectedCellIdxs.Remove(tableCell.idx);
                didDeselectCellWithIdxEvent?.Invoke(this, tableCell.idx);
            }
        }

        protected virtual void DidSelectCellWithIdx(int idx) {

            didSelectCellWithIdxEvent?.Invoke(this, idx);
        }

        protected virtual void UpdateCachedData() {

            _numberOfCells = _dataSource.NumberOfCells();
        }

        public void ReloadDataKeepingPosition() {

            var indexToScrollTo = GetMinVisibleIdx();
            ReloadData();
            ScrollToCellWithIdx(indexToScrollTo, ScrollPositionType.Beginning, animated: false);
        }

        public virtual void SetDataSource(IDataSource newDataSource, bool reloadData) {

            _dataSource = newDataSource;
            if (reloadData) {
                ReloadData();
            }
        }

        public virtual void ReloadData() {

            if (!_isInitialized) {
                LazyInit();
            }

            // Recycle visible cells.
            foreach (var cell in _visibleCells) {
                cell.gameObject.SetActive(false);
                AddCellToReusableCells(cell);
            }

            _visibleCells.Clear();

            if (_dataSource != null) {
                UpdateCachedData();
                _cellSize = _dataSource.CellSize(kFixedCellSizeIndex);
            }
            else {
                _numberOfCells = 0;
                _cellSize = 1;
            }

            _scrollView._fixedCellSize = _cellSize + _spacing;

            RefreshContentSize();

            if (!gameObject.activeInHierarchy) {
                _refreshCellsOnEnable = true;
            }
            else {
                RefreshCells(forcedVisualsRefresh: true, forcedContentRefresh: false);
            }

            didReloadDataEvent?.Invoke(this);
        }

        public void InsertCells(int idx, int count) {

            // Adjust cell idx.
            foreach (var cell in _visibleCells) {
                if (cell.idx >= idx) {
                    cell.MoveIdx(count);
                }
            }

            // Adjust selected cells.
            var newSelectedIdxs = new HashSet<int>();
            foreach (var selectedCellIdx in _selectedCellIdxs) {
                if (selectedCellIdx >= idx) {
                    newSelectedIdxs.Add(selectedCellIdx + count);
                }
                else {
                    newSelectedIdxs.Add(selectedCellIdx);
                }
            }
            _selectedCellIdxs = newSelectedIdxs;

            var prevNumberOfCells = _numberOfCells;
            UpdateCachedData();

            Assert.IsTrue(_numberOfCells == prevNumberOfCells + count, "Number of cells after insert don't match number of cells returned from data source.");

            RefreshContentSize();
            RefreshCells(forcedVisualsRefresh: true, forcedContentRefresh: false);

            didInsertCellsEvent?.Invoke(this);
        }

        public void DeleteCells(int idx, int count) {

            // Adjust cell idx.
            for (int i = _visibleCells.Count - 1; i >= 0; i--) {
                var cell = _visibleCells[i];
                if (cell.idx >= idx && cell.idx < idx + count) {
                    cell.gameObject.SetActive(false);
                    _visibleCells.RemoveAt(i);
                    AddCellToReusableCells(cell);
                }
                else if (cell.idx >= idx + count) {
                    cell.MoveIdx(-count);
                }
            }

            // Adjust selected cells.
            var newSelectedCellIdxs = new HashSet<int>();
            foreach (var selectedCellIdx in _selectedCellIdxs) {
                if (selectedCellIdx >= idx + count) {
                    newSelectedCellIdxs.Add(selectedCellIdx - count);
                }
                else if (selectedCellIdx < idx) {
                    newSelectedCellIdxs.Add(selectedCellIdx);
                }
            }
            _selectedCellIdxs = newSelectedCellIdxs;

            var prevNumberOfCells = _numberOfCells;
            UpdateCachedData();

            Assert.IsTrue(_numberOfCells == prevNumberOfCells - count, "Number of cells after insert don't match number of cells returned from data source.");

            RefreshContentSize();
            RefreshCells(forcedVisualsRefresh: true, forcedContentRefresh: false);

            didDeleteCellsEvent?.Invoke(this);
        }

        public TableCell DequeueReusableCellForIdentifier(string identifier) {

            TableCell cell = null;
            if (_reusableCells.TryGetValue(identifier, out var reusableCellsForIdentifier)) {
                if (reusableCellsForIdentifier.Count > 0) {
                    cell = reusableCellsForIdentifier[0];
                    reusableCellsForIdentifier.RemoveAt(0);
                }
            }

            return cell;
        }

        public void SelectCellWithIdx(int idx, bool callbackTable = false) {

            foreach (var cell in _visibleCells) {
                cell.SetSelected(false, SelectableCell.TransitionType.Instant, changeOwner: this, ignoreCurrentValue: false);
            }

            _selectedCellIdxs.Clear();
            _selectedCellIdxs.Add(idx);
            RefreshCells(forcedVisualsRefresh: true, forcedContentRefresh: false);

            if (callbackTable) {
                DidSelectCellWithIdx(idx);
            }
        }

        public void ClearSelection() {

            foreach (var cell in _visibleCells) {
                cell.SetSelected(false, SelectableCell.TransitionType.Instant, changeOwner: this, ignoreCurrentValue: false);
                cell.ClearHighlight(SelectableCell.TransitionType.Instant);
            }

            _selectedCellIdxs.Clear();
            RefreshCells(forcedVisualsRefresh: true, forcedContentRefresh: false);
        }

        public void ClearHighlights() {

            foreach (var cell in _visibleCells) {
                cell.ClearHighlight(SelectableCell.TransitionType.Instant);
            }
        }

        public void ScrollToPosition(float position, bool animated) {

            _scrollView.ScrollTo(position, animated);

            if (!animated) {
                RefreshCells(forcedVisualsRefresh: true, forcedContentRefresh: false);
            }
        }

        public void ScrollToCellWithIdx(int idx, ScrollPositionType scrollPositionType, bool animated) {

            var visibleCellsIds = GetVisibleCellsIdRange();
            var numberOfCellsOnScreen = visibleCellsIds.Item2 - visibleCellsIds.Item1;

            if (scrollPositionType == ScrollPositionType.Center) {
                idx -= numberOfCellsOnScreen / 2;
            }
            else if (scrollPositionType == ScrollPositionType.End) {
                if (idx >= numberOfCellsOnScreen - 1) {
                    idx = numberOfCellsOnScreen - 1;
                }
            }

            if (idx < 0) {
                idx = 0;
            }

            var cellPosition = GetCellPosition(idx);
            // If there is start padding, we want to scroll to the top, not to the actual start of the 0 index cell
            if (idx == 0) {
                cellPosition -= paddingStart;
            }
            ScrollToPosition(cellPosition, animated);
        }

        public void ChangeRectSize(RectTransform.Axis axis, float size) {

            var rectTransform = (RectTransform)transform;
            rectTransform.SetSizeWithCurrentAnchors(axis, size);
            didChangeRectSizeEvent?.Invoke(this);
        }
    }
}
