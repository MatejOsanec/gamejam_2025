using UnityEngine;

[ExecuteAlways, ImageEffectAllowedInSceneView]
public class ReflectionProbeBakingEffect: MonoBehaviour {

    [SerializeField] Material _material = default;

    protected void OnRenderImage(RenderTexture src, RenderTexture dest) {
        
        Graphics.Blit(src, dest, _material);
    }
}
