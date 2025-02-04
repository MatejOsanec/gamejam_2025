#if UNITY_PS4 || UNITY_PS5
using System.IO;

#if UNITY_PS4
using Sony.PS4.SaveData;
#elif UNITY_PS5
using Unity.SaveData.PS5.Info;
using Unity.SaveData.PS5.Mount;
#endif

public class FilesExistRequest : FilesRequest {

    public override Mounting.MountModeFlags mountFlags => Mounting.MountModeFlags.ReadOnly;

    public FilesExistRequest(string[] fileNames, FileOperationResult result) : base(fileNames, result) { }

    public override void DoFileOperations(Mounting.MountPoint mountPoint, FileOps.FileOperationResponse response) {

        base.DoFileOperations(mountPoint, response);

        FilesExistResult fileExistsResult = (FilesExistResult)result;
        fileExistsResult.filesExist = new bool[_fileNames.Length];
        for (int i = 0; i < _fileNames.Length; i++) {
            fileExistsResult.filesExist[i] = File.Exists(GetFilePath(i));
        }
    }
}
#endif // UNITY_PS4 || UNITY_PS5