#if UNITY_PS4 || UNITY_PS5
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

#if UNITY_PS4
using Sony.PS4.SaveData;
using UnityEngine.PS4;
#elif UNITY_PS5
using Unity.SaveData.PS5;
using Unity.SaveData.PS5.Backup;
using Unity.SaveData.PS5.Core;
using Unity.SaveData.PS5.Delete;
using Unity.SaveData.PS5.Info;
using Unity.SaveData.PS5.Initialization;
using Unity.SaveData.PS5.Mount;
using Unity.SaveData.PS5.Search;
using UnityEngine.PS5;
#endif

public class SonyFileStorage : IFileStorage {

    // PS4/5 is very dangerous when it comes to SaveData.
    // Each save operation demands the following process
    // 1. Mount disk
    // 2. Do IO operations in disk
    // 3. Unmount disk
    // 4. Wait for backup if requested
    // Until all of these actions have happened it is not safe to start another one
    // Should it happen that we would not unmount then it breaks significantly the console, disallowing the user to turn it off resulting in cable unplug turnoff and corrupting save data

    private readonly BackgroundCommandQueue _commandQueue = new();
    private readonly int _userId;
    private readonly SaveDataAsyncEventScheduler _saveDataAsyncEventScheduler = new();

    public const string kSonyDefaultMountPoint = "Default";

    public SonyFileStorage() {

        InitSettings initSettings = new InitSettings() {
            Affinity = ThreadAffinity.AllCores
        };

        InitResult initResult = Main.Initialize(initSettings);
        if (!initResult.Initialized) {
            Debug.LogError("failed to initialize SaveFile Data Library");
        }

        _userId = Utility.initialUserId;
    }

    private class SonyFileCommand<T> : IBackgroundCommand where T : FileOperationResult {

        private readonly SonyFileStorage _sony;
        private readonly FileOpsRequestBase _request;
        private readonly TaskCompletionSource<T> _taskCompletionSource = new();

        public Task<T> resultTask => _taskCompletionSource.Task;

        public SonyFileCommand(SonyFileStorage sony, FileOpsRequestBase request) {

            _sony = sony;
            _request = request;
        }

        public async Task Execute() {

            try {
                Mounting.MountResponse mountResponse = await _sony.MountAsync(_request.mountFlags, _request.blockSize);
                if (mountResponse.IsErrorCode) {
                    await _sony.HandleMountErrorAsync(mountResponse);
                    _taskCompletionSource.SetResult((T) _request.result);
                    return;
                }

                _request.Async = true;
                _request.UserId = _sony._userId;
                _request.MountPointName = mountResponse.MountPoint.PathName;

                await _sony.DoFileOperationAsync(_request, new FileResponse());

                EmptyResponse unmountResponse = await _sony.UnmountAsync(mountResponse.MountPoint.PathName);
                if (unmountResponse.IsErrorCode) {
                    await _sony.HandleUnmountErrorAsync(mountResponse, unmountResponse);
                }

                _taskCompletionSource.SetResult((T)_request.result);
            }
            catch (Exception e) {
                _taskCompletionSource.SetException(e);
            }
        }
    }

    public Task SaveFileAsync(string fileName, string value, StoragePreference storageLocation) {

        var command = new SonyFileCommand<FileOperationResult>(this, new SaveRequest(fileName, new FileOperationResult(), value));
        _commandQueue.Enqueue(command);
        return command.resultTask;
    }

    public async Task<string> LoadFileAsync(string fileName, StoragePreference storageLocation) {

        var command = new SonyFileCommand<LoadFileResult>(this, new LoadFileRequest(fileName, new LoadFileResult()));
        _commandQueue.Enqueue(command);
        return (await command.resultTask).value;
    }

    public Task DeleteFileAsync(string fileName, StoragePreference storageLocation) {

        var command = new SonyFileCommand<FileOperationResult>(this, new DeleteFileRequest(fileName, new FileOperationResult()));
        _commandQueue.Enqueue(command);
        return command.resultTask;
    }

    public async Task<bool> FileExistsAsync(string fileName, StoragePreference storageLocation) {

        var command = new SonyFileCommand<FileExistsResult>(this, new FileExistsRequest(fileName, new FileExistsResult()));
        _commandQueue.Enqueue(command);
        var fileExistsResult = await command.resultTask;
        return fileExistsResult.fileExists;
    }

