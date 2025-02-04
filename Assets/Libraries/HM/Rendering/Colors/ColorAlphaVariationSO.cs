using UnityEngine;

public class ColorAlphaVariationSO : ColorSO {

    [SerializeField] NoAlphaColorSO _baseColor;
    [SerializeField] AlphaSO _alpha;

    public override Color color => _baseColor.color.ColorWithAlpha(_alpha);

    public NoAlphaColorSO baseColor => _baseColor;
    public AlphaSO alpha => _alpha;

#if UNITY_EDITOR
    public void InitializeEditor(NoAlphaColorSO baseColor, AlphaSO alpha) {

        _baseColor = baseColor;
        _alpha = alpha;
    }
#endif
}
