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

public class SaveFilesRequest : FilesRequest {

    private readonly string[] _values;

    public override ulong blockSize {

        get {

            long bufferSizes = 0;
            foreach (string value in _values) {
                bufferSizes += Encoding.UTF8.GetBytes(value).LongLength;
            }

            return (ulong)Math.Max(bufferSizes / Mounting.MountRequest.BLOCK_SIZE, Mounting.MountRequest.BLOCKS_MIN);
        }
    }

    public override Mounting.MountModeFlags mountFlags => Mounting.MountModeFlags.Create2 | Mounting.MountModeFlags.ReadWrite;

    public SaveFilesRequest(string[] fileNames, FileOperationResult result, string[] values) : base(fileNames, result) {

        _values = values;
    }

    public override void DoFileOperations(Mounting.MountPoint mountPoint, FileOps.FileOperationResponse response) {

        try {
            base.DoFileOperations(mountPoint, response);

            for (int i = 0; i < _fileNames.Length; i++) {
                string filePath = GetFilePath(i);
                using (FileStream fs = new FileStream(filePath, FileMode.Create)) {
                    var bytes = Encoding.UTF8.GetBytes(_values[i]);
                    fs.Write(bytes, offset: 0, bytes.Length);
                }
            }
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }
}
#endif // UNITY_PS4 || UNITY_PS5
