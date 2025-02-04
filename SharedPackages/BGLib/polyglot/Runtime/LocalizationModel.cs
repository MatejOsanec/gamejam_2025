namespace BGLib.Polyglot {

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Object = UnityEngine.Object;

    public class LocalizationModel {

        private const Language kDefaultLanguage = Language.English;

        private readonly Localization _localization;
        private readonly List<LocalizationAsset> _inputFiles;
        private CultureInfo _selectedCulture = CultureInfo.InvariantCulture;

        public readonly LocalizationImporter importer;
        public List<LocalizationAsset> inputFiles => _inputFiles;
        public CultureInfo SelectedCultureInfo {
            get => _selectedCulture;
            private set {
                _selectedCulture = value;

                // Override default System culture as that would be taken from device and not from the selected supported language
                // This is introduced mainly to resolve issue with Turkish upper-casing of "i" character and lower-casing of "I" character.
                // See comments of https://beatgames.atlassian.net/browse/USS-433 for details.
                CultureInfo.CurrentCulture = value;
                CultureInfo.CurrentUICulture = value;
                CultureInfo.DefaultThreadCurrentCulture = value;
                CultureInfo.DefaultThreadCurrentUICulture = value;
            }
        }
        public Language SelectedLanguage {
            get => _localization.selectedLanguage;
            set {
                if (value == _localization.selectedLanguage) {
                    return;
                }
                if (_localization.IsLanguageSupported(value)) {
                    _localization.selectedLanguage = value;
                    SelectedCultureInfo = GetCultureInfo(value);
                    _onChangeLanguage?.Invoke(this);
#if UNITY_EDITOR
                    if (Application.isPlaying) {
                        return;
                    }
                    var localized = Object.FindObjectsOfType<LocalizedText>();
                    foreach (var local in localized) {
                        local.OnLocalize(this);
                    }
#endif
                }
                else {
                    Debug.LogWarning(value + " is not a supported language.");
                }
            }
        }
        public Language fallbackLanguage => _localization.FallbackLanguage;
        public LanguageDirection selectedLanguageDirection =>
            Localization.GetLanguageDirection(_localization.selectedLanguage);
        public IReadOnlyList<Language> supportedLanguages => _localization.SupportedLanguages;
        /// <summary>
        /// The english names of all available languages.
        /// </summary>
        public List<string> englishLanguageNames => importer.GetLanguages(
            "LANGUAGE_THIS_EN",
            _localization.SupportedLanguages
        );
        /// <summary>
        /// The localized names of all available languages.
        /// </summary>
        public List<string> localizedLanguageNames => importer.GetLanguages(
            "LANGUAGE_THIS",
            _localization.SupportedLanguages
        );

        internal int selectedLanguageIndex => _localization.selectedLanguageIndex;

        private event Action<LocalizationModel>? _onChangeLanguage;

        internal LocalizationModel(Localization localization, Language language, List<LocalizationAsset> inputFiles) {

            this._localization = localization;
            this._inputFiles = inputFiles;

            // Deterministically initialize the CultureInfo with Invariant to override the system defaults.
            // Mostly used as a safe guard in case the later initialization would fail and the system default would be
            // unsupported language.
            SelectedCultureInfo = CultureInfo.InvariantCulture;

            SelectedLanguage = localization.IsLanguageSupported(language) ? language : kDefaultLanguage;

            SelectedCultureInfo = GetCultureInfo(SelectedLanguage);
            importer = new LocalizationImporter(this);
            Localization.SetSingletonInstance(this);
        }

        /// <summary>
        /// Select a language, used by dropdowns and the like.
        /// </summary>
        /// <param name="selected"></param>
        public void SelectLanguage(int selected) {

            if (_localization.HasNoSupportedLanguage()) {
                SelectedLanguage = (Language)selected;
            }
            else {
                SelectedLanguage = _localization.SupportedLanguages[selected];
            }
        }

        /// <summary>
        /// Add a Localization listener to catch the event that is invoked when the selected language is changed.
        /// </summary>
        /// <param name="localize"></param>
        public void AddOnLocalizeEvent(ILocalize localize) {

            _onChangeLanguage -= localize.OnLocalize;
            _onChangeLanguage += localize.OnLocalize;
            localize.OnLocalize(this);
        }

        public void RemoveOnLocalizeEvent(ILocalize localize) {

            _onChangeLanguage -= localize.OnLocalize;
        }

        public bool InputFilesContains(LocalizationDocument doc) {

            return _inputFiles.Any(
                inputFile => inputFile != null && inputFile.TextAsset == doc.TextAsset && inputFile.Format == doc.Format
            );
        }

        private CultureInfo GetCultureInfo(Language language) {

            return _localization.IsLanguageSupported(language)
                ? new CultureInfo(language.ToCultureInfoName())
                : CultureInfo.InvariantCulture;
        }

        public string Get(string key) {

            bool found = TryGet(key, SelectedLanguage, out var result);
            if (!found) {
                Debug.LogWarning($"Localization key '{key}' not found");
            }
            return result;
        }

        public string GetOrKey(string key) {

            TryGet(key, SelectedLanguage, out var result);
            return result;
        }

        public bool TryGet(string key, Language language, out string value) {

            var languages = importer.GetLanguages(key);
            var selected = (int)language;
            if (languages.Count <= 0 || selected < 0 || selected >= languages.Count) {
                value = key;
                return false;
            }
            value = languages[selected];
            bool isValid = IsValueValid(value);
            if (!isValid) {
                Debug.LogWarning(
                    $"Could not find key {key} for current language {language}. Falling back to {fallbackLanguage} with {languages[(int)fallbackLanguage]}"
                );
                selected = (int)fallbackLanguage;
                value = languages[selected];
                isValid = IsValueValid(value);
            }
#if ARABSUPPORT_ENABLED
            if (isValid && selected == (int) Language.Arabic) {
                value = ArabicSupport.ArabicFixer.Fix(currentString, instance.showTashkeel, instance.useHinduNumbers);
            }
#endif
            return isValid;
        }

        /// <summary>
        ///  Returns the value formatted with the provided arguments and the localization selected culture info if
        /// the key is found. Return the key otherwise.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public string GetFormatOrKey(string key, params object[] arguments) {

            return TryGet(key, SelectedLanguage, out string value)
                ? string.Format(SelectedCultureInfo, value, arguments): value;
        }

        private static bool IsValueValid(string value) {

            return !string.IsNullOrEmpty(value) && !LocalizationImporter.IsLineBreak(value);
        }

        public bool KeyExist(string key) {

            return KeyExist(key, SelectedLanguage);
        }

        private bool KeyExist(string key, Language language) {

            var selected = (int)language;
            var languages = importer.GetLanguages(key);
            return languages.Count > 0 && selected >= 0 && selected < languages.Count;
        }
    }
}
