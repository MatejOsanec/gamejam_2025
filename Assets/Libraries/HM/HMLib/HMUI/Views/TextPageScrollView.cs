using UnityEngine;
using TMPro;

namespace HMUI {

    public class TextPageScrollView : ScrollView {

        [SerializeField] TextMeshProUGUI _text = default;

#if BS_TOURS
        public enum FocusPoint {
            Up,
            Down
        }

        private FocusPoint _focusPoint = FocusPoint.Up;

        public void Init(FocusPoint focusPoint) {

            _text.text = "";
            _focusPoint = focusPoint;
        }
#endif

        public void SetText(string text) {

            _text.text = text;
            UpdateMeshes();
        }

        public void AddText(string text) {

            _text.text += text;
            UpdateMeshes();
        }

        public void UpdateMeshes() {

            _text.ForceMeshUpdate();

            var contentHeight = _text.preferredHeight;
            SetContentSize(contentHeight);

#if BS_TOURS
            if (_focusPoint == FocusPoint.Down) {
                ScrollToEnd(animated: true);
            }
#endif
        }
    }
}
