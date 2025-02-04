#if UNITY_PS4 || UNITY_PS5
using System.IO;
using UnityEngine;

#if UNITY_PS4
using Sony.PS4.SaveData;
#elif UNITY_PS5
using Unity.SaveData.PS5.Info;
using Unity.SaveData.PS5.Mount;
#endif

public class DeleteFileRequest : FileRequest {

    public override Mounting.MountModeFlags mountFlags => Mounting.MountModeFlags.ReadWrite;

    public DeleteFileRequest(string fileName, FileOperationResult result) : base(fileName, result) { }

    public override void DoFileOperations(Mounting.MountPoint mountPoint, FileOps.FileOperationResponse response) {

        base.DoFileOperations(mountPoint, response);

        string filePath = GetFilePath();

        if (File.Exists(filePath)) {
            File.Delete(filePath);
        }
    }
}
#endif // UNITY_PS4 || UNITY_PS5