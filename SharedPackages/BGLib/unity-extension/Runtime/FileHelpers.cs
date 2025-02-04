using System.Collections.Generic;
using UnityEngine.Networking;
using System.IO;
using System.Linq;

public static class FileHelpers {

    public const string kProtocolInfix = "://";

    public static bool PathIsUrl(string filePath) => filePath.Contains(kProtocolInfix);

    public static string GetEscapedURLForFilePath(string filePath) {

        if (PathIsUrl(filePath)) {
            return filePath;
        }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_ANDROID
        return "file:///" + UnityWebRequest.EscapeURL(filePath);
#else
        return "file://" + UnityWebRequest.EscapeURL(filePath);
#endif
    }

    public static string GetUniqueDirectoryNameByAppendingNumber(string dirName) {

        int i = 0;
        var newDirName = dirName;

        while (Directory.Exists(newDirName)) {
            i++;
            newDirName = $"{dirName} {i}";
        }
        return newDirName;
    }

    public static string[] GetFilePaths(string directoryPath, HashSet<string> extensions) {

        if (!Directory.Exists(directoryPath)) {
            return null;
        }

        var allFilePaths = Directory.GetFiles(directoryPath);
        var newFileList = new List<string>();

        foreach (string filePath in allFilePaths) {

            string extension = Path.GetExtension(filePath).Replace(".", "");
            if (extensions.Contains(extension)) {
                newFileList.Add(filePath);
            }
        }

        return newFileList.ToArray();
    }

    public static string[] GetFileNamesFromFilePaths(IEnumerable<string> filePaths) {

        return filePaths.Select(Path.GetFileName).ToArray();
    }
}
