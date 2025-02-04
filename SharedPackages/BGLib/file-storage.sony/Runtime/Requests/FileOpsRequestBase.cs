#if UNITY_PS4 || UNITY_PS5

#if UNITY_PS4
using Sony.PS4.SaveData;
#elif UNITY_PS5
using Unity.SaveData.PS5.Info;
using Unity.SaveData.PS5.Mount;
#endif

public abstract class FileOpsRequestBase : FileOps.FileOperationRequest {

    public FileOperationResult result => _result;

    protected Mounting.MountPoint _mountPoint;

    protected readonly FileOperationResult _result;

    public FileOpsRequestBase(FileOperationResult result) {

        _result = result;
    }

    public virtual ulong blockSize => Mounting.MountRequest.BLOCKS_MIN;
    public abstract Mounting.MountModeFlags mountFlags { get; }

    public override void DoFileOperations(Mounting.MountPoint mountPoint, FileOps.FileOperationResponse response) {

        _mountPoint = mountPoint;

        FileResponse responseBase = (FileResponse)response;
        responseBase.result = result;
    }
}
#endif // UNITY_PS4 || UNITY_PS5
