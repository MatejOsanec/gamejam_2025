using System.Threading.Tasks;
using UnityEngine;


public abstract class ScenesTransitionSetupDataSO : PersistentScriptableObject {

    public SceneInfo[] scenes { get; private set; }

    private SceneSetupData[] _sceneSetupDataArray;

    public event System.Action beforeScenesWillBeActivatedEvent;

    protected void Init(SceneInfo[] scenes, SceneSetupData[] sceneSetupData) {

        this.scenes = scenes;
        _sceneSetupDataArray = sceneSetupData;
    }

    public virtual void BeforeScenesWillBeActivated() {

        beforeScenesWillBeActivatedEvent?.Invoke();
    }

    public virtual Task BeforeScenesWillBeActivatedAsync() {

        return Task.Run(() => beforeScenesWillBeActivatedEvent?.Invoke());
    }

    public void InstallBindings(MonoBehaviour container) {

        if (_sceneSetupDataArray == null) {
            return;
        }

        foreach (var sceneSetupData in _sceneSetupDataArray) {
            if (sceneSetupData == null) {
                continue;
            }
            var type = sceneSetupData.GetType();
        }
    }
}
