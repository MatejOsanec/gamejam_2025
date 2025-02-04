using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

#nullable enable

/// <summary>
/// Use Task.Run() on each of the async calls because otherwise any main thread wait on the semaphore would deadlock main thread
/// IO operations are done in parallel for distinct file names,
/// but IOs done on the same file are done sequentially to avoid race conditions using a semaphore
/// </summary>
public class FileSystemFileStorage : IFileStorage {

    private readonly string _persistentDataPath = Application.persistentDataPath;

    /// <summary>
    /// Per file command queue to ensure we do not get race conditions of multiple access to the same file
    /// File Name, File Lock>
    /// </summary>
    private readonly ConcurrentDictionary<string, BackgroundCommandQueue> _commandQueueMap = new();

    public Task SaveFileAsync(string fileName, string value, StoragePreference storageLocation) {

        var command = new SaveFileCommand(GetFilePath(fileName, storageLocation), value);
        GetCommandQueue(fileName).Enqueue(command);
        return command.resultTask;
    }

    public Task<string?> LoadFileAsync(string fileName, StoragePreference storageLocation) {

        var command = new LoadFileCommand(GetFilePath(fileName, storageLocation));
        GetCommandQueue(fileName).Enqueue(command);
        return command.resultTask;
    }

    public Task DeleteFileAsync(string fileName, StoragePreference storageLocation) {

        var command = new DeleteFileCommand(GetFilePath(fileName, storageLocation));
        GetCommandQueue(fileName).Enqueue(command);
        return command.resultTask;
    }

    public Task<bool> FileExistsAsync(string fileName, StoragePreference storageLocation) {

        var command = new FileExistsCommand(GetFilePath(fileName, storageLocation));
        GetCommandQueue(fileName).Enqueue(command);
        return command.resultTask;
    }

    private BackgroundCommandQueue GetCommandQueue(string fileName) {

        return _commandQueueMap.GetOrAdd(fileName, _ => new BackgroundCommandQueue());
    }

    private string GetFilePath(string fileName, StoragePreference storageLocation) {

#if UNITY_ANDROID && !UNITY_EDITOR
        // Persistent data path is /sdcard/Android/data/<package-name>/files/
        return storageLocation switch {
            StoragePreference.Cloud => Path.Combine(_persistentDataPath, fileName),
            StoragePreference.Local => Path.Combine(_persistentDataPath, "..", "no_backup", fileName),
            _ => throw new System.NotImplementedException()
        };
#else
        return Path.Combine(_persistentDataPath, fileName);
#endif
    }

    private static string GetBackupFilePath(string filePath) {

        return filePath + ".bak";
    }

    private static string GetTempFilePath(string filePath) {

        return filePath + ".tmp";
    }

    private class SaveFileCommand : SyncBackgroundCommand {

        private readonly string _filePath;
        private readonly string _value;

        public SaveFileCommand(string filePath, string value) {

            _filePath = filePath;
            _value = value;
        }

        protected override void ExecuteInternal() {

            var dir = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }

            if (File.Exists(_filePath)) {
                string tempFilePath = GetTempFilePath(_filePath);
                string backupFilePath = GetBackupFilePath(_filePath);
                File.WriteAllText(tempFilePath, _value);
                File.Replace(tempFilePath, _filePath, backupFilePath);
            }
            else {
                File.WriteAllText(_filePath, _value);
            }
        }
    }

    private class LoadFileCommand : SyncBackgroundCommand<string?> {

        private readonly string _filePath;

        public LoadFileCommand(string filePath) {

            _filePath = filePath;
        }

        protected override string? ExecuteInternal() {

            var dir = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(dir)) {
                return null;
            }

            if (File.Exists(_filePath)) {
                return File.ReadAllText(_filePath);
            }

            string backupFilePath = GetBackupFilePath(_filePath);
            if (File.Exists(backupFilePath)) {
                return File.ReadAllText(backupFilePath);
            }

            return null;
        }
    }

    private class DeleteFileCommand : SyncBackgroundCommand {

        private readonly string _filePath;

        public DeleteFileCommand(string filePath) {

            _filePath = filePath;
        }

        protected override void ExecuteInternal() {

            File.Delete(_filePath);
            File.Delete(GetBackupFilePath(_filePath));
            File.Delete(GetTempFilePath(_filePath));
        }
    }

    private class FileExistsCommand : SyncBackgroundCommand<bool> {

        private readonly string _filePath;

        public FileExistsCommand(string filePath) {

            _filePath = filePath;
        }

        protected override bool ExecuteInternal() {

            if (File.Exists(_filePath)) {
                return true;
            }

            return File.Exists(GetBackupFilePath(_filePath));
        }
    }
}
