using System;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class ColorStyle : IReadOnlyColorStyle {

    [SerializeField] bool _useScriptableObjectColor;
    [SerializeField] Color _color = Color.white;
    [SerializeField] ColorSO _colorSo;

    [SerializeField] float _globalLightTintIntensity;

    [SerializeField] bool _gradient;
    [SerializeField] bool _useScriptableObjectGradientColors;
    [SerializeField] Color _color0 = Color.white;
    [SerializeField] Color _color1 = Color.white;
    [SerializeField] ColorSO _color0So;
    [SerializeField] ColorSO _color1So;
    [SerializeField] GradientDirection _gradientDirection = GradientDirection.Horizontal;
    [SerializeField] bool _flipGradientColors;

    public bool useScriptableObjectColor { get => _useScriptableObjectColor; set => _useScriptableObjectColor = value; }

    public Color color {
        get => _useScriptableObjectColor && _colorSo != null ? _colorSo : _color;
        set {
            Assert.IsFalse(_useScriptableObjectColor, "Setting color when useScriptableObjectColor is true is not allowed.");
            _color = value;
        }
    }

    public float globalLightTintIntensity { get => _globalLightTintIntensity; set => _globalLightTintIntensity = value; }

    public bool gradient { get => _gradient; set => _gradient = value; }

    public Color color0 {
        get => _useScriptableObjectGradientColors && _color0So != null ? _color0So : _color0;
        set {
            Assert.IsFalse(_useScriptableObjectGradientColors, "Setting gradient color when useScriptableObjectGradientColors is true is not allowed.");
            _color0 = value;
        }
    }

    public Color color1 {
        get => _useScriptableObjectGradientColors && _color1So != null ? _color1So : _color1;
        set {
            Assert.IsFalse(_useScriptableObjectGradientColors, "Setting gradient color when useScriptableObjectGradientColors is true is not allowed.");
            _color1 = value;
        }
    }

    public GradientDirection gradientDirection { get => _gradientDirection; set => _gradientDirection = value; }

    public bool flipGradientColors => _flipGradientColors;
}
