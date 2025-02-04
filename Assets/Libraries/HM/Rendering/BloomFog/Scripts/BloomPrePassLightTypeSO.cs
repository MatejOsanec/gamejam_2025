using UnityEngine;

public class BloomPrePassLightTypeSO : PersistentScriptableObject {

    [SerializeField] int _renderingPriority = 0;
    [SerializeField] Material _material = default;

    public int renderingPriority => _renderingPriority;
    public Material material => _material;
}