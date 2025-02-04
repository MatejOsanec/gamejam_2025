using System.ComponentModel;
using UnityEngine;

namespace BGLib.Polyglot {

    /// <summary>
    /// Enum of all currently defined languages in code with some additional debug languages. Ordering is actually important, matches CSV columns of localization table
    /// For list of actually supported languages with no debug/hidden languages check LocalizationMain.asset
    /// </summary>
    public enum Language {
        English = 0,
        French = 1,
        Spanish = 2,
        German = 3,
        Italian = 4,
        Portuguese_Brazil = 5,
        Portuguese = 6,
        Russian = 7,
        Greek = 8,
        Turkish = 9,
        Danish = 10,
        Norwegian = 11,
        Swedish = 12,
        Dutch = 13,
        Polish = 14,
        Finnish = 15,
        Japanese = 16,
        Simplified_Chinese = 17,
        Traditional_Chinese = 18,
        Korean = 19,
        Czech = 20,
        Hungarian = 21,
        Romanian = 22,
        Thai = 23,
        Bulgarian = 24,
        Hebrew = 25,
        Arabic = 26,
        Bosnian = 27,
        Debug_Keys = 28,
        Debug_English_Reverted = 29,
        Debug_Word_With_Max_Lenght = 30
    }

    public static class LanguageExtensions {

        public static string ToSerializedName(this Language lang) {

            switch (lang) {
#if BS_TOURS
                case Language.English:
                    return "en_US";
                case Language.French:
                    return "fr_FR";
                case Language.Spanish:
                    return "es_ES";
                case Language.German:
                    return "de_DE";
                case Language.Italian:
                    return "it_IT";
                case Language.Portuguese_Brazil:
                    return "pt_BR";
                case Language.Portuguese:
                    return "pt_PT";
                case Language.Russian:
                    return "ru_RU";
                case Language.Greek:
                    return "el_GR";
                case Language.Turkish:
                    return "tr_TR";
                case Language.Danish:
                    return "da_DA";
                case Language.Norwegian:
                    return "nb_NO";
                case Language.Swedish:
                    return "sv_SE";
                case Language.Dutch:
                    return "nl_NL";
                case Language.Polish:
                    return "pl_PL";
                case Language.Finnish:
                    return "fi_FI";
                case Language.Japanese:
                    return "ja_JP";
                case Language.Simplified_Chinese:
                    return "zh_CH";
                case Language.Traditional_Chinese:
                    return "zh_TW";
                case Language.Korean:
                    return "ko_KR";
                case Language.Czech:
                    return "cs_CZ";
                case Language.Hungarian:
                    return "hu_HU";
                case Language.Romanian:
                    return "ro_RO";
                case Language.Thai:
                    return "th_TH";
                case Language.Bulgarian:
                    return "bg_BG";
                case Language.Hebrew:
                    return "he_IL";
                case Language.Arabic:
                    return "ar_AE";
                case Language.Bosnian:
                    return "bs_Latn_BA";
                case Language.Debug_Keys:
                    return "Debug_Keys";
                case Language.Debug_English_Reverted:
                    return "Debug_English_Reverted";
                case Language.Debug_Word_With_Max_Lenght:
                    return "Debug_Word_With_Max_Lenght";
                default:
                    return "en_US";
#else
                case Language.English:
                    return "en";
                case Language.French:
                    return "fr";
                case Language.Spanish:
                    return "es";
                case Language.German:
                    return "de";
                case Language.Italian:
                    return "it";
                case Language.Portuguese_Brazil:
                    return "pt";
                case Language.Portuguese:
                    return "pt";
                case Language.Russian:
                    return "ru";
                case Language.Greek:
                    return "el";
                case Language.Turkish:
                    return "tr";
                case Language.Danish:
                    return "da";
                case Language.Norwegian:
                    return "no";
                case Language.Swedish:
                    return "sv";
                case Language.Dutch:
                    return "nl";
                case Language.Polish:
                    return "pl";
                case Language.Finnish:
                    return "fi";
                case Language.Japanese:
                    return "ja";
                case Language.Simplified_Chinese:
                    return "zh";
                case Language.Traditional_Chinese:
                    return "zh";
                case Language.Korean:
                    return "ko";
                case Language.Czech:
                    return "cs";
                case Language.Hungarian:
                    return "hu";
                case Language.Romanian:
                    return "ro";
                case Language.Thai:
                    return "th";
                case Language.Bulgarian:
                    return "bg";
                case Language.Hebrew:
                    return "he";
                case Language.Arabic:
                    return "ar";
                case Language.Bosnian:
                    return "bs";
                case Language.Debug_Keys:
                    return "Debug_Keys";
                case Language.Debug_English_Reverted:
                    return "Debug_English_Reverted";
                case Language.Debug_Word_With_Max_Lenght:
                    return "Debug_Word_With_Max_Lenght";
                default:
                    return "en";
#endif
            }
        }