    public Task SaveFilesAsync(string[] fileNames, string[] values) {

        Assert.AreEqual(fileNames.Length, values.Length);
        var command = new SonyFileCommand<FileOperationResult>(this, new SaveFilesRequest(fileNames, new FileOperationResult(), values));
        _commandQueue.Enqueue(command);
        return command.resultTask;
    }

    public async Task<string[]> LoadFilesAsync(string[] fileNames) {

        var command = new SonyFileCommand<LoadFilesResult>(this, new LoadFilesRequest(fileNames, new LoadFilesResult()));
        _commandQueue.Enqueue(command);
        return (await command.resultTask).values;
    }

    public Task DeleteFilesAsync(string[] fileNames) {

        var command = new SonyFileCommand<FileOperationResult>(this, new DeleteFilesRequest(fileNames, new FileOperationResult()));
        _commandQueue.Enqueue(command);
        return command.resultTask;
    }

    public async Task<bool[]> FilesExistAsync(string[] fileNames) {

        var command = new SonyFileCommand<FilesExistResult>(this, new FilesExistRequest(fileNames, new FilesExistResult()));
        _commandQueue.Enqueue(command);
        var filesExistsResult = await command.resultTask;
        // Ensure that we do not return null
        filesExistsResult.filesExist ??= Array.Empty<bool>();
        return  filesExistsResult.filesExist;
    }

    private void HandleUnmountError(Mounting.MountResponse mountResponse, EmptyResponse unmountResponse) {

        if (unmountResponse.ReturnCode == ReturnCodes.DATA_ERROR_NO_SPACE_FS) {
            HandleOutOfSpace(mountResponse);
        }
    }

    private Task HandleUnmountErrorAsync(Mounting.MountResponse mountResponse, EmptyResponse unmountResponse) {

        if (unmountResponse.ReturnCode == ReturnCodes.DATA_ERROR_NO_SPACE_FS) {
            return HandleOutOfSpaceAsync(mountResponse);
        }

        return Task.CompletedTask;
    }

    private void HandleMountError(Mounting.MountResponse mountResponse) {

        switch (mountResponse.ReturnCode) {
            case ReturnCodes.SAVE_DATA_ERROR_BROKEN:
                HandleDataCorrupted(mountResponse.MountPoint);
                break;
            case ReturnCodes.DATA_ERROR_NO_SPACE_FS:
                HandleOutOfSpace(mountResponse);
                break;
        }
    }

    private Task HandleMountErrorAsync(Mounting.MountResponse mountResponse) {

        switch (mountResponse.ReturnCode) {
            case ReturnCodes.SAVE_DATA_ERROR_BROKEN:
                return HandleDataCorruptedAsync(mountResponse.MountPoint);
            case ReturnCodes.DATA_ERROR_NO_SPACE_FS:
                return HandleOutOfSpaceAsync(mountResponse);
        }

        return Task.CompletedTask;
    }

    private void HandleOutOfSpace(Mounting.MountResponse mountResponse) {

        DirName[] outOfSpaceDirNames = {
            mountResponse.MountPoint.DirName
        };
        OutOfSpaceDialog.Show(_saveDataAsyncEventScheduler, _userId, outOfSpaceDirNames, mountResponse.RequiredBlocks, useAnimations: true);
    }

    private Task HandleOutOfSpaceAsync(Mounting.MountResponse mountResponse) {

        DirName[] outOfSpaceDirNames = {
            mountResponse.MountPoint.DirName
        };
        return OutOfSpaceDialog.ShowAsync(_saveDataAsyncEventScheduler, _userId, outOfSpaceDirNames, mountResponse.RequiredBlocks, useAnimations: true);
    }

#if UNITY_PS4
    private EmptyResponse Unmount(Mounting.MountPointName mountPointName, bool doBackup = false) {

        Mounting.UnmountRequest unmountRequest = new Mounting.UnmountRequest() {
            UserId = _userId,
            Async = false,
            Backup = doBackup,
            MountPointName = mountPointName,
        };

        EmptyResponse emptyResponse = new EmptyResponse();

        try {
            Mounting.Unmount(unmountRequest, emptyResponse);
        }
        catch (SaveDataException exception) {
            Debug.LogError($"error when unmounting with message: {exception.ExtendedMessage}");
        }

        return emptyResponse;
    }

