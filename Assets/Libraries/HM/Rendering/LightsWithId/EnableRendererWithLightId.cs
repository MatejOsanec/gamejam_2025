using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableRendererWithLightId : LightWithIdMonoBehaviour {

    [SerializeField] Renderer _renderer = default;
    
    [SerializeField] float _hideAlphaRangeMin = 0.001f;
    [SerializeField] float _hideAlphaRangeMax = 1.0f;
    
    public override void ColorWasSet(Color color) {
        
        _renderer.enabled = color.a >= _hideAlphaRangeMin && color.a <= _hideAlphaRangeMax;
    }
}
