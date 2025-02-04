namespace BGLib.UnityExtension {

    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine.Networking;

    public static class UnityWebRequestExtensions {

        public static async Task<UnityWebRequest.Result> SendWebRequestAsync(
            this UnityWebRequest request,
            CancellationToken cancellationToken = default
        ) {

            if (!cancellationToken.CanBeCanceled) {
                return await request.SendWebRequest();
            }

            await using var cancellationTokenRegistration = cancellationToken.Register(request.Abort);

            return await request.SendWebRequest();
        }

        public static TaskAwaiter<UnityWebRequest.Result> GetAwaiter(
            this UnityWebRequestAsyncOperation webRequestOperation
        ) {

            var tcs = new TaskCompletionSource<UnityWebRequest.Result>();

            if (webRequestOperation.isDone) {
                tcs.TrySetResult(webRequestOperation.webRequest.result);
            }
            else {
                webRequestOperation.completed += SetResult;

                void SetResult(UnityEngine.AsyncOperation _) {

                    webRequestOperation.completed -= SetResult;
                    tcs.TrySetResult(webRequestOperation.webRequest.result);
                }
            }

            return tcs.Task.GetAwaiter();
        }
    }
}
