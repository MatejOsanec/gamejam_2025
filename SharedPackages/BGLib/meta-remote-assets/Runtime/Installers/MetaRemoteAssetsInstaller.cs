namespace BGLib.MetaRemoteAssets.Installers {

    using Platform;
    using Zenject;

    public class MetaRemoteAssetsInstaller: ScriptableObjectInstaller {

        [Inject] private readonly AppInitSetupData _appInitSetupData = default!;

        public override void InstallBindings() {

#if UNITY_ANDROID
            Container.Bind<string>().WithId(MetaRemoteAssetsManager.kPlatformInjectId)
                .FromInstance(SupportedPlatforms.Android).WhenInjectedInto<MetaRemoteAssetsManager>();
#elif UNITY_PS5 || UNITY_PS4
            Container.Bind<string>().WithId(MetaRemoteAssetsManager.kPlatformInjectId)
                .FromInstance(SupportedPlatforms.Playstation).WhenInjectedInto<MetaRemoteAssetsManager>();
#elif UNITY_STANDALONE
            Container.Bind<string>().WithId(MetaRemoteAssetsManager.kPlatformInjectId)
                .FromInstance(SupportedPlatforms.Windows).WhenInjectedInto<MetaRemoteAssetsManager>();
#endif

            InstallRemoteCatalogLoader();

            Container.BindInterfacesAndSelfTo<MetaRemoteAssetsManager>().AsSingle();

            if (_appInitSetupData.runMode != AppInitSetupData.RunMode.PlayTest) {
                Container.BindInterfacesAndSelfTo<MetaRemoteAssetsCatalogUpdater>().AsSingle();
            }
        }

        private void InstallRemoteCatalogLoader() {

            if (_appInitSetupData.runMode == AppInitSetupData.RunMode.PlayTest) {
                Container.BindInterfacesAndSelfTo<MockRemoteCatalogLoader>()
                    .AsSingle();
                return;
            }

            Container.BindInterfacesAndSelfTo<MetaRemoteAssetsRemoteCatalogLoader>()
                .AsSingle();
        }
    }
}
