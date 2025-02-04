namespace BGLib.Polyglot.Editor {

    using Polyglot;
    using UnityEngine;
    using UnityEngine.Assertions;

    public static class EditorLocalization {

        private static LocalizationModel? _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void NoDomainReloadInit() {

            _instance = null;
        }

        public static LocalizationModel instance => _instance ??= CreateInstance();

        private static LocalizationModel CreateInstance() {

            var asset = FindUnityObjectsHelper.FindObjectWithUniqueName<Localization>("LocalizationMain");
            var localizationAssets = LocalizationAsyncInstaller.LoadLocalizationAssetsSync();
            Assert.IsTrue(localizationAssets.Count > 0);
            return new LocalizationModel(asset, Language.English, localizationAssets);
        }
    }
}
