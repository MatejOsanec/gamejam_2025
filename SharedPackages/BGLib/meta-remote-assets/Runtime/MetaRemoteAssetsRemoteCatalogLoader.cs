namespace BGLib.MetaRemoteAssets {

    using System;
    using UnityEngine.AddressableAssets;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;

    public class MetaRemoteAssetsRemoteCatalogLoader : IRemoteCatalogLoader {

        public async Task<bool> LoadRemoteCatalogAsync(CancellationToken cancellationToken) {

            var hasLoadedRemoteCatalog = Addressables.ResourceLocators.Any((locator) => locator.LocatorId == MetaRemoteAssetsManager.RemoteCatalogPath);
            if (hasLoadedRemoteCatalog) {
                return true;
            }

            try {

                await Addressables.LoadContentCatalogAsync(MetaRemoteAssetsManager.RemoteCatalogPath).Task;
                MetaRemoteAssetsManager.MakeRemoteCatalogTopPriority();
                return true;
            }
            catch (Exception e) {
                Debug.LogError($"Could not load remote catalog: {e.Message}");
                Debug.LogException(e);
                return false;
            }
        }

    }
}
