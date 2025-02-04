namespace BGLib.UnityExtension.Editor {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public static class AssetDatabaseExtensions {

        private const string kPackagesPrefix = "Packages/";

        public readonly struct AssetEntry<T> where T : UnityEngine.Object {

            public readonly string guid;
            public readonly string path;

            public AssetEntry(string guid) {

                this.guid = guid;
                this.path = AssetDatabase.GUIDToAssetPath(guid);
            }

            public bool isBeatAsset => !path.StartsWith("Packages") || path.StartsWith("Packages/com.beat");

            public T Load() {

                return AssetDatabase.LoadAssetAtPath<T>(path);
            }

            public IEnumerable<T> LoadAll() {

                foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(path)) {
                    if (obj is T asset) {
                        yield return asset;
                    }
                }
            }
        }

        public static IEnumerable<AssetEntry<T>> FindAssetEntries<T>(string searchFilter, string[] searchInFolders = null)
            where T : UnityEngine.Object {

            return AssetDatabase.FindAssets(searchFilter, searchInFolders)
                .Select(guid => new AssetEntry<T>(guid));
        }

        public static IEnumerable<AssetEntry<T>> FindAssetEntries<T>(string[] searchInFolders = null)
            where T : UnityEngine.Object {

            return AssetDatabase.FindAssets($"t:{typeof(T).Name}", searchInFolders)
                .Select(guid => new AssetEntry<T>(guid));
        }

        public static IEnumerable<AssetEntry<T>> FindAssetEntriesWithName<T>(string assetName, string[] searchInFolders = null)
            where T : UnityEngine.Object {

            return FindAssetEntries<T>($"t:{typeof(T).Name} {assetName}", searchInFolders)
                .Where(asset => Path.GetFileNameWithoutExtension(asset.path) == assetName);
        }

        public static IEnumerable<T> LoadAll<T>(this IEnumerable<AssetEntry<T>> assetEntries)
            where T : UnityEngine.Object {

            foreach (var asset in assetEntries) {
                foreach (var instance in asset.LoadAll()) {
                    yield return instance;
                }
            }
        }

        public static void SaveAsset(UnityEngine.Object unityObject) {

            EditorUtility.SetDirty(unityObject);
            AssetDatabase.SaveAssetIfDirty(unityObject);
        }

        public static T GetOrCreateScriptableObject<T>(string path) where T : ScriptableObject {

            var so = AssetDatabase.LoadAssetAtPath<T>(path);
            if (so == null) {
                so = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(so, path);
            }

            return so;
        }

        public static string GetPackageRoot(string assetPath) {

            if (!assetPath.StartsWith(kPackagesPrefix)) {
                throw new ArgumentException($"Path {assetPath} should start with {kPackagesPrefix}");
            }

            var rootSeparatorIndex = assetPath.IndexOf('/', kPackagesPrefix.Length);
            if (rootSeparatorIndex == -1) {
                throw new ArgumentException($"Path {assetPath} should contain package id");
            }

            return assetPath.Substring(0, rootSeparatorIndex);
        }

        public static string GetPackageRoot(UnityEngine.Object unityObject)
            => GetPackageRoot(AssetDatabase.GetAssetPath(unityObject));

        public static T LoadAssetAtGUID<T>(string GUID) where T : UnityEngine.Object
            => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(GUID));
    }
}
