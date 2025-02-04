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

public class SaveRequest : FileRequest {

    private readonly string _value;

    public override ulong blockSize =>
        (ulong)Math.Max(
            Encoding.UTF8.GetBytes(_value).LongLength / Mounting.MountRequest.BLOCK_SIZE,
            Mounting.MountRequest.BLOCKS_MIN
        );

    public override Mounting.MountModeFlags mountFlags => Mounting.MountModeFlags.Create2 | Mounting.MountModeFlags.ReadWrite;

    public SaveRequest(string fileName, FileOperationResult result, string value) : base(fileName, result) {

        _value = value;
    }

    public override void DoFileOperations(Mounting.MountPoint mountPoint, FileOps.FileOperationResponse response) {

        try {
            base.DoFileOperations(mountPoint, response);
            string filePath = GetFilePath();
            using (FileStream fs = new FileStream(filePath, FileMode.Create)) {
                var bytes = Encoding.UTF8.GetBytes(_value);
                fs.Write(bytes, offset: 0, bytes.Length);
            }
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }
}
#endif // UNITY_PS4 || UNITY_PS5
