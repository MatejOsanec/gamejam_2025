using UnityEngine;

public class SingleFixedSceneScenesTransitionSetupDataSO : ScenesTransitionSetupDataSO {

    [SerializeField] SceneInfo _sceneInfo = default;

    public SceneInfo sceneInfo => _sceneInfo;

    protected void Init(SceneSetupData sceneSetupData) {

        Init(new SceneInfo[] { _sceneInfo }, new SceneSetupData[] { sceneSetupData });
    }
}
