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

public class LoadFileRequest : FileRequest {

    public override Mounting.MountModeFlags mountFlags => Mounting.MountModeFlags.ReadOnly;

    public LoadFileRequest(string fileName, FileOperationResult result) : base(fileName, result) { }

    public override void DoFileOperations(Mounting.MountPoint mountPoint, FileOps.FileOperationResponse response) {

        base.DoFileOperations(mountPoint, response);

        string filePath = GetFilePath();

        if (File.Exists(filePath)) {
            LoadFileResult loadFileResult = (LoadFileResult)result;
            loadFileResult.value = File.ReadAllText(filePath, Encoding.UTF8);
        }
        else {
            Debug.Log($"file not found at path {filePath}");
        }
    }
}
#endif // UNITY_PS4 || UNITY_PS5
