using System;
using System.IO;
using UnityEngine;
using UnityEditor; // Required for AssetDatabase and DefaultAsset
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class JsonLoader
{
    public static AsyncOperationHandle<string> LoadJson(string filePath)
    {
        // Start loading the DefaultAsset via Addressables.
        AsyncOperationHandle<DefaultAsset> assetOp = Addressables.LoadAssetAsync<DefaultAsset>(filePath);

        // Declare the chain operation delegate explicitly.
        Func<AsyncOperationHandle<DefaultAsset>, AsyncOperationHandle<string>> chainFunc = op =>
        {
            if (op.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Failed to load beatmap: {filePath}");
                // Pass an error message string instead of an Exception.
                return Addressables.ResourceManager.CreateCompletedOperation<string>(null, "Failed to load beatmap.");
            }

            DefaultAsset asset = op.Result;
            string path = AssetDatabase.GetAssetPath(asset);

            if (File.Exists(path))
            {
                string jsonString = File.ReadAllText(path);
                // No error; pass null for the error message.
                return Addressables.ResourceManager.CreateCompletedOperation(jsonString, null);
            }

            Debug.LogError($"File not found at path: {path}");
            // Pass an error message string for file not found.
            return Addressables.ResourceManager.CreateCompletedOperation<string>(null, $"File not found: {path}");
        };

        // Specify the autoReleaseHandle flag explicitly to resolve ambiguity.
        AsyncOperationHandle<string> chainHandle = Addressables.ResourceManager.CreateChainOperation(assetOp, chainFunc, false);

        return chainHandle;
    }
}