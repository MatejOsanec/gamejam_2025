using System;
using System.Collections;
using UnityEngine;

namespace HMUI {

public class HoverTextController : MonoBehaviour {

    [SerializeField] TMPro.TextMeshProUGUI _textMesh = default;
    [SerializeField] float _fadeInDelay = 0.3f;
    [SerializeField] float _fadeInSpeed = 4.0f;
    [SerializeField] float _fadeOutSpeed = 2.0f;

    private bool _isFadingOut;
    private bool _isFadingIn;

    protected void Awake() {

        _textMesh.alpha = 0.0f;
    }

    protected void OnDisable() {

        _isFadingIn = false;
        _isFadingOut = false;
        _textMesh.alpha = 0.0f;
    }

    protected void OnApplicationFocus(bool hasFocus) {

        if (!hasFocus) {
            _textMesh.alpha = 0.0f;
        }
    }

    public void ShowText(string text) {

        _textMesh.text = text;

        _isFadingOut = false;
        if (_isFadingIn) {
            return;
        }

        StopAllCoroutines();
        StartCoroutine(ShowTextCoroutine());
    }

    private IEnumerator ShowTextCoroutine() {

        yield return new WaitForSeconds(_fadeInDelay);

        _isFadingIn = true;

        while (_textMesh.alpha < 0.99f) {
            yield return null;
            _textMesh.alpha += Time.deltaTime * _fadeInSpeed;
        }

        _textMesh.alpha = 1.0f;
        _isFadingIn = false;
    }

    public void HideText() {

        _isFadingIn = false;
        if (_isFadingOut) {
            return;
        }

        if (!isActiveAndEnabled) {
            _textMesh.alpha = 0.0f;
            return;
        }

        StopAllCoroutines();
        StartCoroutine(HideTextCoroutine());
    }

    private IEnumerator HideTextCoroutine() {

        _isFadingOut = true;

        while (_textMesh.alpha > 0.001f) {
            yield return null;
            _textMesh.alpha -= Time.deltaTime * _fadeOutSpeed;
        }

        _textMesh.alpha = 0.0f;
        _isFadingOut = false;
    }
}
}
