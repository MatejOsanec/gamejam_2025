using UnityEngine;

public static class FileUtility {

    /// Returns the persistent data path location for the current platform.
    /// If `local: true`, it will be outside the cloud synchronized folder on Android/Quest devices to prevent back-ups.
    public static string GetPlatformPersistentDataPath(bool local = false) {

#if UNITY_PS5 // Do not change for PS4/5 as they specifically can only access user's disk through path "/host/..." as a so called file serving directory
        return "/host/";
#elif UNITY_PS4
        return "/hostapp/";
#else
        string path = Application.persistentDataPath;
#if UNITY_ANDROID && !UNITY_EDITOR
        if (local) {
            path = System.IO.Path.Combine(path, "..", "no_backup");
        }
#endif
        return path;
#endif
    }
}
