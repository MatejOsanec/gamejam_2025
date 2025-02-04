using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace HMUI {

    public class HoverHintPanel : MonoBehaviour {

        [SerializeField] TMPro.TextMeshProUGUI _text = default;
        [SerializeField] Vector2 _padding = new Vector2(6.0f, 4.0f);
        [SerializeField] Vector2 _containerPadding = new Vector2(8.0f, 8.0f);
        [SerializeField] float _separator = 2.0f;
        [SerializeField] float _zOffset = 0.1f;

        public bool isShown { get; private set; }

        protected void Awake() {

            var rectTransform = (RectTransform)transform;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        }

        public void Show(string text, Transform parent, Vector2 containerSize, Rect spawnRect) {

            isShown = true;

            var rectTransform = (RectTransform)transform;
            rectTransform.SetParent(parent, worldPositionStays: false);
            rectTransform.SetAsLastSibling();
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
            gameObject.SetActive(true);

            _text.text = text;
            _text.ForceMeshUpdate();
            Vector2 textSize = _text.bounds.size;

            var panelSize = textSize + _padding;
            rectTransform.sizeDelta = panelSize;
            rectTransform.anchoredPosition = CalculatePanelPosition(containerSize, spawnRect, panelSize);
            var localPos = rectTransform.localPosition;
            localPos.z = -_zOffset;
            rectTransform.localPosition = localPos;
        }

        public void Hide() {

            isShown = false;
            gameObject.SetActive(false);
        }

        private Vector2 CalculatePanelPosition(Vector2 containerSize, Rect spawnRect, Vector2 panelSize) {

            // Calculate X Pos
            float x = spawnRect.center.x;
            if (x < -containerSize.x * 0.5f + _containerPadding.x + panelSize.x * 0.5f) {
                x = -containerSize.x * 0.5f + _containerPadding.x + panelSize.x * 0.5f;
            }
            else if (x > containerSize.x * 0.5f - _containerPadding.x - panelSize.x * 0.5f) {
                x = containerSize.x * 0.5f - _containerPadding.x - panelSize.x * 0.5f;
            }

            float y = spawnRect.center.y + spawnRect.size.y * 0.5f + _separator + panelSize.y * 0.5f;
            if (y > containerSize.y * 0.5f - _containerPadding.y - panelSize.y * 0.5f) {
                y = spawnRect.center.y - spawnRect.size.y * 0.5f - _separator - panelSize.y * 0.5f;
            }

            return new Vector2(x, y);
        }
    }
}