using System;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using BGLib.UnityExtension;
using BGLib.JsonExtension;

#nullable enable

public static class FileStorageExtensions {

    public const int kSizeInBytesUntilDeserializeWarning = 10000; // 10kb

    /// <summary>
    /// Synchronous file saving. This should be avoided, consider switching to Async version
    /// </summary>
    private static void SaveFile(this IFileStorage fileStorage, string fileName, string value, StoragePreference storageLocation) {

        AsyncHelper.RunSync(() => fileStorage.SaveFileAsync(fileName, value, storageLocation));
    }

    /// <summary>
    /// Synchronous file loading. This should be avoided, consider switching to Async version
    /// </summary>
    public static string? LoadFile(this IFileStorage fileStorage, string fileName, StoragePreference storageLocation) {

        return AsyncHelper.RunSync(() => fileStorage.LoadFileAsync(fileName, storageLocation));
    }

    /// <summary>
    /// Synchronous file existence checking. This should be avoided, consider switching to Async version in IFileStorage
    /// </summary>
    public static bool FileExists(this IFileStorage fileStorage, string fileName, StoragePreference storageLocation) {

        return AsyncHelper.RunSync(() => fileStorage.FileExistsAsync(fileName, storageLocation));
    }

    /// <summary>
    /// Synchronous file deletion. This should be avoided, consider switching to Async version
    /// </summary>
    public static void DeleteFile(this IFileStorage fileStorage, string fileName, StoragePreference storageLocation) {

        AsyncHelper.RunSync(() => fileStorage.DeleteFileAsync(fileName, storageLocation));
    }

    /// <summary>
    /// saves obj into JSON as fileName
    /// </summary>
    /// <param name="overrideSerializerSettings">Settings on how to serialize the file. Use a collection from JsonSettings</param>
    public static void SaveToJSONFile(this IFileStorage fileStorage, object obj, string fileName, StoragePreference storageLocation, JsonSerializerSettings? overrideSerializerSettings = null) {

        JsonSerializerSettings serializerSettings = overrideSerializerSettings ?? JsonSettings.compactWithDefault;

        try {
            string json = JsonConvert.SerializeObject(obj, serializerSettings);
            fileStorage.SaveFile(fileName, json, storageLocation);
        }
        catch (Exception e) {
            Debug.LogWarning(e);
        }
    }

    /// <summary>
    /// Loads file JSON text and deserializes it to T
    /// </summary>
    public static T? LoadFromJSONFile<T>(this IFileStorage fileStorage, string fileName, StoragePreference storageLocation) where T : class {

        var json = fileStorage.LoadFile(fileName, storageLocation);
        if (json == null) {
            return null;
        }

        try {
#if UNITY_EDITOR
            if (System.Text.ASCIIEncoding.Unicode.GetByteCount(json) > kSizeInBytesUntilDeserializeWarning) {
                Debug.LogWarning($"JSON file ({storageLocation}/{fileName}) loaded is over 10kb and you are attempting to deserialize with Newtonsoft Json. This may be detrimental to performance.");
            }
#endif

            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (Exception e) {
            Debug.LogWarning($"Exception in json loading:\n{e}");
            return null;
        }
    }

    /// <summary>
    /// saves obj into JSON as fileName async
    /// </summary>
    /// <param name="overrideSerializerSettings">Settings on how to serialize the file. Use a collection from JsonSettings</param>
    public static Task SaveToJSONFileAsync(this IFileStorage fileStorage, object obj, string fileName, StoragePreference storageLocation, JsonSerializerSettings? overrideSerializerSettings = null) {

        JsonSerializerSettings serializerSettings = overrideSerializerSettings ?? JsonSettings.compactWithDefault;

        string json = JsonConvert.SerializeObject(obj, serializerSettings);
        return fileStorage.SaveFileAsync(fileName, json, storageLocation);
    }

    /// <summary>
    /// Loads file JSON text and deserializes it to T async
    /// </summary>
    public static async Task<T?> LoadFromJSONFileAsync<T>(this IFileStorage fileStorage, string fileName, StoragePreference storageLocation) where T : class {

        string? json = await fileStorage.LoadFileAsync(fileName, storageLocation);
        if (json == null) {
            return null;
        }

#if UNITY_EDITOR
        if (System.Text.ASCIIEncoding.Unicode.GetByteCount(json) > kSizeInBytesUntilDeserializeWarning) {
            Debug.LogWarning($"JSON file ({storageLocation}/{fileName}) loaded is over 10kb and you are attempting to deserialize with Newtonsoft Json. This may be detrimental to performance.");
        }
#endif

        return JsonConvert.DeserializeObject<T>(json);
    }
}
