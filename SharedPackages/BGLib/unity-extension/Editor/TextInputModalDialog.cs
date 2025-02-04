using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TextInputModalDialog : EditorWindow {

    public static bool Show(string title, string dialogText, Func<string, bool> validationFunction, out string resultText) {

        TextInputModalDialog window = CreateInstance<TextInputModalDialog>();
        window.titleContent = new GUIContent(title);
        window.minSize = new Vector2(300, 200);
        window.maxSize = new Vector2(300, 200);
        window.Initialize(dialogText, validationFunction);
        window.ShowModal();

        resultText = window.result;
        return window.isSuccessful;
    }

    public bool isSuccessful { get; private set; } = false;
    public string result { get; private set; }

    private string _dialogText;
    private Func<string, bool> _validationFunction;
    private bool _isResultValid = false;

    private Button _acceptButton;

    private void CreateGUI() {

        rootVisualElement.style.paddingTop = 4;
        rootVisualElement.style.paddingRight = 4;
        rootVisualElement.style.paddingBottom = 4;
        rootVisualElement.style.paddingLeft = 4;

        var label = new Label(_dialogText);
        label.style.whiteSpace = WhiteSpace.Normal;
        rootVisualElement.Add(label);

        var textField = new TextField();
        textField.RegisterCallback<ChangeEvent<string>>(OnTextFieldUpdated);
        rootVisualElement.Add(textField);

        var horizontalLayout = new VisualElement();
        horizontalLayout.style.flexDirection = FlexDirection.Row;
        rootVisualElement.Add(horizontalLayout);

        var cancelButton = new Button {
            text = "Cancel"
        };

        cancelButton.clicked += OnCancel;
        horizontalLayout.Add(cancelButton);

        _acceptButton = new Button {
            text = "Accept",
        };
        _acceptButton.clicked += OnAccept;
        _acceptButton.SetEnabled(_validationFunction == null);
        horizontalLayout.Add(_acceptButton);
    }

    private void Initialize(string dialogText,  Func<string, bool> validationFunction) {

        _dialogText = dialogText;
        _validationFunction = validationFunction;
    }

    private void OnTextFieldUpdated(ChangeEvent<string> evt) {

        result = evt.newValue;

        if (string.IsNullOrWhiteSpace(result)) {
            _isResultValid = false;
        }
        else if (_validationFunction == null) {
            _isResultValid = true;
        }
        else {
            _isResultValid = _validationFunction.Invoke(result);
        }

        _acceptButton.SetEnabled(_isResultValid);
    }

    private void OnCancel() {

        isSuccessful = false;
        Close();
    }

    private void OnAccept() {

        isSuccessful = _isResultValid;
        Close();
    }
}
