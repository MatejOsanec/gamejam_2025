namespace BGLib.AppFlow.Initialization {

    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;
    using UnityEngine.ResourceManagement.ResourceLocations;


    public abstract class AddressablesAsyncInstaller<T> : AsyncInstaller {

        private AsyncOperationHandle<IList<T>> _handle;

        protected internal void LoadResourcesBeforeInstall(IInstallerRegistry registry, object _) {

            Addressables.InitializeAsync().WaitForCompletion();
            var handle = LoadAsync(assetLabelRuntimeKey);
            if (handle == null) {
                LoadResourcesBeforeInstall(new List<T>(), registry);
                return;
            }
            _handle = handle.Value;
            _handle.WaitForCompletion();
            LoadResourcesBeforeInstall(_handle.Result, registry);
        }

        protected internal async Task LoadResourcesBeforeInstallAsync(IInstallerRegistry registry, object _) {

            await Addressables.InitializeAsync().Task;
            var handle = LoadAsync(assetLabelRuntimeKey);
            if (handle == null) {
                LoadResourcesBeforeInstall(new List<T>(), registry);
                return;
            }
            _handle = handle.Value;
            LoadResourcesBeforeInstall(await _handle.Task, registry);
        }

        protected static AsyncOperationHandle<IList<T>>? LoadAsync(string runtimeKey) {

            var locations = GetLocations(runtimeKey);
            if (locations.Count <= 0) {
                return null;
            }
            return Addressables.LoadAssetsAsync<T>(locations.ToList(), callback: null);
        }

        protected abstract string assetLabelRuntimeKey { get; }
        protected abstract void LoadResourcesBeforeInstall(IList<T> assets, IInstallerRegistry registry);

        protected void OnDestroy() {

            if (_handle.IsValid()) {
                Addressables.Release(_handle);
            }
        }

        private static HashSet<IResourceLocation> GetLocations(string runtimeKey) {

            var locations = new HashSet<IResourceLocation>();
            foreach (var resourceLocator in Addressables.ResourceLocators) {
                if (resourceLocator.Locate(runtimeKey, typeof(T), out var foundLocations)) {
                    locations.UnionWith(foundLocations);
                }
            }

            return locations;
        }
    }
}
