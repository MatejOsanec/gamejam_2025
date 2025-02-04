namespace BGLib.Polyglot {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AppFlow.Initialization;
    using UnityExtension;
    using UnityEngine;

    public class LocalizationAsyncInstaller : AddressablesAsyncInstaller<TextAsset> {

        [SerializeField] Localization _mainPolyglotAsset = default!;

        private List<LocalizationAsset>? _inputFiles;

        private const string kLocalizationContentLabel = "LocalizationContent";

        protected override string assetLabelRuntimeKey => kLocalizationContentLabel;

        protected override void LoadResourcesBeforeInstall(IList<TextAsset> assets, IInstallerRegistry registry) {

            _inputFiles = LocalizationContentToAsset(assets);
        }

        private static List<LocalizationAsset> LocalizationContentToAsset(IEnumerable<TextAsset> content) {

            return content.Select(
                localizationTextAsset => new LocalizationAsset(localizationTextAsset, GoogleDriveDownloadFormat.CSV)
            ).ToList();
        }

        public override void InstallBindings() {

            if (_inputFiles == null) {
                throw new InvalidOperationException(
                    "Input files cannot be null, it was not initialized before the binding or the initialization failed."
                );
            }
            Container.Bind<Localization>().FromScriptableObject(_mainPolyglotAsset).AsSingle();
            Container.Bind<List<LocalizationAsset>>().FromInstance(_inputFiles).AsCached();
            Container.Bind<LocalizationModel>().AsSingle().NonLazy();
        }

        public static List<LocalizationAsset> LoadLocalizationAssetsSync() {

            var localizationContentList = AddressablesExtensions.LoadContent<TextAsset>(kLocalizationContentLabel);
            return LocalizationContentToAsset(localizationContentList);
        }
    }
}
