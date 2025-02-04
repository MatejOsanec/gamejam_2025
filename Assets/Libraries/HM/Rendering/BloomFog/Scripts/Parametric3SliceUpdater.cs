using System;
using UnityEngine;

[ExecuteAlways]
public class Parametric3SliceUpdater : MonoBehaviour {

    [SerializeField] Parametric3SliceSpriteController _parametric3SliceSpriteController;
    private void Update() {
        _parametric3SliceSpriteController.Refresh();
    }
}

