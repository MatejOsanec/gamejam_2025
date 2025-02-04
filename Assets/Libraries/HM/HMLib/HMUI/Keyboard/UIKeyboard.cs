using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HMUI {

    public enum CapsLockState {

        Lowercase = 0,
        UppercaseOnce = 1,
        Uppercase = 2,
    }

    public class UIKeyboard : MonoBehaviour {

#if BS_TOURS
        [NullAllowed]
#elif !BS_TOURS || UNITY_EDITOR
        [WillNotBeUsed]
#endif
        [SerializeField] Button _okButton;
        [SerializeField] CapsLockState _startsCapsLockState;
        [SerializeField] bool _allowAtRuntime;

#if BS_TOURS || UNITY_EDITOR
        [FutureField, NullAllowed]
        [SerializeField] GameObject _lettersGameObject;
        [FutureField, NullAllowed]
        [SerializeField] GameObject _numbersGameObject;
#endif

        public event Action okButtonWasPressedEvent;
        public event Action<char> keyWasPressedEvent;
        public event Action deleteButtonWasPressedEvent;
        public event Action<CapsLockState> capsLockStateChangedEvent;

        private readonly ButtonBinder _buttonBinder = new ButtonBinder();

        private const float kCapsLockPressWindowToToggleUppercase = 0.2f;
        private CapsLockState _capsLockState;
        private float _capsLockUppercaseOnceTime;
        private List<TextMeshProUGUI> _letterBtnTexts;
#if BS_TOURS
        private bool _showingLetters = true;
#endif

        public bool shouldCapitalize => _capsLockState != CapsLockState.Lowercase;
        public CapsLockState capsLockState => _capsLockState;

        protected void Awake() {

            if (_okButton != null) {
                _buttonBinder.AddBinding(_okButton, () => okButtonWasPressedEvent?.Invoke());
            }
            _letterBtnTexts = new List<TextMeshProUGUI>();
            var keys = GetComponentsInChildren<UIKeyboardKey>(includeInactive: true);
            foreach (var key in keys) {
                var button = key.GetComponent<NoTransitionsButton>();
                if (key.canBeUppercase) {
                    _letterBtnTexts.Add(button.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true));
                }
                switch (key.keyCode) {
                    case KeyCode.CapsLock:
                        _buttonBinder.AddBinding(button, HandleCapsLockPressed);
                        break;
                    case KeyCode.Backspace:
                        _buttonBinder.AddBinding(button, () => deleteButtonWasPressedEvent?.Invoke());
                        break;
#if BS_TOURS
                    case KeyCode.Numlock:
                        _buttonBinder.AddBinding(button, HandleNumSwitchPressed);
                        break;
#endif
                    case KeyCode.Return:
                        _buttonBinder.AddBinding(button, () => okButtonWasPressedEvent?.Invoke());
                        break;
                    default:
                        _buttonBinder.AddBinding(button, () => HandleKeyPress(key.keyCode));
                        break;
                }
            }
        }

        protected void Update() {

            if (!_allowAtRuntime && !Application.isEditor) {
                return;
            }

            // Allow using real keyboards for text entry as well as the on screen one
            if (!Input.anyKey) {
                return;
            }

            string inputString = Input.inputString;

            for (int i = 0; i < inputString.Length; i++) {
                switch (inputString[i]) {

                    case '\b':
                        deleteButtonWasPressedEvent?.Invoke();
                        break;
                    case '\n':
                    case '\r':
                        okButtonWasPressedEvent?.Invoke();
                        return;
                    default:
                        keyWasPressedEvent?.Invoke(inputString[i]);
                        break;
                }
            }
        }

        private void HandleKeyPress(KeyCode keyCode) {

            int intKey = (int)keyCode;
            // letters A(97) - Z(122)
            if (intKey > 96 && intKey < 123) {
                char inputKey  = (char)intKey;
                switch (_capsLockState) {
                    case CapsLockState.UppercaseOnce:
                        inputKey = char.ToUpper(inputKey);
                        SetCapsLockState(CapsLockState.Lowercase);
                        SetKeyboardCapitalization(shouldCapitalize);
                        break;
                    case CapsLockState.Uppercase:
                        inputKey = char.ToUpper(inputKey);
                        break;
                }
                keyWasPressedEvent?.Invoke(inputKey);
            }
            // numbers 0(256) - 9(266)
            else if (intKey > 255 && intKey < 266) {
                // weird mapping for numbers instead of 48 - 57 is unity using 256 - 265
                keyWasPressedEvent?.Invoke((char)(intKey - 208));
            }
            // space(32)
            else if (intKey == 32) {
                keyWasPressedEvent?.Invoke(' ');
            }
            // other
            else {
                char inputKey  = (char)intKey;
                keyWasPressedEvent?.Invoke(inputKey);
            }
        }

        private void HandleCapsLockPressed() {

            CapsLockState newState;
            switch (capsLockState) {
                case CapsLockState.Lowercase:
                    _capsLockUppercaseOnceTime = Time.realtimeSinceStartup;
                    newState = CapsLockState.UppercaseOnce;
                    break;
                case CapsLockState.UppercaseOnce:
                    // Handle double click as switch to uppercase
                    bool shouldBeUppercase = _capsLockUppercaseOnceTime + kCapsLockPressWindowToToggleUppercase >=
                           Time.realtimeSinceStartup;
                    newState = shouldBeUppercase ? CapsLockState.Uppercase : CapsLockState.Lowercase;
                    break;
                case CapsLockState.Uppercase:
                    newState = CapsLockState.Lowercase;
                    break;
                default:
                    Debug.LogError($"Invalid keyboard capslock state {capsLockState}");
                    newState = CapsLockState.Lowercase;
                    break;
            }

            SetCapsLockState(newState);
            SetKeyboardCapitalization(shouldCapitalize);
        }

#if BS_TOURS
        private void HandleNumSwitchPressed() {

            _showingLetters = !_showingLetters;
            _lettersGameObject.SetActive(_showingLetters);
            _numbersGameObject.SetActive(!_showingLetters);

            _capsLockState = CapsLockState.Lowercase;
            SetKeyboardCapitalization(shouldCapitalize);
        }
#endif

        private void SetCapsLockState(CapsLockState newState) {

            _capsLockState = newState;
            capsLockStateChangedEvent?.Invoke(newState);
        }

        private void SetKeyboardCapitalization(bool capitalize) {

            for (int i = 0; i < _letterBtnTexts.Count; i++) {
                var tmPro = _letterBtnTexts[i];
                FontStyles fontStyle = _letterBtnTexts[i].fontStyle;
                if (capitalize && !HasFontStyle(tmPro, FontStyles.UpperCase)) {
                    fontStyle = fontStyle | FontStyles.UpperCase;
                }
                else if (!capitalize && HasFontStyle(tmPro, FontStyles.UpperCase)) {
                    fontStyle ^=  FontStyles.UpperCase;
                }
                tmPro.fontStyle = fontStyle;
            }

        }

        private bool HasFontStyle(TextMeshProUGUI text, FontStyles style) {

            return (text.fontStyle & style) != 0;
        }

        private void OnEnable() {

#if BS_TOURS
            _showingLetters = true;
            _lettersGameObject.SetActive(_showingLetters);
            _numbersGameObject.SetActive(!_showingLetters);
#endif
            SetCapsLockState(_startsCapsLockState);
            SetKeyboardCapitalization(shouldCapitalize);
        }
    }
}