        public static string ToCultureInfoName(this Language lang) {

            switch (lang) {
                case Language.English:
                    return "en";
                case Language.Spanish:
                    return "es";
                case Language.French:
                // return "de"; // we are using german culture instead, because when using string.format for percentage (P), you get ٪ instead of %
                case Language.German:
                    return "de";
                case Language.Japanese:
                    return "ja";
                case Language.Korean:
                    return "ko";
                default:
                    return "";
            }
        }

        public static Language ToLanguage(this string serializedName) {

            serializedName = serializedName.ToLower();
            serializedName = serializedName.Replace('-', '_'); // Oculus Rift platform uses BCP 47 language tag, which represent language by "xy-ZY", we use "xy_zy"

            string[]? subtags = serializedName.Split(separator: '_');
            if (subtags == null || subtags.Length == 0) {
                return Language.English;
            }

            switch (subtags[0]) {
                // IETF tags + ISO 639-1 and ISO 639-2 codes, for the biggest compatibility with different representations
                case "en":
                case "eng":
                    return Language.English;
                case "fr":
                case "fra":
                    return Language.French;
                case "es":
                case "spa":
                    return Language.Spanish;
                case "de":
                case "deu":
                    return Language.German;
                case "it":
                case "ita":
                    return Language.Italian;
                case "pt":
                case "por":
                    if (subtags.Length == 1) {
                        return Language.Portuguese;
                    }
                    switch (subtags[1]) {
                        case "br":
                            return Language.Portuguese_Brazil;
                        case "pt":
                        default:
                            return Language.Portuguese;
                    }
                case "ru":
                case "rus":
                    return Language.Russian;
                case "el":
                case "ell":
                    return Language.Greek;
                case "tr":
                case "tur":
                    return Language.Turkish;
                case "da":
                case "dan":
                    return Language.Danish;
                case "no":
                case "nor":
                    return Language.Norwegian;
                case "sv":
                case "swe":
                    return Language.Swedish;
                case "nl":
                case "nld":
                    return Language.Dutch;
                case "pl":
                case "pol":
                    return Language.Polish;
                case "fi":
                case "fin":
                    return Language.Finnish;
                case "ja":
                case "jpn":
                    return Language.Japanese;
                case "zh":
                case "zho": {
                    if (subtags.Length == 1) {
                        return Language.Simplified_Chinese;
                    }
                    switch (subtags[1]) {
                        case "cn":
                            return Language.Simplified_Chinese;
                        case "tw":
                        case "hk":
                        case "mo":
                        default:
                            return Language.Traditional_Chinese;
                    }
                }
                case "ko":
                case "kor":
                    return Language.Korean;
                case "cs":
                case "ces":
                    return Language.Czech;
                case "hu":
                case "hun":
                    return Language.Hungarian;
                case "ro":
                case "ron":
                    return Language.Romanian;
                case "th":
                case "tha":
                    return Language.Thai;
                case "bg":
                case "bul":
                    return Language.Bulgarian;
                case "he":
                case "heb":
                    return Language.Hebrew;
                case "ar":
                case "ara":
                    return Language.Arabic;
                case "bs":
                case "bos":
                    return Language.Bosnian;
                case "debug": {
                    switch (serializedName) {
                        case "debug_keys":
                            return Language.Debug_Keys;
                        case "debug_english_reverted":
                            return Language.Debug_English_Reverted;
                        case "debug_word_with_max_lenght":
                            return Language.Debug_Word_With_Max_Lenght;
                        default:
                            return Language.Debug_Keys;
                    }
                }
                // Steam localization API language codes
                case "english":
                    return Language.English;
                case "french":
                    return Language.French;
                case "spanish":
                    return Language.Spanish;
                case "german":
                    return Language.German;
                case "italian":
                    return Language.Italian;
                case "portuguese":
                    return Language.Portuguese;
                case "brazilian":
                    return Language.Portuguese_Brazil;
                case "russian":
                    return Language.Russian;
                case "greek":
                    return Language.Greek;
                case "turkish":
                    return Language.Turkish;
                case "danish":
                    return Language.Danish;
                case "norwegian":
                    return Language.Norwegian;
                case "swedish":
                    return Language.Swedish;
                case "dutch":
                    return Language.Dutch;
                case "polish":
                    return Language.Polish;
                case "finnish":
                    return Language.Finnish;
                case "japanese":
                    return Language.Japanese;
                case "schinese":
                    return Language.Simplified_Chinese;
                case "tchinese":
                    return Language.Traditional_Chinese;
                case "korean":
                    return Language.Korean;
                case "czech":
                    return Language.Czech;
                case "hungarian":
                    return Language.Hungarian;
                case "romanian":
                    return Language.Romanian;
                case "thai":
                    return Language.Thai;
                case "bulgarian":
                    return Language.Bulgarian;
                case "hebrew":
                    return Language.Hebrew;
                case "arabic":
                    return Language.Arabic;
                case "bosnian":
                    return Language.Bosnian;

                default:
                    return Language.English;
            }
        }

