using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloomPrePassEffectContainerSO : PersistentScriptableObject {

    [SerializeField] BloomPrePassEffectSO _bloomPrePassEffect = default;

    public BloomPrePassEffectSO bloomPrePassEffect => _bloomPrePassEffect;

    public void Init(BloomPrePassEffectSO bloomPrePassEffect) {
        
        _bloomPrePassEffect = bloomPrePassEffect;
    }
}
