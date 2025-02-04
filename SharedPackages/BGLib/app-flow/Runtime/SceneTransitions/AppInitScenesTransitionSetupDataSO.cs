public class AppInitScenesTransitionSetupDataSO : SingleFixedSceneScenesTransitionSetupDataSO {

    public enum AppInitOverrideStartType {
        DoNotOverride,
        AppStart,
        AppRestart,
        MultiSceneEditor,
    }

    public class AppInitSceneSetupData : SceneSetupData {

        public AppInitOverrideStartType appInitOverrideStartType { get; private set; }

        public AppInitSceneSetupData(AppInitOverrideStartType appInitOverrideStartType) {

            this.appInitOverrideStartType = appInitOverrideStartType;
        }
    }

    public void Init() {

        base.Init(new AppInitSceneSetupData(AppInitOverrideStartType.AppRestart));
    }

    public void InitAsAppStart() {

        base.Init(new AppInitSceneSetupData(AppInitOverrideStartType.AppStart));
    }

    public void __Init(AppInitOverrideStartType appInitOverrideStartType) {

        base.Init(new AppInitSceneSetupData(appInitOverrideStartType));
    }
}