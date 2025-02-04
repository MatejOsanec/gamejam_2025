using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace HMUI {

    public class ScrollViewItemsVisibilityController : MonoBehaviour {

        [SerializeField] RectTransform _viewport = default;
        [SerializeField] RectTransform _contentRectTransform = default;

        private ScrollViewItemForVisibilityController[] _items;
        private float _lastContentAnchoredPositionY;
        private Vector3[] _viewportWorldCorners = new Vector3[4];

        private Tuple<ScrollViewItemForVisibilityController, float>[] _upperItemsCornes;
        private Tuple<ScrollViewItemForVisibilityController, float>[] _lowerItemsCornes;

        private int _lowerLastVisibleIndex;
        private int _upperLastVisibleIndex;

        private float _contentMaxY;
        private float _contentMinY;

        protected void Start() {

            _viewport.GetWorldCorners(_viewportWorldCorners);

            var y0 = _viewport.InverseTransformPoint(_viewportWorldCorners[0]).y;
            var y1 = _viewport.InverseTransformPoint(_viewportWorldCorners[2]).y;

            _contentMinY = Mathf.Min(y0, y1);
            _contentMaxY = Mathf.Max(y0, y1);

            _items = GetComponentsInChildren<ScrollViewItemForVisibilityController>(includeInactive: true);

            _upperItemsCornes = new Tuple<ScrollViewItemForVisibilityController, float>[_items.Length];
            _lowerItemsCornes = new Tuple<ScrollViewItemForVisibilityController, float>[_items.Length];

            var worldCorners = new Vector3[4];
            for (int i = 0; i < _items.Length; ++i) {

                _items[i].GetWorldCorners(worldCorners);

                y0 = _viewport.InverseTransformPoint(worldCorners[0]).y - _contentRectTransform.anchoredPosition.y;
                y1 = _viewport.InverseTransformPoint(worldCorners[2]).y - _contentRectTransform.anchoredPosition.y;

                _lowerItemsCornes[i] = Tuple.Create(_items[i], Mathf.Min(y0, y1));
                _upperItemsCornes[i] = Tuple.Create(_items[i], Mathf.Max(y0, y1));
            }

            _upperItemsCornes = _upperItemsCornes.OrderBy(item => item.Item2).ToArray();
            _lowerItemsCornes = _lowerItemsCornes.OrderBy(item => item.Item2).ToArray();

            _lowerLastVisibleIndex = _items.Length - 1;
            _upperLastVisibleIndex = 0;

            UpdateVisibilityUpDirection(0);
        }

        protected void Update() {

            if (Mathf.Abs(_lastContentAnchoredPositionY - _contentRectTransform.anchoredPosition.y) > 0.001f) {

                if (_lastContentAnchoredPositionY < _contentRectTransform.anchoredPosition.y) {
                    UpdateVisibilityDownDirection(_contentRectTransform.anchoredPosition.y);
                }
                else {
                    UpdateVisibilityUpDirection(_contentRectTransform.anchoredPosition.y);
                }
                _lastContentAnchoredPositionY = _contentRectTransform.anchoredPosition.y;
            }
        }

        private void UpdateVisibilityUpDirection(float newContentAnchoredPositionY) {

            int i = _upperLastVisibleIndex;
            while (i < _lowerItemsCornes.Length) {
                var itemCorner = _lowerItemsCornes[i];
                if (itemCorner.Item2 + newContentAnchoredPositionY < _contentMaxY) {
                    itemCorner.Item1.gameObject.SetActive(true);
                    i++;
                }
                else {
                    break;
                }
            }
            _upperLastVisibleIndex = Math.Min(i, _lowerItemsCornes.Length - 1);

            i = _lowerLastVisibleIndex;
            while (i < _upperItemsCornes.Length) {
                var itemCorner = _upperItemsCornes[i];
                if (itemCorner.Item2 + newContentAnchoredPositionY < _contentMinY) {
                    itemCorner.Item1.gameObject.SetActive(false);
                    i++;
                }
                else {
                    break;
                }
            }
            _lowerLastVisibleIndex = Math.Min(i, _upperItemsCornes.Length - 1);
        }

        private void UpdateVisibilityDownDirection(float newContentAnchoredPositionY) {

            int i = _lowerLastVisibleIndex;
            while (i >= 0) {
                var itemCorner = _upperItemsCornes[i];
                if (itemCorner.Item2 + newContentAnchoredPositionY > _contentMinY) {
                    itemCorner.Item1.gameObject.SetActive(true);
                    i--;
                }
                else {
                    break;
                }
            }
            _lowerLastVisibleIndex = Math.Max(i, 0);

            i = _upperLastVisibleIndex;
            while (i >= 0) {
                var itemCorner = _lowerItemsCornes[i];
                if (itemCorner.Item2 + newContentAnchoredPositionY > _contentMaxY) {
                    itemCorner.Item1.gameObject.SetActive(false);
                    i--;
                }
                else {
                    break;
                }
            }
            _upperLastVisibleIndex = Math.Max(i, 0);
        }
    }
}