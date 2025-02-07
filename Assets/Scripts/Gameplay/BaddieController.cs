using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaddieController : MonoBehaviour
{
    [SerializeField] private Renderer _renderer;


    // Update is called once per frame
    public void SetMaterial(Material material)
    {
        if (_renderer != null)
        {
            _renderer.material = material;
        }
    }
}
