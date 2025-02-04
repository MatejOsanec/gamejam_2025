namespace BGLib.AppFlow.Initialization {

    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.Assertions;
    using UnityEngine.ResourceManagement.AsyncOperations;
    using Zenject;

    public abstract class ScriptableObjectReferenceAsyncInstaller<T> : AsyncInstaller where T : ScriptableObject {

        protected abstract string assetRuntimeKey { get; }

        private AsyncOperationHandle<T> _operationHandle;

        protected internal sealed override void LoadResourcesBeforeInstall(IInstallerRegistry registry, DiContainer _) {

            if (!_operationHandle.IsValid()) {
                _operationHandle = LoadAsync(assetRuntimeKey);
            }
            _operationHandle.WaitForCompletion();
        }

        protected internal sealed override async Task LoadResourcesBeforeInstallAsync(
            IInstallerRegistry registry,
            DiContainer _
        ) {

            if (!_operationHandle.IsValid()) {
                _operationHandle = LoadAsync(assetRuntimeKey);
            }
            await _operationHandle.Task;
        }

        protected static AsyncOperationHandle<T> LoadAsync(string runtimeKey) {

            return Addressables.LoadAssetAsync<T>(runtimeKey);
        }

        public override void InstallBindings() {

            Assert.IsTrue(_operationHandle.IsValid());
            Assert.IsTrue(_operationHandle.IsDone);
            Container.Bind<T>().FromScriptableObject(_operationHandle.Result).AsSingle();
        }

        protected void OnDestroy() {

            if (_operationHandle.IsValid()) {
                Addressables.Release(_operationHandle);
            }
        }
    }
}
