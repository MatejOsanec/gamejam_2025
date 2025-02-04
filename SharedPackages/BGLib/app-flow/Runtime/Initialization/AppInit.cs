using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using BGLib.AppFlow.Initialization;
using UnityEngine.Rendering;
using Zenject;
using static AppInitScenesTransitionSetupDataSO.AppInitOverrideStartType;

public abstract class AppInit : MonoInstaller {

    [SerializeField] GameObject _cameraGO = default;
    [SerializeField] AsyncSceneContext _asyncSceneContext;
    [InjectOptional] protected readonly AppInitScenesTransitionSetupDataSO.AppInitSceneSetupData sceneSetupData =
        new AppInitScenesTransitionSetupDataSO.AppInitSceneSetupData(DoNotOverride);
    [Inject] readonly AppInitSetupData _setupData = default;
    [Inject] readonly GameScenesManager _gameScenesManager = default;


#if UNITY_EDITOR
    private enum SyncMode {
        Undefined,
        Async,
        Sync
    }

    private static SyncMode _syncMode;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void NoDomainReloadInit() {

        _syncMode = SyncMode.Undefined;
    }

    public static bool ShouldRunSynchronously() {

        if (_syncMode == SyncMode.Undefined) {
            _syncMode = UnityEngine.SceneManagement.SceneManager.sceneCount == 1
                ? SyncMode.Async
                : SyncMode.Sync;

        }

        return _syncMode == SyncMode.Sync;
    }
#endif

    protected GameScenesManager gameScenesManager => _gameScenesManager;
    protected bool isTestContext => _setupData?.runMode == AppInitSetupData.RunMode.PlayTest;

    public enum AppStartType {
        AppStart,
        AppRestart,
        MultiSceneEditor
    }

    protected async void Awake() {

        try {
            await StartGameAsync();
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }

    private async Task StartGameAsync() {

        await PreloadAsync();
        await _asyncSceneContext.RunAsync();
        await InitializeAsync();
    }

    /// <summary>
    /// Runs before the load of the initial resources of the game
    /// </summary>
    /// <remarks>
    /// This method cannot be always async.
    /// If you provide any async implementation of this method, you should have a version that runs synchronously
    /// for the editor because the NoTransitionInstaller will not wait for this task and start the Scene context
    /// before the load of the resources.
    /// TODO: Fix this: https://beatgames.atlassian.net/browse/BS-6024
    /// </remarks>
    protected virtual Task PreloadAsync() {

        return Task.CompletedTask;
    }

    private async Task InitializeAsync() {

        _gameScenesManager.MarkSceneAsPersistent(gameObject.scene.name);

        var appStartType = GetAppStartType();

        // App Start or Restart
        if (appStartType is AppStartType.AppStart or AppStartType.AppRestart) {
            _cameraGO.SetActive(true);
        }

        _gameScenesManager.beforeDismissingScenesEvent += HandleBeforeDismissingScenes;

        var startType = GetAppStartType();

        // Wait for everything is prepared.
        await UnityAsyncHelper.WaitUntilAsync(this, () => !_gameScenesManager.isInTransition);

        // App Start Setup and running other scenes in editor
        if (startType == AppStartType.AppStart || startType == AppStartType.MultiSceneEditor) {
            AppStartAndMultiSceneEditorSetup();
        }

        // Repeatable Setup
        await RepeatableSetupAsync();

        // Transition to next scene
        if (startType != AppStartType.MultiSceneEditor) {
            await UnityAsyncHelper.WaitUntilAsync(this, () => SplashScreen.isFinished);
            TransitionToNextScene();
        }
    }

    protected void OnDestroy() {

        if (_gameScenesManager != null) {
            _gameScenesManager.beforeDismissingScenesEvent -= HandleBeforeDismissingScenes;
        }
    }

    private void HandleBeforeDismissingScenes(List<string> scenes) {

        _gameScenesManager.beforeDismissingScenesEvent -= HandleBeforeDismissingScenes;
        _cameraGO.SetActive(false);
    }

    protected internal AppStartType GetAppStartType() {

        if (sceneSetupData.appInitOverrideStartType != DoNotOverride) {

            return sceneSetupData.appInitOverrideStartType switch {
                AppRestart => AppStartType.AppRestart,
                AppStart => AppStartType.AppStart,
                MultiSceneEditor => AppStartType.MultiSceneEditor,
                _ => AppStartType.AppRestart
            };
        }
        return UnityEngine.SceneManagement.SceneManager.sceneCount == 1
            ? AppStartType.AppStart
            : AppStartType.MultiSceneEditor;
    }

    protected abstract void AppStartAndMultiSceneEditorSetup();
    protected abstract Task RepeatableSetupAsync();
    protected abstract void TransitionToNextScene();
}
