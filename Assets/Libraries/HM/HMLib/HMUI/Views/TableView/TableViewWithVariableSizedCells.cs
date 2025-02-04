using UnityEngine;
using UnityEngine.Assertions;

namespace HMUI {

    [RequireComponent(typeof(ScrollView))]
    public class TableViewWithVariableSizedCells : TableView {

        private float _totalHeight;
        private float[] _cachedCellSizes;
        private float[] _cachedCumulativeCellSizes;

        protected override float contentSize => _totalHeight;

        protected override int GetMinVisibleIdx() {

            float pos = _tableType == TableType.Vertical ? scrollView.position : -scrollView.position;
            float cumulativeSize = paddingStart;
            // Edge case - padding bigger than viewport
            if (cumulativeSize > pos) {
                return -1;
            }
            for (int i = 0; i < numberOfCells; i++) {
                var currentCellSize = GetCellSize(i);
                cumulativeSize += currentCellSize;
                if (cumulativeSize > pos + 0.001f * currentCellSize) {
                    return i;
                }
                cumulativeSize += _spacing;
            }
            return numberOfCells - 1;
        }

        protected override int GetMaxVisibleIdx() {

            float pos = _tableType == TableType.Vertical ? scrollView.position : -scrollView.position;
            var rect = viewportTransform.rect;
            float scrollRectSize = _tableType == TableType.Vertical ? rect.height : rect.width;
            float cumulativeSize = paddingStart;
            // Edge case - padding bigger than viewport
            if (cumulativeSize > pos) {
                return -1;
            }
            for (int i = 0; i < numberOfCells; i++) {
                var currentCellSize = GetCellSize(i);
                cumulativeSize += currentCellSize;
                cumulativeSize += _spacing;
                if (cumulativeSize > pos + scrollRectSize - 0.001f * currentCellSize) {
                    return i;
                }
            }
            return numberOfCells - 1;
        }

        protected override float GetCellSize(int idx) {

            Assert.IsTrue(idx < _cachedCellSizes.Length, "Trying to get cell size of a cell that is not in the cache");
            return _cachedCellSizes[idx];
        }

        protected override float GetCellPosition(int idx) {

            Assert.IsTrue(idx < _cachedCumulativeCellSizes.Length, "Trying to get cell placement position of a cell that is not in the cache");
            if (idx == 0) {
                return 0.0f;
            }
            float cumulativePosition = _cachedCumulativeCellSizes[idx - 1];
            return cumulativePosition + idx * _spacing;
        }

        protected override void UpdateCachedData() {

            base.UpdateCachedData();
            _cachedCellSizes = new float[numberOfCells];
            _cachedCumulativeCellSizes = new float[numberOfCells];
            float cumulativeSize = paddingStart;
            for (int i = 0; i < _dataSource.NumberOfCells(); i++) {
                float currentCellSize = _dataSource.CellSize(i);
                _cachedCellSizes[i] = currentCellSize;
                cumulativeSize += currentCellSize;
                _cachedCumulativeCellSizes[i] = cumulativeSize;
            }
            _totalHeight = cumulativeSize + _spacing * Mathf.Clamp(numberOfCells - 1, 0, int.MaxValue) + paddingEnd;
        }
    }
}
