namespace BGLib.MetaRemoteAssets {

    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading;
    using Zenject;
    using System;
    using UnityEngine;

    public class MetaRemoteAssetsCatalogUpdater : IDisposable, IInitializable {

        [Inject] private readonly MetaRemoteAssetsManager _remoteAssetsManager;
        [Inject] private readonly GameScenesManager _scenesManager;

        private const int kWaitIntervalInSeconds = 20;

        private CancellationTokenSource _cancellationTokenSource;
        private Task? _checkForCatalogUpdateOngoingTask;

        private const string kGameplaySceneName = "GameCore";

        public MetaRemoteAssetsCatalogUpdater(MetaRemoteAssetsManager remoteAssetsManager, GameScenesManager scenesManager) {

            _remoteAssetsManager = remoteAssetsManager;
            _scenesManager = scenesManager;

            _cancellationTokenSource = new CancellationTokenSource();
        }


        public void Initialize() {

            _scenesManager.transitionDidFinishEvent -= HandleGameSceneChanged;
            _scenesManager.transitionDidFinishEvent += HandleGameSceneChanged;

            _checkForCatalogUpdateOngoingTask = CheckForCatalogUpdateWithInterval(_cancellationTokenSource.Token);
        }

        public void Dispose() {

            _cancellationTokenSource.Cancel();
        }

        private void HandleGameSceneChanged(
            GameScenesManager.SceneTransitionType sceneTransitionType,
            ScenesTransitionSetupDataSO transitionSetupDataSo,
            DiContainer container
        ) {

            var isGameLoaded = transitionSetupDataSo != null &&
                               transitionSetupDataSo.scenes.Any(info => info.sceneName == kGameplaySceneName);

            // Loaded scenes contains "GameCore" (multiplayer, or single player), but there's an
            // ongoing catalog update task to be cancelled, as we don't want it to happen in-game.
            if (isGameLoaded && _checkForCatalogUpdateOngoingTask != null) {
               _cancellationTokenSource.Cancel();
               _checkForCatalogUpdateOngoingTask = null;
               return;
            }

            // None of the loaded scenes is "GameCore", but the task was previously cancelled, so it
            // needs to be restarted
            if (!isGameLoaded && _checkForCatalogUpdateOngoingTask == null) {
               _cancellationTokenSource = new CancellationTokenSource();
               _checkForCatalogUpdateOngoingTask = CheckForCatalogUpdateWithInterval(_cancellationTokenSource.Token);
            }
        }

        private async Task CheckForCatalogUpdateWithInterval(CancellationToken cancellationToken) {

            await _remoteAssetsManager.WaitInitAsync();

            while (true) {
                try {
                    await _remoteAssetsManager.UpdateCatalogsAsync(cancellationToken);
                }
                catch (Exception e) {
                    Debug.LogException(e);
                }

                await Task.Delay(TimeSpan.FromSeconds(kWaitIntervalInSeconds), cancellationToken);
                if (cancellationToken.IsCancellationRequested) {
                    return;
                }
            }
        }
    }
}
