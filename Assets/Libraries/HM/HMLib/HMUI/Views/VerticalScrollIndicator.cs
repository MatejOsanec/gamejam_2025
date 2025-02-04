using UnityEngine;

namespace HMUI {

    public class VerticalScrollIndicator : MonoBehaviour {

        [SerializeField] RectTransform _handle = default;
#if BS_TOURS
        [SerializeField] float _verticalPadding = 1.0f;
        [SerializeField] float _horizontalPadding = 0.0f;
#else
        [SerializeField] float _padding = 0.0f;
#endif

        public float progress {
            set {
                if (_progress == value) {
                    return;
                }
                _progress = Mathf.Clamp01(value);
                RefreshHandle();
            }
            get => _progress;
        }

        public float normalizedPageHeight {
            set {
                if (_normalizedPageHeight == value) {
                    return;
                }
                _normalizedPageHeight = Mathf.Clamp01(value);
                RefreshHandle();
            }
            get => _normalizedPageHeight;
        }

        public RectTransform handle => _handle;

        private float _progress = 0.0f;
        private float _normalizedPageHeight = 1.0f;

        protected void OnEnable() {

            RefreshHandle();
        }

#if BS_TOURS
        private void RefreshHandle() {

            var rectTransform = (RectTransform)transform;
            var rectSize = rectTransform.rect.size;
            float fullHeight = rectSize.y - 2.0f * _verticalPadding;
            float width = rectSize.x - 2.0f * _horizontalPadding;
            _handle.sizeDelta = new Vector2(width, _normalizedPageHeight * fullHeight);
            _handle.anchoredPosition = new Vector2(0.0f, -_progress * (1.0f - _normalizedPageHeight) * fullHeight - _verticalPadding);
        }
#else
        private void RefreshHandle() {

            var rectTransform = (RectTransform)transform;
            float fullHeight = rectTransform.rect.size.y - 2.0f * _padding;
            _handle.sizeDelta = new Vector2(0.0f, _normalizedPageHeight * fullHeight);
            _handle.anchoredPosition = new Vector2(0.0f, -_progress * (1.0f - _normalizedPageHeight) * fullHeight - _padding);
        }
#endif
    }
}
