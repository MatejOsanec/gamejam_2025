using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleColorSO : ColorSO {

    [SerializeField] protected Color _color = default;

    public override Color color => _color;

    public void SetColor(Color c) {

        _color = c;
    }
}
