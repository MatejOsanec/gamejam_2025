namespace BGLib.Notepad.Editor {

    using global::Notepad;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEngine;

    class NotepadComponentProcessor : IProcessSceneWithReport {
        public int callbackOrder => 0;

        public void OnProcessScene(UnityEngine.SceneManagement.Scene scene, BuildReport report) {

            if (!BuildPipeline.isBuildingPlayer) {
                return;
            }
            var notepadComponents = FindUnityObjectsHelper.GetComponentsInScene<NotepadComponent>(
                scene,
                includeInactive: true
            );
            foreach (var notepadComponent in notepadComponents) {
                Object.DestroyImmediate(notepadComponent);
            }
        }
    }
}
