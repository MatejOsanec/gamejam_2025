using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace BGLib.UIToolkitUtilities.Controls {

    public class Table : VisualElement {

        public class TableColumn {

            public readonly string name;
            public readonly string headerTooltip;
            public readonly Enum cellType;
            public readonly bool initialVisibility = true;
            public readonly int minWidth = -1;
            public readonly int maxWidth = -1;

            public TableColumn(string name, Enum cellType) {

                this.name = name;
                this.headerTooltip = name;
                this.cellType = cellType;
            }

            public TableColumn(string name = "", string headerTooltip = "", Enum cellType = null, bool visible = true, int minWidth = -1, int maxWidth = -1) {

                this.name = name;
                this.headerTooltip = headerTooltip;
                this.cellType = cellType;
                this.initialVisibility = visible;
                this.minWidth = minWidth;
                this.maxWidth = maxWidth;
            }
        }

        public class RuntimeTableColumn {

            public int visibleColumnIndex;
            public bool visible;
        }

        public new class UxmlFactory : UxmlFactory<Table, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits {

            UxmlBoolAttributeDescription _showHeader = new() { name = "show-header" };
            UxmlBoolAttributeDescription _showFooter = new() { name = "show-footer" };
            UxmlIntAttributeDescription _numRows = new() { name = "num-rows" };
            UxmlEnumAttributeDescription<ScrollerVisibility> _verticalScrollerVisibility = new() { name = "vertical-scroller-visibility" };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription { get { yield break; } }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc) {

                base.Init(ve, bag, cc);
                Table tableToInit = (Table)ve;
                tableToInit.showHeader = _showHeader.GetValueFromBag(bag, cc);
                tableToInit.showFooter = _showFooter.GetValueFromBag(bag, cc);
                tableToInit.numRows = _numRows.GetValueFromBag(bag, cc);
                tableToInit.scrollView.verticalScrollerVisibility = _verticalScrollerVisibility.GetValueFromBag(bag, cc);   // TODO test if this works
            }
        }

        /// <summary> Whether to add a footer row; this is a row that fires different events so you can stylize it differently. EG adding a row with only a + button for a new entry. </summary>
        public bool showHeader { get; set; }

        /// <summary> Whether to add a header row with column names </summary>
        public bool showFooter { get; set; }

        /// <summary> Amount of rows to create, excluding header or footer. </summary>
        public int numRows { get; set; }

        /// <summary> Event that fires for each cell for you to fill it. Amount times it fires: numRows*columnSetup.Count </summary>
        public Action<Enum, int, VisualElement> fillCellEvent;

        /// <summary> Event that fires for each footer cell for you to fill it. Amount times it fires: numRows </summary>
        public Action<Enum, VisualElement> fillFooterCellEvent;

        /// <summary> Event that fires for each row for you to fill it, as alternative for fillCellEvent. It has all the cells that would be called for that event already pre-filled. Does not call for footer. </summary>
        public Action<int, VisualElement> fillRowEvent;

        /// <summary> Whether Initialize() has been called on this instance. </summary>
        public bool initialized { get; private set; }

        protected List<TableColumn> columnSetup;
        protected List<RuntimeTableColumn> runtimeColumnData;

        private const string altRowAdditionalStyle = "bg-uitoolkit__table__row-alt";
        private const string headerCellStyle = "bg-uitoolkit__table__cell-header";
        private ScrollView scrollView;
        private VisualElement scrollViewContentContainer;
        private bool newRowIsAltStyle;

        public Table() : base() {

            scrollView = new();
            scrollView.style.flexGrow = 1;
            scrollViewContentContainer = scrollView.Query("unity-content-container");
            this.Add(scrollView);
            initialized = false;
        }

        /// <summary> Do not modify columnSetup during runtime. We're not making a deep copy here so I trust you treat this as a const 🤡 </summary>
        public void Initialize(List<TableColumn> columnSetup) {

            if (initialized) {
                return;
            }

            initialized = true;

            this.columnSetup = columnSetup;
            runtimeColumnData = new(columnSetup.Count);

            // Populate runtime column data;
            int visibleColumnIndex = 0;
            for (int i = 0; i < columnSetup.Count; i++) {
                runtimeColumnData.Insert(i, new() {
                    visible = columnSetup[i].initialVisibility
                });

                if (columnSetup[i].initialVisibility) {
                    runtimeColumnData[i].visibleColumnIndex = visibleColumnIndex;
                    visibleColumnIndex++;
                }
                else {
                    runtimeColumnData[i].visibleColumnIndex = -1;
                }
            }
        }

        /// <summary> Kicks off the process of building new content for the table based on columnSetup and uxml attributes. Be sure to bind to at least fillCellEvent so you can fill each cell with data appropriately. </summary>
        public virtual void BuildTable() {

            if (!initialized) {
                return;
            }

            if (showHeader) {
                CreateHeaderRow();
            }

            for (int i = 0; i < numRows; i++) {
                CreateRow(i);
            }

            if (showFooter) {
                CreateFooterRow();
            }
        }

        /// <summary> Completely wipes the table. </summary>
        public void ClearTable() {

            if (!initialized) {
                return;
            }

            scrollViewContentContainer.RemoveAllChildren();
        }

        /// <summary> Creates a basic row skeleton based on the amount of columns in ColumSetup, and inserts it into the scrollViewContentContainer.</summary>
        protected VisualElement CreateRowSkeleton() {

            VisualElement row = new() { name = "row" };
            row.AddToClassList("bg-uitoolkit__table__row");
            int visibleColumnIndex = 0;
            for (int i = 0; i < columnSetup.Count; i++) {
                if (!runtimeColumnData[i].visible) {
                    continue;
                }

                VisualElement cell = new() { name = "cell" };
                cell.AddToClassList("bg-uitoolkit__table__cell");
                cell.style.justifyContent = Justify.Center;
                if (columnSetup[i].maxWidth >= 0) {
                    cell.style.maxWidth = columnSetup[i].maxWidth;
                }
                if (columnSetup[i].minWidth >= 0) {
                    cell.style.minWidth = columnSetup[i].minWidth;
                }

                runtimeColumnData[i].visibleColumnIndex = visibleColumnIndex;
                visibleColumnIndex++;

                row.Add(cell);
            }

            if (newRowIsAltStyle) {
                row.AddToClassList(altRowAdditionalStyle);
            }
            newRowIsAltStyle = !newRowIsAltStyle;

            scrollViewContentContainer.Add(row);
            return row;
        }

        protected virtual VisualElement CreateHeaderRow() {

            var result = CreateRowSkeleton();

            for (int i = 0; i < columnSetup.Count; i++) {
                if (!runtimeColumnData[i].visible) {
                    continue;
                }

                VisualElement cell = result.ElementAt(runtimeColumnData[i].visibleColumnIndex);
                Label headerLabel = new() { name = "label-header", text = columnSetup[i].name };
                headerLabel.AddToClassList("heading3");
                cell.tooltip = columnSetup[i].headerTooltip;
                cell.AddToClassList(headerCellStyle);
                cell.Add(headerLabel);
            }

            return result;
        }

        protected VisualElement CreateRow(int rowIndex) {

            var result = CreateRowSkeleton();
            fillRowEvent?.Invoke(rowIndex, result);

            for (int i = 0; i < columnSetup.Count; i++) {
                if (!runtimeColumnData[i].visible) {
                    continue;
                }

                VisualElement cell = result.ElementAt(runtimeColumnData[i].visibleColumnIndex);
                fillCellEvent?.Invoke(columnSetup[i].cellType, rowIndex, cell);
            }

            return result;
        }

        protected VisualElement CreateFooterRow() {

            var result = CreateRowSkeleton();
            for (int i = 0; i < columnSetup.Count; i++) {
                if (!runtimeColumnData[i].visible) {
                    continue;
                }

                VisualElement cell = result.ElementAt(runtimeColumnData[i].visibleColumnIndex);
                fillFooterCellEvent.Invoke(columnSetup[i].cellType, cell);
            }

            return result;
        }
    }
}
