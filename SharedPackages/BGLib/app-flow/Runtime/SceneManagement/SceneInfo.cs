using UnityEngine;

//TODO: Rename to SceneInfoSO
public class SceneInfo : PersistentScriptableObject {

    [SerializeField] internal string _sceneName;
    [SerializeField] internal bool _disabledRootObjects;

    public string sceneName => _sceneName;
    public bool disabledRootObjects => _disabledRootObjects;
}
