namespace BGLib.UnityExtension {


    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using UnityEngine;

    using Object = UnityEngine.Object;


    public static class AssetBundleExtensions {

        public static TaskAwaiter<AssetBundle> GetAwaiter(this AssetBundleCreateRequest assetBundleCreateRequest) {

            var loadAssetBundleTaskSource = new TaskCompletionSource<AssetBundle>();
            assetBundleCreateRequest.completed += _ => {
                loadAssetBundleTaskSource.TrySetResult(assetBundleCreateRequest.assetBundle);
            };
            return loadAssetBundleTaskSource.Task.GetAwaiter();
        }

        public static TaskAwaiter<Object> GetAwaiter(this ResourceRequest resourceRequest) {

            var loadResourceTaskSource = new TaskCompletionSource<Object>();
            resourceRequest.completed += _ => {
                loadResourceTaskSource.TrySetResult(resourceRequest.asset);
            };
            return loadResourceTaskSource.Task.GetAwaiter();
        }
    }
}
