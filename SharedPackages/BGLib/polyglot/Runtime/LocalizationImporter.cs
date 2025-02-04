namespace BGLib.Polyglot {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class LocalizationImporter {

        /// <summary>
        /// A dictionary with the key of the text item you want and a list of all the languages.
        /// </summary>
        private readonly Dictionary<string, List<string>> _languageStrings = new Dictionary<string, List<string>>();
        private readonly List<string> _emptyList = new List<string>();
        private readonly List<LocalizationAsset> _inputFiles = new List<LocalizationAsset>();


        internal LocalizationImporter(LocalizationModel settings) {

            ImportFromFiles(settings);
        }

        private void ImportFromFiles(LocalizationModel settings) {

            _inputFiles.Clear();
            _inputFiles.AddRange(settings.inputFiles);
            ImportInputFiles();
        }

        private void ImportInputFiles() {

            for (var index = 0; index < _inputFiles.Count; index++) {
                var inputAsset = _inputFiles[index];

                if (inputAsset == null) {
                    Debug.LogError("Input File at index [" + index + "] is null");
                    continue;
                }

                if (inputAsset.TextAsset == null) {
                    Debug.LogError("Input File Text Asset at index [" + index + "] is null");
                    continue;
                }

                ImportTextFile(inputAsset.TextAsset.text, inputAsset.Format);
            }
        }

        private void ImportTextFile(string text, GoogleDriveDownloadFormat format) {

            List<List<string>> rows;
            text = text.Replace("\r\n", "\n");
            if (format == GoogleDriveDownloadFormat.CSV) {
                rows = CsvReader.Parse(text);
            }
            else {
                rows = TsvReader.Parse(text);
            }
            var canBegin = false;

            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++) {
                var row = rows[rowIndex];
                var key = row[0];

                if (string.IsNullOrEmpty(key) || IsLineBreak(key) || row.Count <= 1) {
                    //Ignore empty lines in the sheet
                    continue;
                }

                if (!canBegin) {
                    //if (key.StartsWith("Polyglot") || key.StartsWith("PolyMaster") || key.StartsWith("BEGIN"))
                    if (key.StartsWith("Polyglot")) {
                        canBegin = true;
                        continue;
                    }
                }

                if (!canBegin) {
                    continue;
                }
                /*
                if (key.StartsWith("END"))
                {
                    break;
                }
                */

                string wordWithMaxLenght = "";
                for (int i = 2; i < row.Count; i++) {
                    if (wordWithMaxLenght.Length < row[i].Length) {
                        wordWithMaxLenght = row[i];
                    }
                }

                if (row[0] == "LANGUAGE_THIS") {
                    row.Add("Keys");
                    row.Add("English Reverted");
                    row.Add("Max Lenght");
                }
                row.Add(row[0]); // Debug_Keys
                row.Add(new string(row[2].ToCharArray().Reverse().ToArray())); // Debug_English_Reverted

                row.Add(wordWithMaxLenght); // Debug_Word_With_Max_Lenght

                //Remove key
                row.RemoveAt(0);
                //Remove description
                row.RemoveAt(0);

                if (_languageStrings.TryAdd(key, row)) {
                    continue;
                }
                Debug.Log($"The key '{key}' already exist, but is now overwritten");
                _languageStrings[key] = row;
            }
        }

        /// <summary>
        /// Checks if the current string is \r or \n
        /// </summary>
        /// <param name="currentString"></param>
        /// <returns></returns>
        public static bool IsLineBreak(string currentString) {

            return currentString.Length == 1 && (currentString[0] == '\r' || currentString[0] == '\n') ||
                   currentString.Length == 2 && currentString.Equals(Environment.NewLine);
        }

        public List<string> GetLanguages(string key) {

            return GetLanguages(key, Array.Empty<Language>());
        }

        /// <summary>
        /// Get all language strings on a given key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="supportedLanguages"></param>
        /// <returns></returns>
        public List<string> GetLanguages(string key, IReadOnlyList<Language> supportedLanguages) {

            if (_languageStrings.Count == 0) {
                ImportInputFiles();
            }

            if (string.IsNullOrEmpty(key) || !_languageStrings.TryGetValue(key, out var allLanguages)) {
                return _emptyList;
            }

            if (supportedLanguages.Count == 0) {
                return allLanguages;
            }

            // Filter the supported languages down to the supported ones
            var supportedLanguageStrings = new List<string>(supportedLanguages.Count);
            for (int index = 0; index < supportedLanguages.Count; index++) {
                var language = supportedLanguages[index];
                var supportedLanguage = (int)language;
                supportedLanguageStrings.Add(allLanguages[supportedLanguage]);
            }
            return supportedLanguageStrings;
        }

        public Dictionary<string, List<string>> GetLanguagesStartsWith(string key) {

            if (_languageStrings.Count == 0) {
                ImportInputFiles();
            }

            var multipleLanguageStrings = new Dictionary<string, List<string>>();
            foreach (var languageString in _languageStrings) {
                if (languageString.Key.ToLower().StartsWith(key.ToLower())) {
                    multipleLanguageStrings.Add(languageString.Key, languageString.Value);
                }
            }

            return multipleLanguageStrings;
        }

        public Dictionary<string, List<string>> GetLanguagesContains(string key) {

            if (_languageStrings.Count == 0) {
                ImportInputFiles();
            }

            var multipleLanguageStrings = new Dictionary<string, List<string>>();
            foreach (var languageString in _languageStrings) {
                if (languageString.Key.ToLower().Contains(key.ToLower())) {
                    multipleLanguageStrings.Add(languageString.Key, languageString.Value);
                }
            }

            return multipleLanguageStrings;
        }

        public List<string> GetKeys() {

            return _languageStrings.Keys.ToList();
        }

#if UNITY_EDITOR
        public void AppendEmptyKeyToSource(string key) {

            if (_languageStrings.ContainsKey(key)) {
                throw new ArgumentException($"The key '{key}' already exists.", nameof(key));
            }
            var inputAsset = _inputFiles[0];
            var numberOfLanguages = Enum.GetNames(typeof(Language)).Length;
            var row = new List<string>();
            for (int i = 0; i < numberOfLanguages; i++) {
                row.Add("");
            }
            string inputAssetPath = UnityEditor.AssetDatabase.GetAssetPath(inputAsset.TextAsset);

            _languageStrings.Add(key, row);
            List<string> csvRow = new List<string>();
            csvRow.Add(key);
            csvRow.Add($"To be translated key \"{key}\", added by editor tool.");
            csvRow.AddRange(row);

            CsvWriter.AppendRow(inputAssetPath, csvRow);
        }
#endif
    }
}
