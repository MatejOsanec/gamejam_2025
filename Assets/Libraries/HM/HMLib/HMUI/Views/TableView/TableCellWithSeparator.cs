using HMUI;
using UnityEngine;

public class TableCellWithSeparator : TableCell {

    [SerializeField] GameObject _separator = default;

    public override void TableViewSetup(ITableCellOwner tableCellOwner, int idx) {

        base.TableViewSetup(tableCellOwner, idx);

        _separator.SetActive(idx < tableCellOwner.numberOfCells - 1);
    }
}
