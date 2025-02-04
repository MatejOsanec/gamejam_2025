using UnityEngine;

public interface IReadOnlyColorStyle {

    public bool useScriptableObjectColor { get; }
    public Color color { get; }
    public float globalLightTintIntensity { get; }
    public bool gradient { get; }
    public Color color0 { get; }
    public Color color1 { get; }
    public GradientDirection gradientDirection { get; }
    public bool flipGradientColors { get; }
}
