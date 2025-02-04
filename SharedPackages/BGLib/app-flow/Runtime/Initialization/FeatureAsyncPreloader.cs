namespace BGLib.AppFlow.Initialization {

    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityExtension;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.ResourceLocations;

    public class FeatureAsyncPreloader : AsyncPreloader {

        private const string kFeatureAsyncPreloader = "FeatureAsyncPreloader";

        public override async Task PreloadAsync() {

            var locations = new HashSet<IResourceLocation>();
            foreach (var resourceLocator in Addressables.ResourceLocators) {
                if (resourceLocator.Locate(kFeatureAsyncPreloader, typeof(AsyncPreloader), out var foundLocations)) {
                    locations.UnionWith(foundLocations);
                }
            }

            if (locations.Count == 0) {
                return;
            }

            var allFeaturePreloaders = await Addressables.LoadAssetsAsync<AsyncPreloader>(locations.ToList(), callback: null);

            await Task.WhenAll(allFeaturePreloaders.Select(preloader => preloader.PreloadAsync()));
        }

#if UNITY_EDITOR
        public override void PreloadSynchronously() { }
#endif
    }
}
