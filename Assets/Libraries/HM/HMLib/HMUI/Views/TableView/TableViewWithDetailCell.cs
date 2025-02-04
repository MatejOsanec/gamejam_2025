using System;
using System.Collections.Generic;
using HMUI;
using UnityEngine;

public class TableViewWithDetailCell : TableView, TableView.IDataSource {

    public event System.Action<TableViewWithDetailCell, int> didSelectContentCellEvent;
    public event System.Action<TableViewWithDetailCell, int> didDeselectContentCellEvent;

    public new interface IDataSource {

        float CellSize();
        int NumberOfCells();
        TableCell CellForContent(TableViewWithDetailCell tableView, int idx, bool detailOpened);
        TableCell CellForDetail(TableViewWithDetailCell tableView, int contentIdx);
    }

    public new IDataSource dataSource {
        get => _dataSource;
        set {

            if (_dataSource == value) {
                return;
            }

            _dataSource = value;
            base._dataSource = this;
            ReloadData();
        }
    }

    private new IDataSource _dataSource;

    private int _selectedId = -1;

    public float CellSize(int idx = 0) => _dataSource.CellSize();

    public int NumberOfCells() => _dataSource.NumberOfCells() + (_selectedId != -1 ? 1 : 0);

    public TableCell CellForIdx(TableView tableView, int idx) {

        if (_selectedId != -1 && idx == (_selectedId + 1)) {
            return _dataSource.CellForDetail(this, idx - 1);
        }

        var detailOpened = _selectedId == idx;

        return (_selectedId != -1 && idx > _selectedId) ?
            _dataSource.CellForContent(this, idx - 1, detailOpened) :
            _dataSource.CellForContent(this, idx, detailOpened);
    }

    public override void ReloadData() {

        ReloadData(currentNewIndex: -1);
    }

    public void ReloadData(int currentNewIndex) {

        _selectedId = currentNewIndex;

        if (_selectedId == -1) {
            ClearSelection();
        }
        else {
            SelectCellWithIdx(_selectedId);
        }

        base.ReloadData();
    }

    protected override void DidSelectCellWithIdx(int idx) {

        var selectedContentCell = false;

        if (_selectedId == -1) {
            selectedContentCell = true;
            _selectedId = idx;
        }
        else if (_selectedId == idx - 1) {
            _selectedId = -1;
        }
        else if (_selectedId != idx) {
            selectedContentCell = true;
            if (idx > _selectedId) {
                idx -= 1;
            }
            _selectedId = idx;
        }
        else {
            _selectedId = -1;
        }

        (int _, int maxIdx) = GetVisibleCellsIdRange();

        ReloadData(_selectedId);

        if (_selectedId >= maxIdx) {
            scrollView.ScrollTo((_selectedId + 1) * cellSize, animated: true);
        }

        if (selectedContentCell) {
            didSelectContentCellEvent?.Invoke(this, idx);
        } else if (_selectedId == -1) {
            didDeselectContentCellEvent?.Invoke(this, idx);
        }
    }
}
