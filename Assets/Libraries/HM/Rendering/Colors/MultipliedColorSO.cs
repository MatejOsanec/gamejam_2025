using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipliedColorSO : ColorSO {

    [SerializeField] SimpleColorSO _baseColor = default;
    [SerializeField] Color _multiplierColor = default;

    public override Color color => _multiplierColor * _baseColor.color;
}
