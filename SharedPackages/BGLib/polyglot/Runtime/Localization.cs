namespace BGLib.Polyglot {

    using System;
    using UnityEngine;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;

    public class Localization : ScriptableObject {

        private const string KeyNotFound = "[{0}]";

        private static LocalizationModel? _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void NoDomainReloadInit() {

            _instance = null;
        }

        /// <summary>
        /// The singleton instance of this manager.
        /// </summary>
        public static LocalizationModel Instance {
            get {
#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    throw new NotImplementedException(
                        "Localization is not supported in Editor, use 'EditorLocalization.instance' instead."
                    );
                }
#endif
                if (_instance is null) {
                    throw new InvalidOperationException("Cannot call localization before it's loaded");
                }
                return _instance;
            }
        }

        internal static void SetSingletonInstance(LocalizationModel model) {

            _instance = model ?? throw new ArgumentNullException(nameof(model));
        }

        [Header("Language Support")]
        [Tooltip("The supported languages by the game.\n Leave empty if you support them all.")]
        [SerializeField]
        private List<Language> supportedLanguages = default!;

        public IReadOnlyList<Language> SupportedLanguages => supportedLanguages;

        [Tooltip(
            "The currently selected language of the game.\nThis will also be the default when you start the game for the first time."
        )]
        [SerializeField]
        internal Language selectedLanguage = Language.English;

        [Tooltip("If we cant find the string for the selected language we fall back to this language.")]
        [SerializeField]
        private Language fallbackLanguage = Language.English;

        internal Language FallbackLanguage => fallbackLanguage;

        #region Arabic Support

#if ARABSUPPORT_ENABLED
        [Header("Arabic Support")]

        [Tooltip("Vowel marks in Arabic.")]
        [SerializeField]
        private bool showTashkeel = true;

        [SerializeField]
        private bool useHinduNumbers = false;
#endif

        #endregion

        internal static LanguageDirection GetLanguageDirection(Language language) {

            return language switch {
                Language.Hebrew => LanguageDirection.RightToLeft,
                Language.Arabic => LanguageDirection.RightToLeft,
                _ => LanguageDirection.LeftToRight
            };
        }

        internal int selectedLanguageIndex => HasNoSupportedLanguage()
            ? (int)selectedLanguage
            : supportedLanguages.IndexOf(selectedLanguage);

        internal bool IsLanguageSupported(Language language) {

            return HasNoSupportedLanguage() || supportedLanguages.Contains(language);
        }

        internal bool HasNoSupportedLanguage() {

            return supportedLanguages.Count == 0;
        }

        /// <summary>
        /// The english name of the selected language.
        /// </summary>
        public string EnglishLanguageName => Get("LANGUAGE_THIS_EN");

        /// <summary>
        /// The Localized name of the selected language.
        /// </summary>
        public string LocalizedLanguageName => Get("LANGUAGE_THIS");

        public static Language ConvertSystemLanguage(SystemLanguage selected) {

            return selected.ToLanguage(useFallbackLanguage: false);
        }

        /// <summary>
        /// Retreives the correct language string by key.
        /// </summary>
        /// <param name="key">The key string</param>
        /// <returns>A localized string</returns>
        public static string Get(string key) {

            return Instance.Get(key);
        }
    }
}