    private async Task<EmptyResponse> UnmountAsync(Mounting.MountPointName mountPointName, bool doBackup = false) {

        Mounting.UnmountRequest unmountRequest = new Mounting.UnmountRequest() {
            UserId = _userId,
            Async = true,
            Backup = doBackup,
            MountPointName = mountPointName,
        };

        EmptyResponse unmountResponse = await _saveDataAsyncEventScheduler.InvokeAsyncEvent(Mounting.Unmount, unmountRequest, new EmptyResponse());
        // If not error and we create a backup, we need to wait for the backup to finish
        if (doBackup && !unmountResponse.IsErrorCode) {
            TaskCompletionSource<EmptyResponse> taskCompletionSource = new TaskCompletionSource<EmptyResponse>();
            void FinishUnmountAfterBackup(ResponseBase backupResponse) {
                _saveDataAsyncEventScheduler.UnregisterNotification(FunctionTypes.NotificationUnmountWithBackup, FinishUnmountAfterBackup);
                taskCompletionSource.TrySetResult((EmptyResponse)backupResponse);
            }

            // Wait for the backup finished event
            _saveDataAsyncEventScheduler.RegisterNotification(
                FunctionTypes.NotificationUnmountWithBackup,
                FinishUnmountAfterBackup
            );
            return await taskCompletionSource.Task;
        }

        return unmountResponse;
    }

    private void HandleDataCorrupted(Mounting.MountPoint mountPoint) {

        Backups.CheckBackupResponse checkBackupResponse = CheckBackup(mountPoint);
        bool wasBackupFound = true;
        if (checkBackupResponse.IsErrorCode) {
            if (checkBackupResponse.ReturnCode == ReturnCodes.SAVE_DATA_ERROR_NOT_FOUND) {
                wasBackupFound = false;
            }
        }

        DirName[] dirNames = {
            mountPoint.DirName
        };

        CorruptedSaveDialog.Show(_saveDataAsyncEventScheduler, _userId, dirNames, canRestoreDirectories: wasBackupFound);

        if (wasBackupFound) {
            RestoreBackup(mountPoint);
        }
        else {
            Delete(mountPoint);
        }
    }

    private async Task HandleDataCorruptedAsync(Mounting.MountPoint mountPoint) {

        Backups.CheckBackupResponse checkBackupResponse = await CheckBackupAsync(mountPoint);
        bool wasBackupFound = true;
        if (checkBackupResponse.IsErrorCode) {
            if (checkBackupResponse.ReturnCode == ReturnCodes.SAVE_DATA_ERROR_NOT_FOUND) {
                wasBackupFound = false;
            }
        }

        DirName[] dirNames = {
            mountPoint.DirName
        };

        await CorruptedSaveDialog.ShowAsync(_saveDataAsyncEventScheduler, _userId, dirNames, canRestoreDirectories: wasBackupFound);

        if (wasBackupFound) {
            await RestoreBackupAsync(mountPoint);
        }
        else {
            await DeleteAsync(mountPoint);
        }
    }

#elif UNITY_PS5
    private EmptyResponse Unmount(Mounting.MountPointName mountPointName) {

        Mounting.UnmountRequest unmountRequest = new Mounting.UnmountRequest() {
            UserId = _userId,
            Async = false,
            MountPointName = mountPointName,
        };

        EmptyResponse emptyResponse = new EmptyResponse();

        try {
            Mounting.Unmount(unmountRequest, emptyResponse);
        }
        catch (SaveDataException exception) {
            Debug.LogError($"error when unmounting with message: {exception.ExtendedMessage}");
        }

        return emptyResponse;
    }

    private Task<EmptyResponse> UnmountAsync(Mounting.MountPointName mountPointName, bool doBackup = false) {

        Mounting.UnmountRequest unmountRequest = new Mounting.UnmountRequest() {
            UserId = _userId,
            Async = true,
            MountPointName = mountPointName,
        };

        return _saveDataAsyncEventScheduler.InvokeAsyncEvent(Mounting.Unmount, unmountRequest, new EmptyResponse());
    }

    private void HandleDataCorrupted(Mounting.MountPoint mountPoint) {

        DirName[] dirNames = {
            mountPoint.DirName
        };

        CorruptedSaveDialog.Show(_saveDataAsyncEventScheduler, _userId, dirNames, canRestoreDirectories: false);
    }

    private Task HandleDataCorruptedAsync(Mounting.MountPoint mountPoint) {

        DirName[] dirNames = {
            mountPoint.DirName
        };

        return CorruptedSaveDialog.ShowAsync(_saveDataAsyncEventScheduler, _userId, dirNames, canRestoreDirectories: false);
    }
#endif

