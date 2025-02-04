using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteLightWithId : LightWithIdMonoBehaviour {

    [SerializeField] SpriteRenderer _spriteRenderer = default;

    [Space]
    [SerializeField] bool _hideIfAlphaOutOfRange = false;
    [SerializeField] [DrawIf("_hideIfAlphaOutOfRange", true)] float _hideAlphaRangeMin = 0.001f;
    [SerializeField] [DrawIf("_hideIfAlphaOutOfRange", true)] float _hideAlphaRangeMax = 1.0f;

    [Space]
    [SerializeField] float _intensity = 1.0f;
    [SerializeField] float _minAlpha = 0.0f;
    [SerializeField] MultiplyColorByAlphaType _multiplyColorByAlpha = MultiplyColorByAlphaType.None;
    [SerializeField, DrawIf("_setAlphaOnly", false)] bool _setColorOnly = false;
    [SerializeField, DrawIf("_setColorOnly", false)] bool _setAlphaOnly = false;
    [SerializeField] bool _setOnlyOnce = false;

    public Color color => _spriteRenderer.color;

    private enum MultiplyColorByAlphaType {
        None,
        BeforeApplyingMinAlpha,
        AfterApplyingMinAlpha
    }
    public override void ColorWasSet(Color color) {

        if (_multiplyColorByAlpha == MultiplyColorByAlphaType.BeforeApplyingMinAlpha) {
            color.r *= color.a;
            color.g *= color.a;
            color.b *= color.a;
        }
        
        color.a = _setColorOnly ? _spriteRenderer.color.a : Mathf.Max(color.a, _minAlpha);
        
        if (_multiplyColorByAlpha == MultiplyColorByAlphaType.AfterApplyingMinAlpha) {
            color.r *= color.a;
            color.g *= color.a;
            color.b *= color.a;
        }
        
        if (_setAlphaOnly) {
            _spriteRenderer.color = _spriteRenderer.color.ColorWithAlpha(color.a * _intensity);
        } 
        else {
            _spriteRenderer.color = color * _intensity;
        }

        if (_hideIfAlphaOutOfRange) {
            _spriteRenderer.enabled = color.a >= _hideAlphaRangeMin && color.a <= _hideAlphaRangeMax;
        }

        if (_setOnlyOnce) {
            enabled = false;
        }
    }
}
