namespace BGLib.AppFlow.Initialization {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Zenject;

    public class AsyncSceneContext : SceneContext {

        private enum State {
            NotInitialized,
            Initializing,
            Initialized
        }

        [SerializeField] List<AsyncPreloader> _asyncPreloaders;
        [SerializeField] List<AsyncInstaller> _asyncInstallers;

        private State _state = State.NotInitialized;
        private AsyncInstallerRegistry _registry;



        public override async void Run() {

            await RunAsync();
        }

        public async Task RunAsync() {

            Assert.AreEqual(State.NotInitialized, _state);
            try {
                _state = State.Initializing;

#if UNITY_EDITOR
                _registry = AppInit.ShouldRunSynchronously()
                    // ReSharper disable once MethodHasAsyncOverload
                    ? LoadInstallers()
                    : await LoadInstallersAsync();
#else
                _registry = await LoadInstallersAsync();
#endif

                base.Run();
            }
            catch (Exception e) {
                Debug.LogException(e, gameObject);
            }
            finally {
                _state = State.Initialized;
            }
        }

        /// <summary>
        /// This creates container strictly for loading
        /// </summary>
        private DiContainer CreateContainerForLoading() {

            return new DiContainer(GetParentContainers(), ProjectContext.Instance.Container.IsValidating);
        }

        private AsyncInstallerRegistry CreateRegistry() {

            var registry = new AsyncInstallerRegistry();
            registry.monoInstallers.AddRange(_asyncInstallers);

            return registry;
        }

        private async Task<AsyncInstallerRegistry> LoadInstallersAsync() {

            var registry = CreateRegistry();
            var container = CreateContainerForLoading();

            var preloadingTasks = _asyncPreloaders.Select(
                r => r.PreloadAsync()
            );

            await Task.WhenAll(preloadingTasks);

            var initializationTasks = _asyncInstallers.Select(
                r => r.LoadResourcesBeforeInstallAsync(registry, container)
            );

            await Task.WhenAll(initializationTasks);

            return registry;
        }

#if UNITY_EDITOR
        private AsyncInstallerRegistry LoadInstallers() {

            var registry = CreateRegistry();
            var container = CreateContainerForLoading();

            foreach (var preloader in _asyncPreloaders) {
                preloader.PreloadSynchronously();
            }

            foreach (var asyncInstaller in _asyncInstallers) {
                asyncInstaller.LoadResourcesBeforeInstall(registry, container);
            }

            return registry;
        }
#endif

        protected override void InstallInstallers() {

            base.InstallInstallers();

            var registry = _registry;
#if UNITY_EDITOR
            if (IsValidating) {
                registry = LoadInstallers();
            }
#endif

            foreach (var installer in registry.scriptableObjectInstallers) {
                Container.Inject(installer);
                installer.InstallBindings();
            }

            foreach (var installer in registry.monoInstallers) {
                Container.Inject(installer);
                installer.InstallBindings();
            }
        }
    }
}
