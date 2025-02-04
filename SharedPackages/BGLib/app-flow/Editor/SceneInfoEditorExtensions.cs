namespace BGLib.AppFlow.Editor {

    public static class SceneInfoEditorExtensions {

        public static void SetSceneData(
            this SceneInfo sceneInfo,
            string? sceneName = null,
            bool? disabledRootObjects = null
        ) {

            if (sceneName != null) {
                sceneInfo._sceneName = sceneName;
            }

            if (disabledRootObjects != null) {
                sceneInfo._disabledRootObjects = disabledRootObjects.Value;
            }
        }
    }
}
