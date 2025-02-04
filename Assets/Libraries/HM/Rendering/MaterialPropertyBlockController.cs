using System.Collections.Generic;
using UnityEngine;

public class MaterialPropertyBlockController : MonoBehaviour {

    [SerializeField] Renderer[] _renderers;

    public Renderer[] renderers => _renderers;

    public MaterialPropertyBlock materialPropertyBlock => _materialPropertyBlock ??= new MaterialPropertyBlock();

    private MaterialPropertyBlock _materialPropertyBlock;

    public void ApplyChanges() {

        for (int i = 0; i < _renderers.Length; i++) {
            _renderers[i].SetPropertyBlock(_materialPropertyBlock);
        }
    }

    public void SetRendererState(bool newState) {

        foreach (Renderer r in _renderers) {
            r.enabled = newState;
        }
    }
}
