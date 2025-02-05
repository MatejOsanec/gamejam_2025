using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomValueToShader : PersistentScriptableObject {

    private System.Random _random = new();
    private int _lastFrameNum = -1;

    
    private static readonly int _randomValueID = Shader.PropertyToID("_GlobalRandomValue");

    public void SetRandomSeed(int seed) {

        _random = new System.Random(seed);
    }

    public void SetRandomValueToShaders() {

        int frameCount = Time.frameCount;

        #if UNITY_EDITOR
        if (Application.isPlaying && _lastFrameNum == frameCount) {
            return;
        }
        #else
        if (_lastFrameNum == frameCount) {
            return;
        }
        #endif

        Shader.SetGlobalFloat(_randomValueID, (float)_random.NextDouble());
        _lastFrameNum = frameCount;
    }
}
