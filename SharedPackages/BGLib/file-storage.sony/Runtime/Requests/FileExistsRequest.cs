#if UNITY_PS4 || UNITY_PS5
using System.IO;

#if UNITY_PS4
using Sony.PS4.SaveData;
#elif UNITY_PS5
using Unity.SaveData.PS5.Info;
using Unity.SaveData.PS5.Mount;
#endif

public class FileExistsRequest : FileRequest {

    public override Mounting.MountModeFlags mountFlags => Mounting.MountModeFlags.ReadOnly;

    public FileExistsRequest(string fileName, FileOperationResult result) : base(fileName, result) { }

    public override void DoFileOperations(Mounting.MountPoint mountPoint, FileOps.FileOperationResponse response) {

        base.DoFileOperations(mountPoint, response);

        string filePath = GetFilePath();

        FileExistsResult fileExistsResult = (FileExistsResult)result;
        fileExistsResult.fileExists = File.Exists(filePath);
    }
}
#endif // UNITY_PS4 || UNITY_PS5