namespace BGLib.BuildProcess.Editor {

    using System.Collections;
    using UnityEditor;
    using UnityEditor.PackageManager;
    using UnityEditor.PackageManager.Requests;
    using UnityEngine;

    public class PackageRequestStageWaiter : IBuildStageWaiter {

        private readonly Request _packageRequest;

        public PackageRequestStageWaiter(Request packageRequest) {

            _packageRequest = packageRequest;
        }

        public IEnumerator Wait() {

            var waitFor = EditorBuildStageWaiter.shortWait;

            yield return waitFor;
            while (!_packageRequest.IsCompleted) {
                yield return waitFor;
            }
            if (_packageRequest.Status == StatusCode.Failure) {
                Debug.LogError(_packageRequest.Error.message);
                yield break;
            }

            while (EditorApplication.isUpdating || EditorApplication.isCompiling) {
                yield return waitFor;
            }

            yield return null;
        }
    }
}
