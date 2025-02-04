#if UNITY_PS4 || UNITY_PS5
using System;
using System.IO;
using System.Text;
using UnityEngine;

#if UNITY_PS4
using Sony.PS4.SaveData;
#elif UNITY_PS5
using Unity.SaveData.PS5.Info;
using Unity.SaveData.PS5.Mount;
#endif

public class LoadFilesRequest : FilesRequest {

    public override Mounting.MountModeFlags mountFlags => Mounting.MountModeFlags.ReadOnly;

    public LoadFilesRequest(string[] fileNames, FileOperationResult result) : base(fileNames, result) { }

    public override void DoFileOperations(Mounting.MountPoint mountPoint, FileOps.FileOperationResponse response) {

        base.DoFileOperations(mountPoint, response);

        LoadFilesResult loadResult = (LoadFilesResult)result;
        loadResult.values = new string[_fileNames.Length];

        for (int i = 0; i < _fileNames.Length; i++) {
            string filePath = GetFilePath(i);
            if (File.Exists(filePath)) {
                loadResult.values[i] = File.ReadAllText(filePath, Encoding.UTF8);
            }
        }
    }
}
#endif // UNITY_PS4 || UNITY_PS5
