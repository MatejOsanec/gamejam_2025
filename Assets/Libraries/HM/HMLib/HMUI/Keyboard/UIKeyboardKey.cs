using TMPro;
using UnityEngine;

namespace HMUI {

    public class UIKeyboardKey : MonoBehaviour {

        [SerializeField] KeyCode _keyCode;
        [SerializeField, NullAllowed] TextMeshProUGUI _text;
        [SerializeField] string _overrideText;
        [SerializeField] bool _canBeUppercase;
        [Tooltip(@"If this is true, text will not be set into the UI of this key, but the key will still exists
                   and report it's presses. Intended usage is for example with an icon instead of text")]
        [SerializeField] bool _dontSetText;

        public KeyCode keyCode => _keyCode;
        public bool canBeUppercase => _canBeUppercase;

        protected void Awake() {

            if (!_dontSetText) {
                _text.text = string.IsNullOrEmpty(_overrideText) ? keyCode.ToString() : _overrideText;
            }
        }

        protected void OnValidate() {

            if (!_dontSetText && _text != null) {
                _text.text = string.IsNullOrEmpty(_overrideText) ? keyCode.ToString() : _overrideText;
            }
        }
    }
}
