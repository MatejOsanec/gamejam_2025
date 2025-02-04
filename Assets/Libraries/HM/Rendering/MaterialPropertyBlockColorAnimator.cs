using UnityEngine;

[ExecuteAlways]
public class MaterialPropertyBlockColorAnimator : MaterialPropertyBlockAnimator {

    [Space]
    [SerializeField] Color _color = Color.black;

    public Color color { get => _color; set => _color = value; }

    protected override void SetProperty() {

        _materialPropertyBlockController.materialPropertyBlock.SetColor(propertyId, _color);
    }
}
