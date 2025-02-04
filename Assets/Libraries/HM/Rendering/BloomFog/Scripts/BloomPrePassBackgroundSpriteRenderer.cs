using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class BloomPrePassBackgroundSpriteRenderer : BloomPrePassBackgroundNonLightRendererCore {

    [SerializeField] SpriteRenderer _spriteRenderer = default;
    public override Renderer renderer => _spriteRenderer;

}
