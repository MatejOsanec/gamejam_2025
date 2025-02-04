using UnityEngine;

public class TextureEffectSO : PersistentScriptableObject {

    public virtual void Render(RenderTexture src, RenderTexture dest) {

        Graphics.Blit(src, dest);
    }
}
