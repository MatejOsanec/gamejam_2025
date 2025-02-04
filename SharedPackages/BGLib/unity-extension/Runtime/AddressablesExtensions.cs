namespace BGLib.UnityExtension {

    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;

    public static class AddressablesExtensions {

        public static Task<IList<T>> LoadContentAsync<T>(IKeyEvaluator label) {

            return LoadContentOperation<T>(label).Task;
        }

        public static IList<T> LoadContent<T>(object label) {

            var loadOp = LoadContentOperation<T>(label);
            return loadOp.WaitForCompletion();
        }

        private static AsyncOperationHandle<IList<T>> LoadContentOperation<T>(object label) {

            return Addressables.LoadAssetsAsync<T>(label, null);
        }

        // Intended to be mainly used with Addressable AssetReference.InstantiateAsync to workaround following issue
        // Issue: https://forum.unity.com/threads/asyncoperationhandle-completed-not-firing-until-a-frame-late.1097638/
        // TL;DR AssetReference.InstantiateAsync returns from await (and callback) 1-2 frames after the object is already in scene
        public static TaskAwaiter<T> GetAwaiter<T>(this AsyncOperationHandle<T> asyncOperationHandle) {

            var taskSource = new TaskCompletionSource<T>();
            if (asyncOperationHandle.IsDone) {
                taskSource.SetResult(asyncOperationHandle.Result);
            }
            else {
                void SetResult(AsyncOperationHandle<T> handle) {
                    asyncOperationHandle.Completed -= SetResult;
                    taskSource.SetResult(asyncOperationHandle.Result);
                }

                asyncOperationHandle.Completed += SetResult;
            }

            return taskSource.Task.GetAwaiter();
        }
    }
}