    private Response DoFileOperation<Request, Response>(Request request, Response response) where Request : FileRequest where Response : FileResponse {

        try {
            FileOps.CustomFileOp(request, response);
        }
        catch (SaveDataException exception) {
            Debug.LogError($"error when doing custom file operation with message {exception.ExtendedMessage}");
        }

        return response;
    }

    private Task<Response> DoFileOperationAsync<Request, Response>(Request request, Response response) where Request : FileOps.FileOperationRequest where Response : FileResponse {

        return _saveDataAsyncEventScheduler.InvokeAsyncEvent(FileOps.CustomFileOp, request, response);
    }

    private Mounting.MountResponse Mount(Mounting.MountModeFlags mountMode, ulong blockSize = Mounting.MountRequest.BLOCKS_MIN) {

        DirName dirName = new DirName() {
            Data = kSonyDefaultMountPoint
        };

        Mounting.MountRequest mountRequest = new Mounting.MountRequest() {
            UserId = _userId,
            Async = false,
            Blocks = blockSize,
            MountMode = mountMode,
            DirName = dirName,
        };

        Mounting.MountResponse mountResponse = new Mounting.MountResponse();

        try {
            Mounting.Mount(mountRequest, mountResponse);
        }
        catch (SaveDataException exception) {
            Debug.LogError($"error when mounting with message: {exception.ExtendedMessage}");
        }

        return mountResponse;
    }

    private Task<Mounting.MountResponse> MountAsync(Mounting.MountModeFlags mountMode, ulong blockSize = Mounting.MountRequest.BLOCKS_MIN) {

        DirName dirName = new DirName() {
            Data = kSonyDefaultMountPoint
        };

        Mounting.MountRequest mountRequest = new Mounting.MountRequest() {
            UserId = _userId,
            Async = true,
            Blocks = blockSize,
            MountMode = mountMode,
            DirName = dirName,
        };

        Mounting.MountResponse mountResponse = new Mounting.MountResponse();

        return _saveDataAsyncEventScheduler.InvokeAsyncEvent(Mounting.Mount, mountRequest, mountResponse);
    }

    private EmptyResponse Delete(Mounting.MountPoint mountPoint) {

        Deleting.DeleteRequest deleteRequest = new Deleting.DeleteRequest() {
            UserId = _userId,
            Async = true,
            DirName = mountPoint.DirName,
        };

        EmptyResponse deleteResponse = new EmptyResponse();

        try {
            Deleting.Delete(deleteRequest, deleteResponse);
        }
        catch (SaveDataException exception) {
            Debug.LogError($"error when deleting corrupted save with message: {exception.ExtendedMessage}");
        }

        return deleteResponse;
    }

    private Task<EmptyResponse> DeleteAsync(Mounting.MountPoint mountPoint) {

        Deleting.DeleteRequest deleteRequest = new Deleting.DeleteRequest() {
            UserId = _userId,
            Async = true,
            DirName = mountPoint.DirName,
        };

        return _saveDataAsyncEventScheduler.InvokeAsyncEvent(Deleting.Delete, deleteRequest, new EmptyResponse());
    }

    public async Task<bool> HasDirectoryAsync(string directoryName) {

        Searching.DirNameSearchRequest dirNameSearchRequest = new Searching.DirNameSearchRequest() {
            UserId = _userId,
            Async = true,
            Key = Searching.SearchSortKey.DirName,
            Order = Searching.SearchSortOrder.Ascending,
            IncludeBlockInfo = false,
            IncludeParams = false,
            MaxDirNameCount = Searching.DirNameSearchRequest.DIR_NAME_MAXSIZE,
        };

        var searchDirectoriesResponse = await _saveDataAsyncEventScheduler.InvokeAsyncEvent(Searching.DirNameSearch, dirNameSearchRequest, new Searching.DirNameSearchResponse());
        if (searchDirectoriesResponse.SaveDataItems == null) {
            return false;
        }

        foreach (var searchSaveDataItem in searchDirectoriesResponse.SaveDataItems) {
            if (searchSaveDataItem.DirName.Data == directoryName) {
                return true;
            }
        }

        return false;
    }

