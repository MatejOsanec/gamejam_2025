namespace BGLib.BuildProcess.Editor {

    using System.Collections;
    using Unity.EditorCoroutines.Editor;
    using UnityEditor;

    public class EditorBuildStageWaiter : IBuildStageWaiter {

        internal static readonly EditorWaitForSeconds shortWait = new EditorWaitForSeconds(0.2f);
        public static readonly EditorBuildStageWaiter shared = new EditorBuildStageWaiter();

        public IEnumerator Wait() {

            var waitFor = shortWait;

            yield return waitFor;
            while (EditorApplication.isUpdating || EditorApplication.isCompiling) {
                yield return waitFor;
            }
            yield return waitFor;
        }
    }
}
