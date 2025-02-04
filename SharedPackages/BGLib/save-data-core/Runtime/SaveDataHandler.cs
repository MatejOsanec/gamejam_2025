namespace BGLib.SaveDataCore {

    using System;
    using System.Threading.Tasks;
    using UnityEngine;
    using BGLib.JsonExtension;

    /// <summary>
    /// Stub so we can access SaveDataHandlers in SaveDataFlushingService.
    /// </summary>
    public interface ISaveDataHandler {

        public VersionableSaveData instance { get; }
        public LoaderState GetState();
        public Task<SaveDataResult> SaveAsync(bool force = false);
        public Task<SaveDataResult> ResetChangesAsync();
#if UNITY_EDITOR
        public SaveDataResult Load();
        public SaveDataResult Save(bool force = false);
        public void TestFullUpdateLoop();
#endif // UNITY_EDITOR
    }

    /// <summary>
    /// Save Data Loader is a container and manager for your save files. It keeps track of its current state and file loading.
    /// While it saves data just fine on its own, it can optionally plug in to SaveDataFlushingService to periodically flush saves to disk.
    /// </summary>
    /// <typeparam name="T">Subclass of VersionableSaveData</typeparam>
    public abstract class SaveDataHandler<T> : ISaveDataHandler where T : VersionableSaveData, new() {

        // Config
        protected virtual Version firstVersion => new Version("0.0.0");
        protected abstract Version version { get; }
        protected abstract string fileNameWithExtension { get; }
        protected abstract StoragePreference preferredStorageLocation { get; }

        // State
        public LoaderState state { get; private set; } = LoaderState.Unloaded;
        public T instance {
            get {
                if (state < LoaderState.FileLoaded) {
                    throw new Exception("Attempted to access instance before the save file was loaded.");
                }
                return _instance!;
            }
        }
        VersionableSaveData ISaveDataHandler.instance => instance;

        // Internal
        private IFileStorage _fileStorage;
        private Task<SaveDataResult>? _loadFileTask;
        private Task? _saveFileTask = null;
        private T? _instance;

        public SaveDataHandler (IFileStorage fileStorage) {

            _fileStorage = fileStorage;
        }

        /// <summary>
        /// Constructor supporting creation of handler with existing data.
        /// Your data will be considered dirty as we can't verify the accuracy of the data this way.
        /// </summary>
        public SaveDataHandler (IFileStorage fileStorage, T instance) {

            _fileStorage = fileStorage;
            _instance = instance;
            _instance.isDirty = true;
            state = LoaderState.FileLoaded;
        }

        /// <summary>
        /// Attempts to load the file, if it is not yet loaded already.
        /// </summary>
        /// <returns>Whether the file loaded successfully</returns>
        public async Task<SaveDataResult> LoadAsync() {

            switch (state) {
                case LoaderState.Unloaded:
                    _loadFileTask = InternalLoadAsync();
                    return await _loadFileTask;
                case LoaderState.Loading:
                    if (_loadFileTask == null) {
                        Debug.Log("Attempted to await load task already in progress, but it did not exist");
                        return SaveDataResult.AsyncLoadStateButNoTask;
                    }

                    return await _loadFileTask;
                case LoaderState.FileLoaded:
                    return SaveDataResult.OK;
            }

            return SaveDataResult.UnknownLoaderState;
        }

        public LoaderState GetState() {

            return state;
        }

        /// <summary>
        /// Directly saves the file to disk. Consider using SaveDataFlushingService instead. <br />
        /// Note that the file must be marked as Dirty, or be force saved, for it to actually save to disk.
        /// </summary>
        public async Task<SaveDataResult> SaveAsync(bool force = false) {

            switch (state) {
                case LoaderState.Unloaded:
                case LoaderState.Loading:
                    return SaveDataResult.LoadingNotCompleted;
                case LoaderState.FileLoaded:
                    if (_instance == null) {
                        Debug.Log("Could not save as there was no save file loaded (instance was null!).");
                        return SaveDataResult.NoInstanceToSave;
                    }

                    if (!force && !_instance.isDirty) {
                        return SaveDataResult.OK_NotDirty;
                    }

                    if (_saveFileTask is not null && _saveFileTask.IsCompleted is false) {
                        Debug.Log("Enqueueing a file save while another save operation is in progress.");
                    }

                    var saveFileTask = _fileStorage.SaveToJSONFileAsync(_instance, fileNameWithExtension, preferredStorageLocation, JsonSettings.readableWithDefault);
                    _saveFileTask = saveFileTask;
                    await saveFileTask;

                    _instance.isDirty = false;
                    return SaveDataResult.OK;
            }

            throw new Exception("SaveAsync encountered an unknown Loader State.");
        }

        /// <summary>
        /// Will attempt to discard current changes to the save file by reloading what is on disk.
        /// This will fail if the save file is still being loaded or is not yet loaded.
        /// </summary>
        /// <returns></returns>
        public async Task<SaveDataResult> ResetChangesAsync() {

            switch (state) {
                case LoaderState.Unloaded:
                    return SaveDataResult.AttemptedAccessWhileUnloaded;
                case LoaderState.Loading:
                    return SaveDataResult.AttemptedReloadWhileLoading;
                case LoaderState.FileLoaded:
                    state = LoaderState.Unloaded;
                    _loadFileTask = InternalLoadAsync();
                    return await _loadFileTask;
                default:
                    return SaveDataResult.UnknownLoaderState;
            }
        }

        protected virtual async Task<SaveDataResult> InternalLoadAsync() {

            state = LoaderState.Loading;

            bool exists = await _fileStorage.FileExistsAsync(fileNameWithExtension, preferredStorageLocation);
            if (!exists) {
                _instance = new();
                _instance.version = firstVersion.ToString();
                _instance.isDirty = true;
            }
            else {
                T? deserializedJson = await _fileStorage.LoadFromJSONFileAsync<T>(fileNameWithExtension, preferredStorageLocation);
                if (deserializedJson == null) {
                    Debug.Log($"Failed to load/deserialize {fileNameWithExtension}");
                    return SaveDataResult.FailedToLoadOrDeserialize;
                }

                UpdateVersionLoop(ref deserializedJson);
                _instance = deserializedJson;
                _instance.isDirty = false;
            }

            state = LoaderState.FileLoaded;
            return SaveDataResult.OK;
        }

        // TODO: What about when the structure has changed so much, that it needs a separate type to be able to deserialize?
        private SaveDataResult UpdateVersionLoop(ref T deserializedJson) {

            Version onDiskVersion = new Version(deserializedJson.version);
            while (onDiskVersion < version) {
                var methodName = $"UpdateFromVersion_{onDiskVersion.Major}_{onDiskVersion.Minor}_{onDiskVersion.Build}";
                var type = GetType();
                var methodExists = type.GetMethod(methodName) != null;
                if (!methodExists) {
                    Debug.Log($"Method with name {methodName} did not exist to update save file.");
                    return SaveDataResult.UpdateMethodDoesNotExist;
                }

                Version resultVersion = (Version)type.GetMethod(methodName).Invoke(this, new object[] { deserializedJson });
                if (resultVersion == onDiskVersion) {
                    Debug.Log($"Method {methodName} failed to update save file");
                    return SaveDataResult.UpdateMethodFailed;
                }

                Debug.Log($"Updated from version {onDiskVersion} to {resultVersion}");
                onDiskVersion = resultVersion;
                deserializedJson.version = resultVersion.ToString();
            }

            return SaveDataResult.OK;
        }

#if UNITY_EDITOR
        /// <summary>
        /// For test runs, triggers full update loop for the file and checks if we can make it to the current version without issues.
        /// </summary>
        public void TestFullUpdateLoop() {

            if (version == firstVersion) {
                return; // we good!
            }

            T newInstance = new();
            newInstance.version = firstVersion.ToString();
            var result = UpdateVersionLoop(ref newInstance);
            if (result.IsError()) {
                throw new Exception($"Failed test with code {result}");
            }
        }

        /// <summary>
        /// Synchronous editor-only variant of LoadAsync.
        /// </summary>
        /// <returns>success</returns>
        public virtual SaveDataResult Load() {

            switch (state) {
                case LoaderState.Unloaded:
                    state = LoaderState.Loading;
                    bool exists = _fileStorage.FileExists(fileNameWithExtension, preferredStorageLocation);
                    if (!exists) {
                        _instance = new();
                        _instance.version = firstVersion.ToString();
                        _instance.isDirty = true;
                    }
                    else {
                        T? deserializedJson = _fileStorage.LoadFromJSONFile<T>(fileNameWithExtension, preferredStorageLocation);
                        if (deserializedJson == null) {
                            Debug.Log($"Failed to load/deserialize {fileNameWithExtension}");
                            return SaveDataResult.FailedToLoadOrDeserialize;
                        }

                        UpdateVersionLoop(ref deserializedJson);
                        _instance = deserializedJson;
                        _instance.isDirty = false;
                    }

                    state = LoaderState.FileLoaded;
                    return SaveDataResult.OK;
                case LoaderState.Loading:
                    Debug.Log("Ignored Load() call as SaveDataHandler is already in loading state. Did you mix and match async/non-async calls?");
                    return SaveDataResult.SynchronousLoadAlreadyInLoadingState;
                case LoaderState.FileLoaded:
                    return SaveDataResult.OK;
            }

            return SaveDataResult.UnknownLoaderState;
        }

        /// <summary>
        /// Synchronous editor-only variant of SaveAsync.
        /// </summary>
        /// <returns>success</returns>
        public SaveDataResult Save(bool force = false) {

            switch (state) {
                case LoaderState.Unloaded:
                case LoaderState.Loading:
                    return SaveDataResult.LoadingNotCompleted;
                case LoaderState.FileLoaded:
                    if (_instance == null) {
                        Debug.Log("Could not save as there was no save file loaded (instance was null!).");
                        return SaveDataResult.NoInstanceToSave;
                    }

                    if (!force && !_instance.isDirty) {
                        return SaveDataResult.OK_NotDirty;
                    }

                    _fileStorage.SaveToJSONFile(_instance, fileNameWithExtension, preferredStorageLocation, JsonSettings.readableWithDefault);
                    _instance.isDirty = false;
                    return SaveDataResult.OK;
            }

            return SaveDataResult.UnknownLoaderState;
        }


        /// <summary>
        /// Editor-only function to delete the current save file.
        /// It is async, but can be treated as fire-and-forget.
        /// </summary>
        public async Task DeleteAsync() {

            state = LoaderState.Unloaded;
            await _fileStorage.DeleteFileAsync(fileNameWithExtension, preferredStorageLocation);
            await InternalLoadAsync();
        }

#else
        public virtual void TestFullUpdateLoop() {

            Debug.LogError("Called SaveDataHandler.TestFullUpdateLoop() outside of editor");
        }

        public virtual SaveDataResult Load() {

            Debug.LogError("Called SaveDataHandler.Load() outside of editor");
            return SaveDataResult.GenericError;
        }

        public SaveDataResult Save(bool force = false) {

            Debug.LogError("Called SaveDataHandler.Save() outside of editor");
            return SaveDataResult.GenericError;
        }

        public Task DeleteAsync() {

            Debug.LogError("Called SaveDataHandler.DeleteAsync() outside of editor");
            return Task.CompletedTask;
        }
#endif // UNITY_EDITOR
    }
}
