#if UNITY_PS4 || UNITY_PS5

using System.IO;

#if UNITY_PS4
using Sony.PS4.SaveData;
#elif UNITY_PS5
using Unity.SaveData.PS5.Info;
using Unity.SaveData.PS5.Mount;
#endif

public abstract class FileRequest : FileOpsRequestBase {

    protected readonly string _fileName;

    public override ulong blockSize => Mounting.MountRequest.BLOCKS_MIN;

    public FileRequest(string fileName, FileOperationResult result) : base(result) {

        _fileName = fileName;
    }

    protected string GetFilePath() {

        return Path.Combine(_mountPoint.PathName.Data, _fileName);
    }
}
#endif // UNITY_PS4 || UNITY_PS5