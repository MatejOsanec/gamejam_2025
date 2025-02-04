//#define GamesScenesManagerLogging

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Assertions;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using Zenject;
using UnityEngine.Scripting;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class GameScenesManager : MonoBehaviour {

    [SerializeField] SceneInfo _emptyTransitionSceneInfo = default;

    [Inject] readonly ZenjectSceneLoader _zenjectSceneLoader = default;

    public event System.Action<SceneTransitionType, float> transitionDidStartEvent;
    public event System.Action<List<string>> beforeDismissingScenesEvent;
    public event System.Action<SceneTransitionType, ScenesTransitionSetupDataSO, DiContainer> transitionDidFinishEvent;
    public event System.Action<ScenesTransitionSetupDataSO, DiContainer> installEarlyBindingsEvent;

    public const float kStandardTransitionLength = 0.7f;
    public const float kShortTransitionLength = 0.35f;
    public const float kLongTransitionLength = 1.3f;

    public DiContainer currentScenesContainer => _scenesStack.Last().container;
    public bool isInTransition => _currentSceneTransitionType != SceneTransitionType.None;
    public SceneTransitionType currentSceneTransitionType => _currentSceneTransitionType;
    public WaitUntil waitUntilSceneTransitionFinish => new WaitUntil(() => isInTransition == false);
    internal Dictionary<string, AsyncOperationHandle<SceneInstance>> sceneNameToSceneOperationHandlesDictionary => _sceneNameToSceneOperationHandlesDictionary;

    public enum SceneTransitionType {
        None,
        Push,
        Pop,
        Replace,
        ClearAndOpen,
        Append,
        Activate,
        Remove,
        Deactivate
    }

    private class ScenesStackData {

        public List<string> sceneNames { get; private set; }
        public DiContainer container { get; private set; }

        public ScenesStackData(List<string> sceneNames) {
            this.sceneNames = sceneNames;
        }

        public void SetDiContainer(DiContainer container) {
            Assert.IsNull(this.container, "DiContainer for ScenesStackData was set already.");
            this.container = container;
        }
    }

    private SceneTransitionType _currentSceneTransitionType = SceneTransitionType.None;
    private readonly List<ScenesStackData> _scenesStack = new List<ScenesStackData>();
    private readonly HashSet<string> _neverUnloadScenes = new HashSet<string>();
    private readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> _sceneNameToSceneOperationHandlesDictionary = new Dictionary<string, AsyncOperationHandle<SceneInstance>>();

    private const string kRootContainerGOName = "RootContainer";

    public void MarkSceneAsPersistent(string sceneName) {

        _neverUnloadScenes.Add(sceneName);
    }

    public List<string> GetCurrentlyLoadedSceneNames() {

        var sceneNames = new List<string>(SceneManager.sceneCount);

        for (int i = 0; i < SceneManager.sceneCount; i++) {
            var scene = SceneManager.GetSceneAt(i);

            if (!scene.name.Contains("InitTestScene")) {
                sceneNames.Add(scene.name);
            }
        }

        return sceneNames;
    }

    public void RegisterExternallyLoadedScene(string sceneName, AsyncOperationHandle<SceneInstance> asyncOperationHandle) {

        Assert.IsFalse(_sceneNameToSceneOperationHandlesDictionary.ContainsKey(sceneName));
        _sceneNameToSceneOperationHandlesDictionary[sceneName] = asyncOperationHandle;
    }

    public void LoadSingleScene(SceneInfo sceneInfo) {

        if (IsSceneInStack(sceneInfo.sceneName)) {
            return;
        }

        if (isInTransition) {
            Debug.LogWarning($"Failed loading scene {sceneInfo.name} during transition");
            return;
        }

        var sceneName = sceneInfo.sceneName;
        if (_scenesStack.Count == 0) {
            List<string> currentScenes = GetCurrentlyLoadedSceneNames();
            _scenesStack.Add(new ScenesStackData(currentScenes));
        }
        _scenesStack[_scenesStack.Count - 1].sceneNames.Add(sceneName);
        Log("Loading scene " + sceneName);
        StartCoroutine(LoadOneScene(sceneName));
    }

    public void UnloadSingleScene(SceneInfo sceneInfo) {

        if (!IsSceneInStack(sceneInfo.sceneName)) {
            return;
        }

        if (isInTransition) {
            Debug.LogWarning($"Failed unloading scene {sceneInfo.name} during transition");
            return;
        }

        RemoveSceneFromStack(sceneInfo.sceneName);
        List<string> currentlyLoadedScenes = GetCurrentlyLoadedSceneNames();

        if (currentlyLoadedScenes.Contains(sceneInfo.sceneName)) {
            StartCoroutine(UnloadOneScene(sceneInfo.sceneName));
        }
    }

    public bool IsSceneActiveOrLoading(SceneInfo sceneInfo) {

        return IsSceneInStack(sceneInfo.sceneName);
    }

    public void PushScenes(ScenesTransitionSetupDataSO scenesTransitionSetupData, float minDuration = 0.0f, System.Action afterMinDurationCallback = null, System.Action<DiContainer> finishCallback = null) {

        if (isInTransition) {
            return;
        }

        _currentSceneTransitionType = SceneTransitionType.Push;

        Log($"PUSH - {scenesTransitionSetupData.name}");

        transitionDidStartEvent?.Invoke(SceneTransitionType.Push, minDuration);

        var newSceneNames = SceneNamesFromSceneInfoArray(scenesTransitionSetupData.scenes);
        Assert.IsFalse(IsAnySceneInStack(newSceneNames));

        List<string> currentScenes;

        if (_scenesStack.Count > 0) {
            currentScenes = _scenesStack[^1].sceneNames;
        }
        // If the stack is empty, we add currently loaded scenes.
        else {
            currentScenes = GetCurrentlyLoadedSceneNames();
            _scenesStack.Add(new ScenesStackData(currentScenes));
        }

        var scenesStackData = new ScenesStackData(newSceneNames.ToList());
        _scenesStack.Add(scenesStackData);

        StartCoroutine(
            ScenesTransitionCoroutine(
                newScenesTransitionSetupData: scenesTransitionSetupData,
                scenesToPresent: newSceneNames,
                presentType: ScenePresentType.Load,
                scenesToDismiss: currentScenes,
                dismissType: SceneDismissType.Deactivate,
                minDuration: minDuration,
                afterMinDurationCallback: afterMinDurationCallback,
                canTriggerGarbageCollector: true,
                extraBindingsCallback: (container) => {
                    scenesStackData.SetDiContainer(container);
                    scenesTransitionSetupData.InstallBindings(container);
                    installEarlyBindingsEvent?.Invoke(scenesTransitionSetupData, container);
                },
                finishCallback: (container) => {
                    Log("Transition Finished");
                    _currentSceneTransitionType = SceneTransitionType.None;
                    transitionDidFinishEvent?.Invoke(SceneTransitionType.Push, scenesTransitionSetupData, scenesStackData.container);
                    finishCallback?.Invoke(container);
                }
            )
        );
    }

    public void PopScenes(float minDuration = 0.0f, System.Action afterMinDurationCallback = null, System.Action<DiContainer> finishCallback = null) {

        if (isInTransition) {
            return;
        }

        _currentSceneTransitionType = SceneTransitionType.Pop;

        Log("POP");

        transitionDidStartEvent?.Invoke(SceneTransitionType.Pop, minDuration);

        Assert.IsTrue(_scenesStack.Count > 1);

        List<string> currentScenes = _scenesStack[^1].sceneNames;
        List<string> prevScenes = _scenesStack[^2].sceneNames;
        _scenesStack.RemoveAt(_scenesStack.Count - 1);

        StartCoroutine(
            ScenesTransitionCoroutine(
                newScenesTransitionSetupData: null,
                scenesToPresent: prevScenes,
                presentType: ScenePresentType.Activate,
                scenesToDismiss: currentScenes,
                dismissType: SceneDismissType.Unload,
                minDuration: minDuration,
                afterMinDurationCallback: afterMinDurationCallback,
                extraBindingsCallback: null,
                canTriggerGarbageCollector: true,
                finishCallback: (container) => {
                    Log("Transition Finished");
                    _currentSceneTransitionType = SceneTransitionType.None;
                    transitionDidFinishEvent?.Invoke(SceneTransitionType.Pop, null, _scenesStack.Last().container);
                    finishCallback?.Invoke(container);
                }
            )
        );
    }

    public void ReplaceScenes(
        ScenesTransitionSetupDataSO scenesTransitionSetupData,
        IEnumerator[] beforeNewScenesActivateRoutines = null,
        float minDuration = 0.0f,
        System.Action afterMinDurationCallback = null,
        System.Action<DiContainer> finishCallback = null
    ) {

        if (isInTransition) {
            return;
        }

        _currentSceneTransitionType = SceneTransitionType.Replace;

        Log($"REPLACE - {scenesTransitionSetupData.name}");

        transitionDidStartEvent?.Invoke(SceneTransitionType.Replace, minDuration);

        var newSceneNames = SceneNamesFromSceneInfoArray(scenesTransitionSetupData.scenes);

        var scenesStackData = new ScenesStackData(newSceneNames);

        List<string> currentScenes;

        if (_scenesStack.Count > 0) {
            currentScenes = _scenesStack[^1].sceneNames;
            _scenesStack[^1] = scenesStackData;
        }
        // If the stack is empty, we add currently loaded scenes.
        else {
            currentScenes = GetCurrentlyLoadedSceneNames();
            _scenesStack.Add(scenesStackData);
        }

        var emptyTransitionSceneNameList = new List<string> { _emptyTransitionSceneInfo.sceneName };

        StartCoroutine(
            ScenesTransitionCoroutine(
                newScenesTransitionSetupData: null,
                scenesToPresent: emptyTransitionSceneNameList,
                presentType: ScenePresentType.Load,
                scenesToDismiss: currentScenes,
                dismissType: SceneDismissType.Unload,
                minDuration: minDuration,
                afterMinDurationCallback: afterMinDurationCallback,
                extraBindingsCallback: null,
                canTriggerGarbageCollector: false,
                finishCallback: (emptySceneContainer) => {
                    StartCoroutine(
                        ScenesTransitionCoroutine(
                            newScenesTransitionSetupData: scenesTransitionSetupData,
                            scenesToPresent: newSceneNames,
                            presentType: ScenePresentType.Load,
                            scenesToDismiss: emptyTransitionSceneNameList,
                            dismissType: SceneDismissType.Unload,
                            minDuration: 0.0f,
                            afterMinDurationCallback: null,
                            canTriggerGarbageCollector: true,
                            extraBindingsCallback: (container) => {
                                scenesStackData.SetDiContainer(container);
                                scenesTransitionSetupData.InstallBindings(container);
                                installEarlyBindingsEvent?.Invoke(scenesTransitionSetupData, container);
                            },
                            finishCallback: (container) => {
                                Log("Transition Finished");
                                _currentSceneTransitionType = SceneTransitionType.None;
                                transitionDidFinishEvent?.Invoke(SceneTransitionType.Replace, scenesTransitionSetupData, scenesStackData.container);
                                finishCallback?.Invoke(container);
                            }
                        )
                    );
                }
            )
        );
    }

    public void ClearAndOpenScenes(ScenesTransitionSetupDataSO scenesTransitionSetupData, float minDuration = 0.0f, System.Action afterMinDurationCallback = null, System.Action<DiContainer> finishCallback = null, bool unloadAllScenes = true) {

        if (isInTransition) {
            return;
        }

        _currentSceneTransitionType = SceneTransitionType.ClearAndOpen;

        Log($"CLEAR AND OPEN - {scenesTransitionSetupData.name}");

        transitionDidStartEvent?.Invoke(SceneTransitionType.ClearAndOpen, minDuration);

        var newSceneNames = SceneNamesFromSceneInfoArray(scenesTransitionSetupData.scenes);

        List<string> allCurrentScenes;

        if (_scenesStack.Count > 0) {
            allCurrentScenes = new List<string>();

            foreach (var scenes in _scenesStack) {
                allCurrentScenes.AddRange(scenes.sceneNames);
            }
        }
        else {
            allCurrentScenes = GetCurrentlyLoadedSceneNames();
        }

        if (unloadAllScenes) {
            // Remove also all scenes marked as un-loadable.
            foreach (var neverUnloadScene in _neverUnloadScenes) {
                allCurrentScenes.Add(neverUnloadScene);
            }

            _neverUnloadScenes.Clear();
        }

        _scenesStack.Clear();
        var scenesStackData = new ScenesStackData(newSceneNames.ToList());
        _scenesStack.Add(scenesStackData);

        var emptyTransitionSceneNameList = new List<string> { _emptyTransitionSceneInfo.sceneName };

        StartCoroutine(
            ScenesTransitionCoroutine(
                newScenesTransitionSetupData: null,
                scenesToPresent: emptyTransitionSceneNameList,
                presentType: ScenePresentType.Load,
                scenesToDismiss: allCurrentScenes,
                dismissType: SceneDismissType.Unload,
                minDuration: minDuration,
                afterMinDurationCallback: afterMinDurationCallback,
                extraBindingsCallback: null,
                canTriggerGarbageCollector: false,
                finishCallback: (emptySceneContainer) => {
                    StartCoroutine(
                        ScenesTransitionCoroutine(
                            newScenesTransitionSetupData: scenesTransitionSetupData,
                            scenesToPresent: newSceneNames,
                            presentType: ScenePresentType.Load,
                            scenesToDismiss: emptyTransitionSceneNameList,
                            dismissType: SceneDismissType.Unload,
                            minDuration: 0.0f,
                            afterMinDurationCallback: null,
                            canTriggerGarbageCollector: true,
                            extraBindingsCallback: (container) => {
                                scenesStackData.SetDiContainer(container);
                                scenesTransitionSetupData.InstallBindings(container);
                                installEarlyBindingsEvent?.Invoke(scenesTransitionSetupData, container);
                            },
                            finishCallback: (container) => {
                                Log("Transition Finished");
                                _currentSceneTransitionType = SceneTransitionType.None;
                                transitionDidFinishEvent?.Invoke(SceneTransitionType.ClearAndOpen, scenesTransitionSetupData, scenesStackData.container);
                                finishCallback?.Invoke(container);
                            }
                        )
                    );
                }
            )
        );
    }

    public void AppendScenes(ScenesTransitionSetupDataSO scenesTransitionSetupData, bool activateScenes = true, float minDuration = 0.0f, System.Action afterMinDurationCallback = null, System.Action<DiContainer> finishCallback = null) {

        if (isInTransition) {
            return;
        }
        _currentSceneTransitionType = SceneTransitionType.Append;

        Log($"PUSH - {scenesTransitionSetupData.name}");

        transitionDidStartEvent?.Invoke(SceneTransitionType.Append, minDuration);

        List<string> currentScenes;
        if (_scenesStack.Count > 0) {
            currentScenes = _scenesStack[^1].sceneNames;
        }
        else {
            currentScenes = GetCurrentlyLoadedSceneNames();
            _scenesStack.Add(new ScenesStackData(currentScenes));
        }

        var newSceneNames = SceneNamesFromSceneInfoArray(scenesTransitionSetupData.scenes);
        Assert.IsFalse(IsAnySceneInStack(newSceneNames), "Attempted to append a scene that is already loaded");

        var scenesToStack = new List<string>(currentScenes);
        scenesToStack.AddRange(newSceneNames);

        var scenesStackData = new ScenesStackData(scenesToStack);
        _scenesStack.Add(scenesStackData);

        StartCoroutine(
            ScenesTransitionCoroutine(
                newScenesTransitionSetupData: scenesTransitionSetupData,
                scenesToPresent: newSceneNames,
                presentType: activateScenes ? ScenePresentType.Load : ScenePresentType.LoadAndDoNotActivate,
                scenesToDismiss: new List<string>(),
                dismissType: SceneDismissType.DoNotUnload,
                minDuration: minDuration,
                afterMinDurationCallback: null,
                canTriggerGarbageCollector: false,
                extraBindingsCallback: container => {
                    scenesStackData.SetDiContainer(container);
                    scenesTransitionSetupData.InstallBindings(container);
                    installEarlyBindingsEvent?.Invoke(scenesTransitionSetupData, container);
                },
                finishCallback: container => {
                    Log("Transition Finished");
                    _currentSceneTransitionType = SceneTransitionType.None;
                    transitionDidFinishEvent?.Invoke(SceneTransitionType.Append, scenesTransitionSetupData, scenesStackData.container);
                    finishCallback?.Invoke(container);
                }
            )
        );
    }

    public void RemoveScenes(ScenesTransitionSetupDataSO scenesTransitionSetupDataSo, float minDuration = 0.0f, System.Action afterMinDurationCallback = null, System.Action<DiContainer> finishCallback = null) {

        if (isInTransition) {
            return;
        }
        _currentSceneTransitionType = SceneTransitionType.Remove;

        Log("POP");

        transitionDidStartEvent?.Invoke(SceneTransitionType.Remove, minDuration);

        Assert.IsTrue(_scenesStack.Count > 1, "Attempted to remove scene from the stack while there were none");

        var sceneNamesToRemove = SceneNamesFromSceneInfoArray(scenesTransitionSetupDataSo.scenes);
        Assert.IsTrue(AreAllScenesInStack(sceneNamesToRemove));

        var currentScenes = _scenesStack[_scenesStack.Count - 1].sceneNames;
        var prevScenes = _scenesStack[_scenesStack.Count - 2].sceneNames;
        _scenesStack.RemoveAt(_scenesStack.Count - 1);

        StartCoroutine(
            ScenesTransitionCoroutine(
                newScenesTransitionSetupData: null,
                scenesToPresent: new List<string>(),
                presentType: ScenePresentType.DoNotLoad,
                scenesToDismiss: sceneNamesToRemove,
                dismissType: SceneDismissType.Unload,
                minDuration: minDuration,
                afterMinDurationCallback: afterMinDurationCallback,
                extraBindingsCallback: null,
                canTriggerGarbageCollector: false,
                finishCallback: container => {
                    _currentSceneTransitionType = SceneTransitionType.None;
                    transitionDidFinishEvent?.Invoke(SceneTransitionType.Remove, null, _scenesStack.Last().container);
                    finishCallback?.Invoke(container);
                }
            )
        );

        var bisectedCurrentScenes = currentScenes.Where(scene => !sceneNamesToRemove.Contains(scene)).ToList();

        // Removed scenes are not equal to what should be loaded, push new scenes to stack
        if (!prevScenes.SequenceEqual(bisectedCurrentScenes)) {
            _scenesStack.Add(new ScenesStackData(bisectedCurrentScenes));
        }
    }

    public void ActivateScenes(ScenesTransitionSetupDataSO scenesTransitionSetupData, float minDuration = 0.0f, System.Action afterMinDurationCallback = null, System.Action<DiContainer> finishCallback = null) {

        if (isInTransition) {
            return;
        }
        _currentSceneTransitionType = SceneTransitionType.Activate;

        Log($"ACTIVATE - {scenesTransitionSetupData.name}");

        transitionDidStartEvent?.Invoke(SceneTransitionType.Activate, minDuration);

        var newSceneNames = SceneNamesFromSceneInfoArray(scenesTransitionSetupData.scenes);
        Assert.IsTrue(AreAllScenesInStack(newSceneNames));

        StartCoroutine(
            ScenesTransitionCoroutine(
                newScenesTransitionSetupData: scenesTransitionSetupData,
                scenesToPresent: newSceneNames,
                presentType: ScenePresentType.Activate,
                scenesToDismiss: new List<string>(),
                dismissType: SceneDismissType.DoNotUnload,
                minDuration: minDuration,
                afterMinDurationCallback: null,
                extraBindingsCallback: null,
                canTriggerGarbageCollector: false,
                finishCallback: container => {
                    Log("Transition Finished");
                    _currentSceneTransitionType = SceneTransitionType.None;
                    transitionDidFinishEvent?.Invoke(SceneTransitionType.Activate, scenesTransitionSetupData, container);
                    finishCallback?.Invoke(container);
                }
            )
        );
    }

    public void DeactivateScenes(ScenesTransitionSetupDataSO scenesTransitionSetupData, float minDuration = 0.0f, System.Action afterMinDurationCallback = null, System.Action<DiContainer> finishCallback = null) {

        if (isInTransition) {
            return;
        }
        _currentSceneTransitionType = SceneTransitionType.Deactivate;

        Log($"DEACTIVATE - {scenesTransitionSetupData.name}");

        transitionDidStartEvent?.Invoke(SceneTransitionType.Deactivate, minDuration);

        var sceneToDeactivateNames = SceneNamesFromSceneInfoArray(scenesTransitionSetupData.scenes);
        Assert.IsTrue(AreAllScenesInStack(sceneToDeactivateNames));

        StartCoroutine(
            ScenesTransitionCoroutine(
                newScenesTransitionSetupData: scenesTransitionSetupData,
                scenesToPresent: new List<string>(),
                presentType: ScenePresentType.Activate,
                scenesToDismiss: sceneToDeactivateNames,
                dismissType: SceneDismissType.Deactivate,
                minDuration: minDuration,
                afterMinDurationCallback: null,
                extraBindingsCallback: null,
                canTriggerGarbageCollector: false,
                finishCallback: container => {
                    Log("Transition Finished");
                    _currentSceneTransitionType = SceneTransitionType.None;
                    transitionDidFinishEvent?.Invoke(SceneTransitionType.Deactivate, scenesTransitionSetupData, container);
                    finishCallback?.Invoke(container);
                }
            )
        );
    }

    private enum ScenePresentType {
        DoNotLoad,
        Load,
        LoadAndDoNotActivate,
        Activate,
    }

    private enum SceneDismissType {
        DoNotUnload,
        Unload,
        Deactivate,
    }

    private IEnumerator LoadOneScene(string sceneName) {

        var loadSceneOperation = _zenjectSceneLoader.LoadSceneFromAddressablesAsync(
            sceneName,
            LoadSceneMode.Additive,
            activateOnLoad: true,
            priority: int.MaxValue,
            extraBindingsEarly: null,
            extraBindings: null,
            containerMode: LoadSceneRelationship.None,
            extraBindingsLate: null
        );

        _sceneNameToSceneOperationHandlesDictionary[sceneName] = loadSceneOperation;
        yield return loadSceneOperation;
        Log("Scene loaded " + sceneName);
    }

    private IEnumerator UnloadOneScene(string sceneName) {

        if (!_sceneNameToSceneOperationHandlesDictionary.TryGetValue(sceneName, out var sceneOperationHandle)) {
            yield break;
        }
        var unloadSceneOperation = Addressables.UnloadSceneAsync(sceneOperationHandle);
        yield return unloadSceneOperation;
    }

    private IEnumerator ScenesTransitionCoroutine(
        ScenesTransitionSetupDataSO newScenesTransitionSetupData,
        List<string> scenesToPresent,
        ScenePresentType presentType,
        List<string> scenesToDismiss,
        SceneDismissType dismissType,
        float minDuration,
        bool canTriggerGarbageCollector,
        System.Action afterMinDurationCallback,
        System.Action<DiContainer> extraBindingsCallback,
        System.Action<DiContainer> finishCallback
    ) {

        // Remove persistent scenes from scenesToDismiss
        scenesToDismiss = scenesToDismiss.Except(_neverUnloadScenes).ToList();
        scenesToPresent = scenesToPresent.Except(_neverUnloadScenes).ToList();

        // We need to watch for new ES during loading and disable them. There can be multiple.
        // Otherwise, bugs like [USS-2126](https://beatgames.atlassian.net/browse/USS-2126) can happen.
        List<EventSystem> allEventSystems = new List<EventSystem>();
        BackupToListAndDisableCurrentEventSystem(ref allEventSystems);

        // Loading scenes can cause lag spikes, this gives scripts listening to transitionDidStartEvent time to hide this (eg fade to black)
        yield return new WaitForSeconds(minDuration);

        // Presenting scenes.
        // Enable objects in presented scenes.
        if (presentType == ScenePresentType.Activate) {

            afterMinDurationCallback?.Invoke();

            if (scenesToPresent.Count > 0) {
                var activeScene = SceneManager.GetSceneByName(scenesToPresent[^1]);
                SceneManager.SetActiveScene(activeScene);
                foreach (var sceneName in scenesToPresent) {
                    MoveGameObjectsFromContainerToSceneRoot(sceneName);
                }

                // We might have enabled a new event system and need to disable it
                BackupToListAndDisableCurrentEventSystem(ref allEventSystems);
            }

        }
        // Load Scenes.
        else if (presentType == ScenePresentType.Load || presentType == ScenePresentType.LoadAndDoNotActivate) {

            Task beforeLoadTask = null;
            if (newScenesTransitionSetupData != null) {
                beforeLoadTask = newScenesTransitionSetupData.BeforeScenesWillBeActivatedAsync();
            }

            // Loading one scene.
            if (scenesToPresent.Count == 1) {

                var sceneName = scenesToPresent[0];
                Log("Loading scene " + sceneName);

                AsyncOperationHandle<SceneInstance> loadSceneOperationHandle = _zenjectSceneLoader.LoadSceneFromAddressablesAsync(
                    sceneName,
                    LoadSceneMode.Additive,
                    activateOnLoad: false,
                    priority: int.MaxValue,
                    extraBindingsEarly: extraBindingsCallback,
                    extraBindings: null,
                    extraBindingsLate: null,
                    containerMode: LoadSceneRelationship.None
                );
                _sceneNameToSceneOperationHandlesDictionary[sceneName] = loadSceneOperationHandle;

                if (beforeLoadTask != null) {
                    yield return WaitUntilTaskCompleted(beforeLoadTask);
                }

                afterMinDurationCallback?.Invoke();

                yield return loadSceneOperationHandle;
                Assert.AreEqual(loadSceneOperationHandle.Status, AsyncOperationStatus.Succeeded);
                yield return loadSceneOperationHandle.Result.ActivateAsync();

                Log("Scene loaded " + sceneName);

                SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

                ActivatePresentedSceneRootObjects(new List<string> { sceneName });

                if (presentType == ScenePresentType.LoadAndDoNotActivate) {
                    ReparentRootGameObjectsToDisabledGameObject(sceneName);
                }
                else {
                    // We might have enabled a new event system and need to disable it
                    BackupToListAndDisableCurrentEventSystem(ref allEventSystems);
                }
            }
            // Loading multiple scenes.
            else {

                if (beforeLoadTask != null) {
                    yield return WaitUntilTaskCompleted(beforeLoadTask);
                }

                int sceneNum = 0;

                foreach (var sceneName in scenesToPresent) {

                    var scene = SceneManager.GetSceneByName(sceneName);
                    Assert.IsFalse(scene.isLoaded);
                    AsyncOperationHandle<SceneInstance> loadSceneOperation;

                    if (sceneNum == 0) {
                        loadSceneOperation = _zenjectSceneLoader.LoadSceneFromAddressablesAsync(
                            sceneName,
                            LoadSceneMode.Additive,
                            activateOnLoad: true,
                            priority: int.MaxValue,
                            extraBindingsEarly: extraBindingsCallback,
                            extraBindings: null,
                            extraBindingsLate: null,
                            containerMode: LoadSceneRelationship.None
                        );
                    }
                    else {
                        loadSceneOperation = Addressables.LoadSceneAsync(
                            sceneName,
                            LoadSceneMode.Additive,
                            activateOnLoad: true,
                            priority: int.MaxValue
                        );
                    }
                    _sceneNameToSceneOperationHandlesDictionary[sceneName] = loadSceneOperation;

                    Log("Loading scene " + sceneName);
                    yield return loadSceneOperation;

                    Log("Scene loaded " + sceneName);
                    sceneNum++;
                }

                afterMinDurationCallback?.Invoke();

                // Last loaded scene should be active.
                var activeScene = SceneManager.GetSceneByName(scenesToPresent[^1]);
                SceneManager.SetActiveScene(activeScene);

                if (presentType != ScenePresentType.LoadAndDoNotActivate) {
                    ActivatePresentedSceneRootObjects(scenesToPresent);
                    // We might have enabled a new event system
                    BackupToListAndDisableCurrentEventSystem(ref allEventSystems);
                }
            }
        }

        beforeDismissingScenesEvent?.Invoke(scenesToDismiss);

        // Dismissing scenes.
        if (dismissType == SceneDismissType.Deactivate) {
            foreach (var sceneName in scenesToDismiss) {
                ReparentRootGameObjectsToDisabledGameObject(sceneName);
            }
        }
        else if (dismissType == SceneDismissType.Unload) {
            SetActiveRootObjectsInScenes(scenesToDismiss, false);

            foreach (var dismissScene in scenesToDismiss) {
                Log("Unloading Scene " + dismissScene);
                // A bit tricky, we don't have this scene OperationHandle, because if was loaded by Zenject automatically in Editor
                // as parent contract dependency
                if (!_sceneNameToSceneOperationHandlesDictionary.TryGetValue(dismissScene, out var sceneOperationHandleToDismiss)) {
                    Debug.LogWarning($"Unable to find operation handle for loaded {dismissScene}, this should only happen in Unity Editor if started from other than GameLoader scene");
                    var scene =  SceneManager.GetSceneByName(dismissScene);
                    yield return SceneManager.UnloadSceneAsync(scene);
                }
                else {
                    var unloadSceneOperation = Addressables.UnloadSceneAsync(sceneOperationHandleToDismiss);
                    _sceneNameToSceneOperationHandlesDictionary[dismissScene] = unloadSceneOperation;
                    yield return unloadSceneOperation;
                    _sceneNameToSceneOperationHandlesDictionary.Remove(dismissScene);
                }

                Log("Scene unloaded " + dismissScene);
            }

            if (canTriggerGarbageCollector && ShouldUnloadUnusedAssets(scenesToDismiss)) {

                yield return Resources.UnloadUnusedAssets();
            }
        }

        if (canTriggerGarbageCollector) {
            System.GC.Collect();
            // If we don't have yield here, we can get race conditions.
            // The yield postponed finishCallback by at least one frame (usually about 100+ms)
            // and without it we get race condition with Awakes and Starts not being done yet.
            yield return null;
        }

        Random.InitState(0);

        // Re-enable eventSystems we've disabled. If we do it sooner, there is a race condition.
        // Because GC/unload delays finishCallback, but the scene is already enabled.
        foreach (EventSystem eventSystem in allEventSystems) {
            if (eventSystem == null) {
                continue;
            }
            eventSystem.enabled = true;
        }
        finishCallback?.Invoke(_scenesStack[^1].container);
    }

    private bool ShouldUnloadUnusedAssets(List<string> scenesToDismiss) {

        // We skip unload if secenesToDismiss is empty, or contains only EmptyTransition
        bool isOnlyEmptyTransition = scenesToDismiss.Count == 1 && scenesToDismiss[0] == _emptyTransitionSceneInfo.sceneName;
        return scenesToDismiss.Count > 0 && !isOnlyEmptyTransition;
    }

    private void BackupToListAndDisableCurrentEventSystem(ref List<EventSystem> list) {

        EventSystem current = EventSystem.current;

        if (current != null && !list.Contains(current)) {
            list.Add(current);
            current.enabled = false;
        }
    }

    private static IEnumerator WaitUntilTaskCompleted(Task task) {

        yield return new WaitUntil(() => task.IsCompleted);
        if (task.IsFaulted) {
            Debug.LogException(
                task.Exception?.InnerException ?? new Exception("BeforeScenesWillBeActivatedAsync execution failed")
            );
        }
        else if (task.IsCanceled) {
            Debug.LogWarning(
                "BeforeScenesWillBeActivatedAsync execution was cancelled, this is suspicious"
            );
        }
    }

    private static void ActivatePresentedSceneRootObjects(List<string> scenesToPresent) {

        var presentedScenesRootObjects = new List<GameObject>();
        foreach (var scene in scenesToPresent) {
            var rootObjects = SceneManager.GetSceneByName(scene).GetRootGameObjects();
            presentedScenesRootObjects.AddRange(rootObjects);
        }

        Random.InitState(0);
        foreach (var go in presentedScenesRootObjects) {
            go.SetActive(true);
        }
    }

    private bool IsAnySceneInStack(List<string> sceneNames) {

        foreach (var sceneName in sceneNames) {
            if (IsSceneInStack(sceneName)) {
                return true;
            }
        }

        return false;
    }

    private bool AreAllScenesInStack(List<string> sceneNames) {

        foreach (var sceneName in sceneNames) {
            if (!IsSceneInStack(sceneName)) {
                return false;
            }
        }
        return true;
    }

    private bool IsSceneInStack(string searchSceneName) {

        foreach (var scenesStackData in _scenesStack) {
            foreach (var sceneName in scenesStackData.sceneNames) {
                if (sceneName == searchSceneName) {
                    return true;
                }
            }
        }

        return false;
    }

    private void RemoveSceneFromStack(string sceneName) {

        foreach (var scenesStackData in _scenesStack) {
            scenesStackData.sceneNames.Remove(sceneName);
        }
    }

    private List<string> SceneNamesFromSceneInfoArray(SceneInfo[] sceneInfos) {

        var sceneNames = new List<string>(sceneInfos.Length);

        foreach (var sceneInfo in sceneInfos) {
            sceneNames.Add(sceneInfo.sceneName);
        }

        return sceneNames;
    }

    private void SetActiveRootObjectsInScenes(List<string> sceneNames, bool value) {

        foreach (var sceneName in sceneNames) {
            if (value) {
                Log("Activating objects in scene " + sceneName);
            }
            else {
                Log("Deactivating objects in scene " + sceneName);
            }

            var scene = SceneManager.GetSceneByName(sceneName);
            UnityScenesHelper.SetActiveRootObjectsInScene(scene, value);
        }
    }

    private void ReparentRootGameObjectsToDisabledGameObject(string sceneName) {

        Log("Re-parenting root game objects to disabled game object." + sceneName);

        var scene = SceneManager.GetSceneByName(sceneName);
        var parentGO = new GameObject(kRootContainerGOName);

        SceneManager.MoveGameObjectToScene(parentGO, scene);

        var parentGOTransform = parentGO.transform;
        var rootGameObjects = new List<GameObject>(scene.rootCount);
        scene.GetRootGameObjects(rootGameObjects);

        foreach (GameObject go in rootGameObjects) {
            go.transform.SetParent(parentGOTransform, worldPositionStays:false);
        }

        parentGO.SetActive(false);
    }

    private void MoveGameObjectsFromContainerToSceneRoot(string sceneName) {

        Log("Reactivating game objects." + sceneName);

        var scene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(scene);
        var rootGameObjects = new List<GameObject>(scene.rootCount);
        scene.GetRootGameObjects(rootGameObjects);

        Assert.IsTrue(rootGameObjects.Count == 1 && rootGameObjects[0].name == kRootContainerGOName, "There must be exactly one root container game object");

        var containerTransforms = rootGameObjects[0].transform;
        var childGOTransforms = new List<Transform>(containerTransforms.childCount);

        for (int i = 0; i < containerTransforms.childCount; i++) {
            childGOTransforms.Add(containerTransforms.GetChild(i));
        }

        foreach (var childTransform in childGOTransforms) {
            childTransform.SetParent(parent:null, worldPositionStays:false);
        }

        Destroy(rootGameObjects[0]);
    }

    [Conditional("GamesScenesManagerLogging")]
    private static void Log(string message) {

        UnityEngine.Debug.Log(message);
    }
}
