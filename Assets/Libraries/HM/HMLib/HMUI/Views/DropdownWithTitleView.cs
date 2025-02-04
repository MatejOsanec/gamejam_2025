using UnityEngine;

namespace HMUI {

    public class DropdownWithTitleView : SimpleTextDropdown {

        [SerializeField] RectTransform _rectTransform;
        [SerializeField] RectTransform _titleRectTransform;

        protected override void RefreshSize(TableView.IDataSource dataSource) {

            var newSize = GetNewTableViewRectSize(dataSource);
            var size = newSize + _titleRectTransform.sizeDelta.y;
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
            base.RefreshSize(dataSource);
        }
    }
}
