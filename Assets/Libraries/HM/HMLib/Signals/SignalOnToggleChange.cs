using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SignalOnToggleChange : MonoBehaviour {

    [SerializeField] [SignalSender] Signal _toggleValueChangeSignal = default;
    [SerializeField] Toggle _toggle = default;

    protected void Start() {

        _toggle.onValueChanged.AddListener(RaiseSignal);
    }

    protected void OnDestroy() {

        if (_toggle) {
            _toggle.onValueChanged.RemoveListener(RaiseSignal);
        }
    }

    private void RaiseSignal(bool newValue) {

        _toggleValueChangeSignal.Raise();
    }
}
