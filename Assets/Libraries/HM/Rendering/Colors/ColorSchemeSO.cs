using UnityEngine;

public class ColorSchemeSO : PersistentScriptableObject {

    [SerializeField] internal ColorScheme _colorScheme = default;
    [SerializeField] internal int _order = default;

    public ColorScheme colorScheme => _colorScheme;
    public int order => _order;

    [ContextMenu("Log Color Scheme")]
    private void LogColorScheme() {
        string export = $"{colorScheme.colorSchemeId} Color Scheme:\n";
        export += $"Left Saber Color: #{ColorUtility.ToHtmlStringRGB(colorScheme.saberAColor)}\n";
        export += $"Right Saber Color: #{ColorUtility.ToHtmlStringRGB(colorScheme.saberBColor)}\n";
        export += $"Environment Color 1: #{ColorUtility.ToHtmlStringRGB(colorScheme.environmentColor0)}\n";
        export += $"Environment Color 2: #{ColorUtility.ToHtmlStringRGB(colorScheme.environmentColor1)}\n";
        export += $"Environment Boost Color 1: #{ColorUtility.ToHtmlStringRGB(colorScheme.environmentColor0Boost)}\n";
        export += $"Environment Boost Color 2: #{ColorUtility.ToHtmlStringRGB(colorScheme.environmentColor1Boost)}\n";
        export += $"Obstacle Color: #{ColorUtility.ToHtmlStringRGB(colorScheme.obstaclesColor)}\n";
#if BS_TOURS
        export += $"Bombs Emission Color: #{ColorUtility.ToHtmlStringRGB(colorScheme.bombsEmissionColor)}\n";
#endif
        Debug.Log(export);
    }
}
