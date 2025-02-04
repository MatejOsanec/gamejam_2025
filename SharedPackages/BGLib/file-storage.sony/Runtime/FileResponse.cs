#if UNITY_PS4 || UNITY_PS5

#if UNITY_PS4
using Sony.PS4.SaveData;
#elif UNITY_PS5
using Unity.SaveData.PS5.Info;
#endif

public class FileResponse : FileOps.FileOperationResponse {

    public FileOperationResult result;
}
#endif // UNITY_PS4 || UNITY_PS5