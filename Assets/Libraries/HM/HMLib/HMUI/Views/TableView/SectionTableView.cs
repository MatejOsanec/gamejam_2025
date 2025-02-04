using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HMUI {

    public class SectionTableView : TableView, TableView.IDataSource {

        [SerializeField] bool _unfoldSectionsByDefault = false;

        public event System.Action<SectionTableView, int, int> didSelectRowInSectionEvent;
        public event System.Action<SectionTableView, int> didSelectHeaderEvent;

        public new interface IDataSource {
            float RowHeight();
            int NumberOfSections();
            int NumberOfRowsInSection(int section);
            TableCell CellForSectionHeader(int section, bool unfolded);
            TableCell CellForRowInSection(int section, int row);
        }

        public new IDataSource dataSource {
            get {
                return _dataSource;
            }
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

        private struct Section {
            public bool unfolded;
            public int startBaseRow;
            public int numberOfBaseRows;
        }

        private Section[] _sections;

        public bool IsSectionUnfolded(int section) {

            return _sections[section].unfolded;
        }

        public float CellSize(int idx) {

            return _dataSource.RowHeight();
        }

        public int NumberOfCells() {

            if (_sections == null || _sections.Length == 0) {
                return 0;
            }

            return _sections[_sections.Length - 1].startBaseRow + _sections[_sections.Length - 1].numberOfBaseRows;
        }

        public TableCell CellForIdx(TableView tableView, int baseRow) {

            int row;
            int section;
            bool isSectionHeader;

            SectionAndRowForBaseRow(baseRow, out section, out row, out isSectionHeader);

            if (isSectionHeader) {
                return _dataSource.CellForSectionHeader(section, _sections[section].unfolded);
            }
            else {
                return _dataSource.CellForRowInSection(section: section, row: row);
            }
        }

        public override void ReloadData() {

            ReloadData(resetFoldState: true);
            // base.ReloadData is not called here, because it's called in ReloadData(bool resetFoldState) function.
        }

        public void ReloadData(bool resetFoldState) {

            // Prepare data.
            int numberOfSections = _dataSource.NumberOfSections();
            if (_sections == null || _sections.Length != numberOfSections) {
                _sections = new Section[numberOfSections];
            }

            // Reset fold state.
            if (resetFoldState) {
                for (int i = 0; i < _sections.Length; i++) {
                    _sections[i].unfolded = _unfoldSectionsByDefault;
                }
            }

            // Prepare structure which helps to compute row and section from base row index.
            int position = 0;
            for (int i = 0; i < _sections.Length; i++) {
                _sections[i].startBaseRow = position;
                int numberOfRowsInSection = _dataSource.NumberOfRowsInSection(i);

                if (_sections[i].unfolded) {
                    _sections[i].numberOfBaseRows = numberOfRowsInSection + 1; // Section header is included in number of rows in section.
                }
                else {
                    _sections[i].numberOfBaseRows = 1; // Only section header.
                }

                position += _sections[i].numberOfBaseRows;
            }

            base.ReloadData();
        }

        protected override void DidSelectCellWithIdx(int baseRow) {

            int row;
            int section;
            bool isSectionHeader;

            SectionAndRowForBaseRow(baseRow, out section, out row, out isSectionHeader);

            if (isSectionHeader) {
                didSelectHeaderEvent?.Invoke(this, section);
            }
            else {
                didSelectRowInSectionEvent?.Invoke(this, row, section);
            }
        }

        public void UnfoldAllSections() {

            for (int i = 0; i < _sections.Length; i++) {
                _sections[i].unfolded = true;
            }
            ReloadData(resetFoldState: false);
        }

        public void FoldAll() {

            for (int i = 0; i < _sections.Length; i++) {
                _sections[i].unfolded = false;
            }
            ReloadData(resetFoldState: false);
        }

        public void UnfoldSection(int section) {

            if (_sections[section].unfolded) {
                return;
            }

            // Update data.
            _sections[section].unfolded = true;
            int numberOfRowsInSection = _dataSource.NumberOfRowsInSection(section);
            _sections[section].numberOfBaseRows = numberOfRowsInSection + 1; // Section header is included in number of rows in section.
            for (int i = section + 1; i < _sections.Length; i++) {
                _sections[i].startBaseRow += numberOfRowsInSection;
            }

            InsertCells(_sections[section].startBaseRow + 1, numberOfRowsInSection);
        }

        public void FoldSection(int section) {

            if (!_sections[section].unfolded) {
                return;
            }

            // Update data.
            int prevNumberOfBaseRowsInSection = _sections[section].numberOfBaseRows;
            _sections[section].unfolded = false;
            _sections[section].numberOfBaseRows = 1; // Only section header.
            for (int i = section + 1; i < _sections.Length; i++) {
                _sections[i].startBaseRow -= prevNumberOfBaseRowsInSection - 1;
            }

            DeleteCells(_sections[section].startBaseRow + 1, prevNumberOfBaseRowsInSection - 1);
        }

        public void ScrollToRow(int section, int row, TableView.ScrollPositionType scrollPositionType, bool animated) {

            ScrollToCellWithIdx(_sections[section].startBaseRow + row, scrollPositionType, animated);
        }

        public void SectionAndRowForBaseRow(int baseRow, out int section, out int row, out bool isSectionHeader) {

            // Find the section and row for base row.
            // Splitting invervals algorithm.
            int lowSection = 0;
            int highSection = _sections.Length - 1;
            section = _sections.Length / 2;
            bool sectionChanged = false;
            do {
                sectionChanged = false;
                if (baseRow < _sections[section].startBaseRow) {
                    section = (section + lowSection) / 2;
                    highSection = section - 1;
                    sectionChanged = true;
                }
                else if (baseRow >= _sections[section].startBaseRow + _sections[section].numberOfBaseRows) {
                    section = (section + 1 + highSection) / 2;
                    lowSection = section + 1;
                    sectionChanged = true;
                }
            } while (sectionChanged);

            row = baseRow - _sections[section].startBaseRow;

            if (row == 0) {
                isSectionHeader = true;
            }
            else {
                isSectionHeader = false;
                row -= 1;
            }
        }
    }
}
