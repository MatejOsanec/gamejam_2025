using UnityEngine;

public class FixedScenesScenesTransitionSetupDataSO : ScenesTransitionSetupDataSO {

    [SerializeField] SceneInfo[] _sceneInfos = default;

    public void Init() {
            
        Init(_sceneInfos, sceneSetupData: null);
    }
}