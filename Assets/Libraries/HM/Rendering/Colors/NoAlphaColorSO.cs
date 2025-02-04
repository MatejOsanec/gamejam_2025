using UnityEngine;

public class NoAlphaColorSO : ColorSO {

    [SerializeField] [ColorUsage(showAlpha: false)] Color _color = default;

    public override Color color => _color.ColorWithAlpha(alpha: 1.0f);
}
