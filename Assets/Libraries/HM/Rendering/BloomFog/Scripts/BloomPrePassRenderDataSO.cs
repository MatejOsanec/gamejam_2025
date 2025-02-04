using UnityEngine;
using System;

public class BloomPrePassRenderDataSO : PersistentScriptableObject {

    public class Data {
        [NonSerialized] public RenderTexture bloomPrePassRenderTexture;
        [NonSerialized] public Vector2 textureToScreenRatio;
        [NonSerialized] public Matrix4x4 viewMatrix;
        [NonSerialized] public Matrix4x4 projectionMatrix;
        [NonSerialized] public float stereoCameraEyeOffset;
        [NonSerialized] public ToneMapping toneMapping;
    }

    public readonly Data data = new Data();
}
