using UnityEngine;

public class ColorStyleSO : PersistentScriptableObject {

    [SerializeField] ColorStyle _colorStyle;

    public IReadOnlyColorStyle colorStyle => _colorStyle;
}
