using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class JsonLoader
{
    public static AsyncOperationHandle<string> LoadJson(string filePath)
    {
        // Start loading the TextAsset via Addressables.
        AsyncOperationHandle<TextAsset> assetOp = Addressables.LoadAssetAsync<TextAsset>(filePath);
        // Declare the chain operation delegate explicitly.
        Func<AsyncOperationHandle<TextAsset>, AsyncOperationHandle<string>> chainFunc = op =>
        {
            if (op.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Failed to load beatmap: {filePath}");
                // Pass an error message string instead of an Exception.
                return Addressables.ResourceManager.CreateCompletedOperation<string>(null, "Failed to load beatmap.");
            }
            TextAsset asset = op.Result;
            string jsonString = asset.text; // Directly access the text content
            // No error; pass null for the error message.
            return Addressables.ResourceManager.CreateCompletedOperation(jsonString, null);
        };
        // Specify the autoReleaseHandle flag explicitly to resolve ambiguity.
        AsyncOperationHandle<string> chainHandle = Addressables.ResourceManager.CreateChainOperation(assetOp, chainFunc);
        return chainHandle;
    }
}