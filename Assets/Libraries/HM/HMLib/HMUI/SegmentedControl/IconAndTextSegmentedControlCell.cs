using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HMUI {

    public class IconAndTextSegmentedControlCell : SegmentedControlCell {

        [SerializeField] Image _icon = default;
        [SerializeField] protected TextMeshProUGUI _text = default;

        public Sprite sprite {
            set => _icon.sprite = value;
            get => _icon.sprite;
        }

        public string text {
            set => _text.text = value;
            get => _text.text;
        }

        public void SetTextActive(bool active) {

            _text.gameObject.SetActive(active);
        }
    }
}
