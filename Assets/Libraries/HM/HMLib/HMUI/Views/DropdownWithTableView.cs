using UnityEngine;
using UnityEngine.UI;

namespace HMUI {

    [RequireComponent(typeof(RectTransform))]
    public class DropdownWithTableView : MonoBehaviour {

        [SerializeField] Button _button = default;
        [SerializeField] TableView _tableView = default;
        [SerializeField] ModalViewBase _modalView = default;
        [Tooltip("Increase this if you need extra space for buttons or graphics on the bottom of the dropdown.")]
        [SerializeField] float _extraSpace = 0.0f;
        [SerializeField] int _numberOfVisibleCells = 5;
        [SerializeField] bool _hideOnSelection = true;

        public event System.Action<DropdownWithTableView, int> didSelectCellWithIdxEvent;

        public TableView.IDataSource tableViewDataSource => _tableView.dataSource;
        public int selectedIndex { get; private set; }
        public bool interactable {
            get => _button.interactable;
            set => _button.interactable = value;
        }

        public void Init(TableView.IDataSource tableViewDataSource) {

            _tableView.SetDataSource(tableViewDataSource, reloadData: true);
        }

        public void ReloadData() {

            _tableView.ReloadData();
            RefreshSize(tableViewDataSource);
        }

        public virtual void SelectCellWithIdx(int idx) {

            selectedIndex = idx;
            _tableView.SelectCellWithIdx(idx);
        }

        protected virtual void Awake() {

            _button.onClick.AddListener(OnButtonClick);

            _modalView.blockerClickedEvent += HandleModalViewBlockerClicked;

            _tableView.didSelectCellWithIdxEvent += HandleTableViewDidSelectCellWithIdx;
            _modalView.gameObject.SetActive(false);
        }

        protected void OnDisable() {

            Hide(animated: false);
        }

        protected virtual void OnDestroy() {

            if (_tableView != null) {
                _tableView.didSelectCellWithIdxEvent -= HandleTableViewDidSelectCellWithIdx;
            }

            if (_button != null) {
                _button.onClick.RemoveListener(OnButtonClick);
            }

            if (_modalView != null) {
                _modalView.blockerClickedEvent -= HandleModalViewBlockerClicked;
            }
        }

        protected virtual void RefreshSize(TableView.IDataSource dataSource) {

            _tableView.ChangeRectSize(RectTransform.Axis.Vertical, GetNewTableViewRectSize(dataSource));
        }

        protected float GetNewTableViewRectSize(TableView.IDataSource dataSource) {

            float newViewportSize = Mathf.Min(_numberOfVisibleCells, _tableView.numberOfCells) * dataSource.CellSize(TableView.kFixedCellSizeIndex);
            newViewportSize += _extraSpace;
            float currentViewportSize = _tableView.viewportTransform.rect.height;
            float currentTableSize = ((RectTransform)_tableView.transform).rect.height;
            return newViewportSize + (currentTableSize - currentViewportSize);
        }

        private void OnButtonClick() {

            Show(animated: true);
        }

        private void HandleTableViewDidSelectCellWithIdx(TableView tableView, int idx) {

            selectedIndex = idx;
            didSelectCellWithIdxEvent?.Invoke(this, idx);

            if (_hideOnSelection) {
                Hide(animated: true);
            }
        }

        public void Hide(bool animated) {

            _modalView.Hide(animated);
            _button.enabled = true;
        }

        public void Show(bool animated) {

            if (!isActiveAndEnabled) {
                return;
            }

            _button.enabled = false;
            _modalView.Show(animated, moveToCenter: false);

            _tableView.ScrollToCellWithIdx(selectedIndex, TableView.ScrollPositionType.Center, animated: false);
        }

        private void HandleModalViewBlockerClicked() {

            Hide(animated: true);
        }
    }
}