        public static Language ToLanguage(this UnityEngine.SystemLanguage systemLanguage, bool useFallbackLanguage = true) {

            switch (systemLanguage) {
                case SystemLanguage.Arabic:
                    return Language.Arabic;
                case SystemLanguage.Bulgarian:
                    return Language.Bulgarian;
                case SystemLanguage.Czech:
                    return Language.Czech;
                case SystemLanguage.Danish:
                    return Language.Danish;
                case SystemLanguage.Dutch:
                    return Language.Dutch;
                case SystemLanguage.English:
                    return Language.English;
                case SystemLanguage.Finnish:
                    return Language.Finnish;
                case SystemLanguage.French:
                    return Language.French;
                case SystemLanguage.German:
                    return Language.German;
                case SystemLanguage.Greek:
                    return Language.Greek;
                case SystemLanguage.Hebrew:
                    return Language.Hebrew;
                case SystemLanguage.Hungarian:
                    return Language.Hungarian;
                case SystemLanguage.Italian:
                    return Language.Italian;
                case SystemLanguage.Japanese:
                    return Language.Japanese;
                case SystemLanguage.Korean:
                    return Language.Korean;
                case SystemLanguage.Norwegian:
                    return Language.Norwegian;
                case SystemLanguage.Polish:
                    return Language.Polish;
                case SystemLanguage.Portuguese:
                    return Language.Portuguese;
                case SystemLanguage.Romanian:
                    return Language.Romanian;
                case SystemLanguage.Russian:
                    return Language.Russian;
                case SystemLanguage.Spanish:
                    return Language.Spanish;
                case SystemLanguage.Swedish:
                    return Language.Swedish;
                case SystemLanguage.Thai:
                    return Language.Thai;
                case SystemLanguage.Turkish:
                    return Language.Turkish;
                case SystemLanguage.ChineseSimplified:
                    return Language.Simplified_Chinese;
                case SystemLanguage.ChineseTraditional:
                    return Language.Traditional_Chinese;
                default:
                    if (useFallbackLanguage) {
                        return Localization.Instance.fallbackLanguage;
                    }
                    else {
                        throw new InvalidEnumArgumentException(nameof(systemLanguage), (int)systemLanguage, typeof(Language));
                    }
            }
        }
    }
}
