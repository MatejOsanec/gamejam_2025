namespace BGLib.UnityExtension {

    using System;
    using System.IO;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;

    public static class ExternalFileReader {

        public static async Task<bool> ExistsAsync(string filePath) {

            Debug.Log($"[ExternalFileReader] ExistsAsync Called on {filePath}");
            if (string.IsNullOrWhiteSpace(filePath)) {
                Debug.Log($"[ExternalFileReader] ExistsAsync Throw on {filePath}");
                throw new ArgumentNullException(nameof(filePath), "Provided an empty file path");
            }
            if (!FileHelpers.PathIsUrl(filePath)) {
                Debug.Log($"[ExternalFileReader] ExistsAsync File.Exists on {filePath}");
                return File.Exists(filePath);
            }
            Debug.Log($"[ExternalFileReader] ExistsAsync Start Web Request on {filePath}");
            using UnityWebRequest webRequest = UnityWebRequest.Get(filePath);
            webRequest.downloadHandler = null;
            var result = await webRequest.SendWebRequest();
            Debug.Log($"[ExternalFileReader] ExistsAsync Web Request Finished on {filePath}");
            return result == UnityWebRequest.Result.Success;
        }
    }
}
