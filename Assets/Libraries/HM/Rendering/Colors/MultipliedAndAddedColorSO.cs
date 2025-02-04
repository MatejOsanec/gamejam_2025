using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipliedAndAddedColorSO : ColorSO {

    [SerializeField] SimpleColorSO _baseColor = default;
    [SerializeField] Color _multiplierColor = default;
    [SerializeField] Color _addColor = default;

    public override Color color => _multiplierColor * _baseColor.color + _addColor;
}
