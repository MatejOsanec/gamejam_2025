using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HMUI {

    public enum TableViewSelectionType {
        None,
        Single,
        Multiple,
        DeselectableSingle
    }

    public interface ITableCellOwner {

        TableViewSelectionType selectionType { get; }
        bool canSelectSelectedCell { get; }
        int numberOfCells { get; }
    }
}
