using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace HMUI {

    public class InputFieldView : Selectable {

        private const float kBlinkingRate = 0.4f;

        [SerializeField] TextMeshProUGUI _textView = default;
        [SerializeField] CanvasGroup _textViewCanvasGroup = default;
        [SerializeField] ImageViewBase _blinkingCaret = default;
        [SerializeField] GameObject _placeholderText = default;
        [SerializeField] Button _clearSearchButton = default;

        [Header("Input Specific Keyboard Settings")]
        [SerializeField] bool _useGlobalKeyboard = true;
        [SerializeField, DrawIf(nameof(_useGlobalKeyboard), true)] bool _useSystemKeyboardIfAvailable;
#if BS_TOURS || UNITY_EDITOR
#pragma warning disable CS0414
        [FutureField]
        [SerializeField, DrawIf(nameof(_useGlobalKeyboard), true)] string _globalKeyboardTitleLocalizationKey;
        [FutureField]
        [SerializeField, DrawIf(nameof(_useGlobalKeyboard), true)] bool _fadeBackgroundScreens = true;
#pragma warning restore CS0414
#endif
        [SerializeField] Vector3 _keyboardPositionOffset = Vector3.zero;

        [Header("Input Field Settings")]
        [SerializeField] bool _useUppercase = false;
        [SerializeField] int _textLengthLimit = 0;
        [SerializeField] float _caretOffset = 0.4f;

        public new enum SelectionState {

            Normal = 0,
            Highlighted = 1,
            Pressed = 2,
            Disabled = 3,
            Selected = 4,
        }

        public class InputFieldChanged : UnityEvent<InputFieldView> { }

        public InputFieldChanged onValueChanged { get => _onValueChanged; set => _onValueChanged = value; }
        public event System.Action<SelectionState> selectionStateDidChangeEvent;

        public SelectionState selectionState => _selectionState;
        public bool useGlobalKeyboard => _useGlobalKeyboard;
        public bool useSystemKeyboardIfAvailable => _useSystemKeyboardIfAvailable;
#if BS_TOURS
        public string globalKeyboardTitleLocalizationKey => _globalKeyboardTitleLocalizationKey;
        public bool fadeBackgroundScreens => _fadeBackgroundScreens;
#endif
        public Vector3 keyboardPositionOffset => _keyboardPositionOffset;

        public string text {
            get => _text;
            private set {
                _text = value;
                _textView.SetText(_text);
                _textView.ForceMeshUpdate(ignoreActiveState : true);
                UpdateCaretPosition();
                UpdatePlaceholder();
            }
        }

        private SelectionState _selectionState = SelectionState.Normal;
        private string _text = "";
        private bool _hasKeyboardAssigned = false;
        private ButtonBinder _buttonBinder;
        private InputFieldChanged _onValueChanged = new InputFieldChanged();
        private readonly YieldInstruction _blinkWaitYieldInstruction = new WaitForSeconds(kBlinkingRate);

        protected override void Awake() {

            _blinkingCaret.enabled = false;

            _buttonBinder = new ButtonBinder();
            _buttonBinder.AddBinding(
                _clearSearchButton,
                () => {
                    text = "";
                    _clearSearchButton.interactable = false;
                    _onValueChanged.Invoke(this);
                }
            );

            _clearSearchButton.interactable = false;
        }

        protected override void OnDestroy() {

            _buttonBinder?.ClearBindings();
        }

#if UNITY_EDITOR
        protected override void OnValidate() {

            base.OnValidate();
            transition = Selectable.Transition.None;
        }
#endif

        protected override void DoStateTransition(Selectable.SelectionState state, bool instant) {

            SelectionState thisState = SelectionState.Normal;
            if (_hasKeyboardAssigned) {
                thisState = SelectionState.Selected;
            }

            else {
                switch (state) {
                    case Selectable.SelectionState.Normal:
                        thisState = SelectionState.Normal;
                        break;
                    case Selectable.SelectionState.Highlighted:
                        thisState = SelectionState.Highlighted;
                        break;
                    case Selectable.SelectionState.Pressed:
                        thisState = SelectionState.Pressed;
                        break;
                    case Selectable.SelectionState.Disabled:
                        thisState = SelectionState.Disabled;
                        break;
                    case Selectable.SelectionState.Selected:
                        thisState = SelectionState.Selected;
                        break;
                }
            }
            _selectionState = thisState;
            selectionStateDidChangeEvent?.Invoke(thisState);
            UpdatePlaceholder();
        }

        public void ActivateKeyboard(UIKeyboard keyboard) {

            if (_hasKeyboardAssigned) {
                return;
            }

            _hasKeyboardAssigned = true;
            if (_textViewCanvasGroup != null) {
                _textViewCanvasGroup.ignoreParentGroups = true;
            }

            keyboard.keyWasPressedEvent += KeyboardKeyPressed;
            keyboard.deleteButtonWasPressedEvent += KeyboardDeletePressed;
            UpdateCaretPosition();
            _blinkingCaret.enabled = true;
            StopAllCoroutines();
            StartCoroutine(BlinkingCaretCoroutine());
            selectionStateDidChangeEvent?.Invoke(SelectionState.Selected);
            _clearSearchButton.interactable = false;
        }

        public void DeactivateKeyboard(UIKeyboard keyboard) {

            if (!_hasKeyboardAssigned) {
                return;
            }
            _hasKeyboardAssigned = false;
            if (_textViewCanvasGroup != null) {
                _textViewCanvasGroup.ignoreParentGroups = false;
            }

            keyboard.keyWasPressedEvent -= KeyboardKeyPressed;
            keyboard.deleteButtonWasPressedEvent -= KeyboardDeletePressed;
            StopAllCoroutines();
            _blinkingCaret.enabled = false;
            selectionStateDidChangeEvent?.Invoke(SelectionState.Normal);
            _selectionState = SelectionState.Normal;
            UpdateClearButton();
        }

        public void SetText(string value) {

            text = value;

            UpdateClearButton();
        }

        public void ClearInput() {

            text = "";
            UpdateClearButton();
        }

        private void KeyboardKeyPressed(char letter) {

            if (text.Length < _textLengthLimit) {
                text += _useUppercase ? char.ToUpper(letter) : letter;
                _onValueChanged.Invoke(this);
            }

            UpdatePlaceholder();
            _blinkingCaret.enabled = true;
            StopAllCoroutines();
            StartCoroutine(BlinkingCaretCoroutine());
        }

        private void KeyboardDeletePressed() {

            text = text.Length <= 0 ? "" : text.Substring(0, text.Length - 1);
            _onValueChanged.Invoke(this);
            UpdatePlaceholder();
            _blinkingCaret.enabled = true;
            StopAllCoroutines();
            StartCoroutine(BlinkingCaretCoroutine());
        }

        private IEnumerator BlinkingCaretCoroutine() {

            while (true) {
                yield return _blinkWaitYieldInstruction;
                _blinkingCaret.enabled = !_blinkingCaret.enabled;
            }
        }

        private void UpdateCaretPosition() {

            var textEndPosX = Mathf.Max(_textView.GetRenderedValues(onlyVisibleCharacters: false).x, 0.0f);
            var rect = (RectTransform)_blinkingCaret.transform;
            var pos = rect.anchoredPosition;
            pos.x = textEndPosX + ((_textView.text.Length > 0) ? _caretOffset : 0.0f);
            rect.anchoredPosition = pos;
        }

        private void UpdatePlaceholder() {

            _placeholderText.SetActive(string.IsNullOrEmpty(_text));
        }

        private void UpdateClearButton() {

            _clearSearchButton.interactable = !string.IsNullOrEmpty(_text);
        }
    }

}
