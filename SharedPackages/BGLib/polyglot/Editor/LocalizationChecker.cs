using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using BGLib.Polyglot.Editor;
using BGLib.UnityExtension.Editor;
using Object = UnityEngine.Object;

public static class LocalizationChecker {

    private class SearchFile {
        public readonly string filename;
        public readonly List<SearchKey> keys;

        public SearchFile(string filename) {
            this.filename = filename;
            keys = new List<SearchKey>();
        }
    }

    private readonly struct SearchKey {

        public readonly string key;
        public readonly int line;

        public SearchKey(string key, int line) {
            this.key = key;
            this.line = line;
        }

        public override string ToString() {
            return $"key:{key} at line:{line.ToString()}";
        }
    }

    private static int FindWord(ref string source, int charIndex, string word) {

        int startIndex = charIndex;
        int endIndex = startIndex + word.Length;
        while (charIndex < endIndex) {
            if (source[charIndex] != word[charIndex - startIndex]) {
                return -1;
            }
            charIndex++;
        }
        return startIndex;
    }

    private static List<SearchKey> AllKeys(ref string str) {

        var keys = new List<SearchKey>();

        // Testing "Localization.Get" and "Localization.GetFormat" strings in code
        {
            var pattern = "Localization.Get\\(\"([a-zA-Z0-9_]*)\"|Localization.GetFormat\\(\"([a-zA-Z0-9_]*)\"";
            var matches = Regex.Matches(str, pattern);

            foreach (Match match in matches) {
                var localizationKey = match.Groups[1].Value;
                if (!string.IsNullOrEmpty(localizationKey)) {
                    var lineIndex = str.Substring(0, match.Index).Count(c => c == '\n') + 1;
                    keys.Add(new SearchKey(localizationKey, lineIndex));
                }

                localizationKey = match.Groups[2].Value;
                if (!string.IsNullOrEmpty(localizationKey)) {
                    var lineIndex = str.Substring(0, match.Index).Count(c => c == '\n') + 1;
                    keys.Add(new SearchKey(localizationKey, lineIndex));
                }
            }
        }

        // Testing [LocalizationKey] attribute strings in code
        {
            var pattern = "\\[LocalizationKey\\](\\s\\w*)*=\\s*\"([A-Z_]*)\"\\s*;";
            var matches = Regex.Matches(str, pattern);

            foreach (Match match in matches) {
                var localizationKey = match.Groups[2].Value;
                var lineIndex = str.Substring(0, match.Index).Count(c => c == '\n') + 1;

                keys.Add(new SearchKey(localizationKey, lineIndex));
            }
        }

        return keys;
    }

    static string GetRelativePath(string fileSpec, string folder) {

        var pathUri = new Uri(fileSpec);
        // Folders must end in a slash
        if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString())) {
            folder += Path.DirectorySeparatorChar;
        }
        var folderUri = new Uri(folder);
        return Uri.UnescapeDataString(
            folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar)
        );
    }

    public static List<UnityObjectWithDescription> FindMissingLocalizationKeysInScripts() {

        var files = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
        var localization = EditorLocalization.instance;
        var searchFiles = new List<SearchFile>();
        foreach (string file in files) {
            if (!File.Exists(file)) {
                continue;
            }
            string fileText = File.ReadAllText(file);
            var foundKeys = AllKeys(ref fileText);
            if (foundKeys.Count == 0) {
                continue;
            }
            SearchFile? missingKeyFile = null;
            foreach (var foundKey in foundKeys.Where(foundKey => !localization.KeyExist(foundKey.key))) {
                if (missingKeyFile == null) {
                    missingKeyFile = new SearchFile(file);
                    searchFiles.Add(missingKeyFile);
                }
                missingKeyFile.keys.Add(foundKey);
            }
        }

        var localizedReferenceDescriptions = new List<UnityObjectWithDescription>();
        foreach (var missingKeyFile in searchFiles) {
            string filePath = Path.Combine("Assets/", GetRelativePath(missingKeyFile.filename, Application.dataPath));
            var sourceCode = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
            foreach (var missingKey in missingKeyFile.keys) {
                localizedReferenceDescriptions.Add(
                    new UnityObjectWithDescription(
                        sourceCode,
                        $"file:'{missingKeyFile.filename}', key:'{missingKey.key}' at line:{missingKey.line.ToString()}"
                    )
                );
            }
        }
        return localizedReferenceDescriptions;
    }

    public static HashSet<string> FindAllUsedKeys() {

        var keys = new HashSet<string>();
        var files = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);

        for (int i = 0; i < files.Length; i++) {
            if (!File.Exists(files[i])) {
                continue;
            }

            string fileText = File.ReadAllText(files[i]);
            var foundKeys = AllKeys(ref fileText);

            foreach (var key in foundKeys) {
                keys.Add(key.key);
            }
        }

        return keys;
    }

    public static List<UnityObjectWithDescription> FindMissingLocalizationKeysInObjects(
        IEnumerable<Object> objects,
        HashSet<string>? keysToIgnore
    ) {

        var checker = new LocalizationKeyCheckerForUnityObjects(objects, keysToIgnore, new IgnoredAssemblies());
        return checker.CheckKey() ?? new List<UnityObjectWithDescription>();
    }

    public static List<UnityObjectWithDescription> FindMissingLocalizationKeysInScriptableObjects() {

        var scriptableObjectsToCheck = FindUnityObjectsHelper.FindAllScriptableObjectsInProject();
        return FindMissingLocalizationKeysInObjects(scriptableObjectsToCheck, keysToIgnore: null);
    }
}
