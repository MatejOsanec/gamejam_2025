using UnityEngine;

public class BloomPrePassBackgroundNonLightDynamicRenderer: BloomPrePassBackgroundNonLightRendererCore {

    public override Renderer renderer => _renderer;

    private Renderer _renderer = default;

    protected override void OnEnable() {

        if (_renderer != null) {
            Register();
        }
    }

    public void SetRenderer(Renderer renderer) {

        _renderer = renderer;
        if (renderer != null) {
            Register();
        }
        else {
            Unregister();
        }
    }
}
