using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SignalOnUIButtonClick : MonoBehaviour {

    [SerializeField] [SignalSender] Signal _buttonClickedSignal = default;
    [SerializeField] Button _button = default;

    protected void Start() {

        _button.onClick.AddListener(_buttonClickedSignal.Raise);
    }

    protected void OnDestroy() {

        if (_button) {
            _button.onClick.RemoveListener(_buttonClickedSignal.Raise);
        }
    }
}
