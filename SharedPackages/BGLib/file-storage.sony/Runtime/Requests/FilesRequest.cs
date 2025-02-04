#if UNITY_PS4 || UNITY_PS5

using System.IO;

#if UNITY_PS4
using Sony.PS4.SaveData;
#elif UNITY_PS5
using Unity.SaveData.PS5.Info;
using Unity.SaveData.PS5.Mount;
#endif

public abstract class FilesRequest : FileOpsRequestBase {

    protected readonly string[] _fileNames;

    public override ulong blockSize => Mounting.MountRequest.BLOCKS_MIN;

    public FilesRequest(string[] fileNames, FileOperationResult result) : base(result) {

        _fileNames = fileNames;
    }

    protected string GetFilePath(int fileIndex) {

        return Path.Combine(_mountPoint.PathName.Data, _fileNames[fileIndex]);
    }
}
#endif // UNITY_PS4 || UNITY_PS5