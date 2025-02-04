#if UNITY_PS4 || UNITY_PS5
using System.IO;

#if UNITY_PS4
using Sony.PS4.SaveData;
#elif UNITY_PS5
using Unity.SaveData.PS5.Info;
using Unity.SaveData.PS5.Mount;
#endif

public class DeleteFilesRequest : FilesRequest {

    public override Mounting.MountModeFlags mountFlags => Mounting.MountModeFlags.ReadWrite;

    public DeleteFilesRequest(string[] fileNames, FileOperationResult result) : base(fileNames, result) { }

    public override void DoFileOperations(Mounting.MountPoint mountPoint, FileOps.FileOperationResponse response) {

        base.DoFileOperations(mountPoint, response);

        for (int i = 0; i < _fileNames.Length; i++) {
            string filePath = GetFilePath(i);
            if (File.Exists(filePath)) {
                File.Delete(filePath);
            }
        }
    }
}
#endif // UNITY_PS4 || UNITY_PS5