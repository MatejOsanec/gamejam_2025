using UnityEngine;
using UnityEngine.Assertions;

public class MaterialController : MonoBehaviour {

    [SerializeField] Material _material;

    [Space]
    [SerializeField] Renderer[] _renderers;

    public Material material => _material;

    protected void OnValidate() {

        foreach (var r in _renderers) {
            Assert.AreEqual(r.sharedMaterial, _material, $"Materials are not equal for {r.name}");
        }
    }
}