    public bool HasDirectory(string directoryName) {

        Searching.DirNameSearchRequest dirNameSearchRequest = new Searching.DirNameSearchRequest() {
            UserId = _userId,
            Async = false,
            Key = Searching.SearchSortKey.DirName,
            Order = Searching.SearchSortOrder.Ascending,
            IncludeBlockInfo = false,
            IncludeParams = false,
            MaxDirNameCount = Searching.DirNameSearchRequest.DIR_NAME_MAXSIZE,
        };

        var searchDirectoriesResponse = new Searching.DirNameSearchResponse();
        try {
            Searching.DirNameSearch(dirNameSearchRequest, searchDirectoriesResponse);
        }
        catch (SaveDataException exception) {
            Debug.LogError($"error when searching directories with message: {exception.ExtendedMessage}");
        }

        if (searchDirectoriesResponse.SaveDataItems == null) {
            return false;
        }

        foreach (var searchSaveDataItem in searchDirectoriesResponse.SaveDataItems) {
            if (searchSaveDataItem.DirName.Data == directoryName) {
                return true;
            }
        }

        return false;
    }

#if UNITY_PS4
    private Backups.CheckBackupResponse CheckBackup(Mounting.MountPoint mountPoint) {

        Backups.CheckBackupRequest checkBackupRequest = new Backups.CheckBackupRequest() {
            UserId = _userId,
            Async = false,
            IncludeParams = true,
            IncludeIcon = true,
            DirName = mountPoint.DirName,
        };

        Backups.CheckBackupResponse checkBackupResponse = new Backups.CheckBackupResponse();

        try {
            Backups.CheckBackup(checkBackupRequest, checkBackupResponse);
        }
        catch (SaveDataException exception) {
            Debug.LogError($"error when checking backup with message: {exception.ExtendedMessage}");
        }

        return checkBackupResponse;
    }

    private Task<Backups.CheckBackupResponse> CheckBackupAsync(Mounting.MountPoint mountPoint) {

        Backups.CheckBackupRequest checkBackupRequest = new Backups.CheckBackupRequest() {
            UserId = _userId,
            Async = true,
            IncludeParams = true,
            IncludeIcon = true,
            DirName = mountPoint.DirName,
        };

        return _saveDataAsyncEventScheduler.InvokeAsyncEvent(Backups.CheckBackup, checkBackupRequest, new Backups.CheckBackupResponse());
    }

    private EmptyResponse RestoreBackup(Mounting.MountPoint mountPoint) {

        Backups.RestoreBackupRequest restoreBackupRequest = new Backups.RestoreBackupRequest() {
            UserId = _userId,
            Async = true,
            DirName = mountPoint.DirName,
        };

        EmptyResponse restoreBackupResponse = new EmptyResponse();

        try {
            Backups.RestoreBackup(restoreBackupRequest, restoreBackupResponse);
        }
        catch (SaveDataException exception) {
            Debug.LogError($"error when restoring backup with message: {exception.ExtendedMessage}");
        }

        return restoreBackupResponse;
    }

    private Task<EmptyResponse> RestoreBackupAsync(Mounting.MountPoint mountPoint) {

        Backups.RestoreBackupRequest restoreBackupRequest = new Backups.RestoreBackupRequest() {
            UserId = _userId,
            Async = true,
            DirName = mountPoint.DirName,
        };

        return _saveDataAsyncEventScheduler.InvokeAsyncEvent(Backups.RestoreBackup, restoreBackupRequest, new EmptyResponse());
    }
#elif UNITY_PS5
    private EmptyResponse Backup(Mounting.MountPoint mountPoint) {

        Backups.BackupRequest restoreBackupRequest = new Backups.BackupRequest() {
            UserId = _userId,
            Async = true,
            DirName = mountPoint.DirName,
        };

        EmptyResponse restoreBackupResponse = new EmptyResponse();

        try {
            Backups.Backup(restoreBackupRequest, restoreBackupResponse);
        }
        catch (SaveDataException exception) {
            Debug.LogError($"error when restoring backup with message: {exception.ExtendedMessage}");
        }

        return restoreBackupResponse;
    }

    private Task<EmptyResponse> BackupAsync(Mounting.MountPoint mountPoint) {

        Backups.BackupRequest restoreBackupRequest = new Backups.BackupRequest() {
            UserId = _userId,
            Async = true,
            DirName = mountPoint.DirName,
        };

        return _saveDataAsyncEventScheduler.InvokeAsyncEvent(Backups.Backup, restoreBackupRequest, new EmptyResponse());
    }
#endif
}
#endif // UNITY_PS4 || UNITY_PS5
