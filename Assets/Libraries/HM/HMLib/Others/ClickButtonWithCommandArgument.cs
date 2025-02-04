using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ClickButtonWithCommandArgument : MonoBehaviour {

    [SerializeField] string _argument = default;
    [SerializeField] Button _button = default;

    protected IEnumerator Start() {

        yield return null;

        var args = Environment.GetCommandLineArgs();
        foreach (var arg in args) {
            if (arg == _argument) {
                _button.onClick.Invoke();
            }
        }
    }
}
