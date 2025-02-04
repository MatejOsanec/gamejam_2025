namespace BGLib.SaveDataCore {

    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// Service to handle periodically writing changed save files to disk.
    /// </summary>
    public class SaveDataFlushingService {

        private readonly HashSet<Object> blockingObjects = new();
        private List<ISaveDataHandler> _handlers = new();

        public void Register(ISaveDataHandler handler) {

            _handlers.Add(handler);
        }

        /// <summary>
        /// Checks all monitored handlers for changes and applies them to disk if there are no blockers present.
        /// </summary>
        /// <returns>Whether all saves succeeded</returns>
        public async Task<bool> FlushSaveFilesAsync() {

            if (blockingObjects.Count > 0) {
                Debug.Log($"Attempted to flush save files, but there were {blockingObjects.Count} objects preventing this from happening.");
                return false;
            }

            bool allSavesSucceeded = true;
            foreach (var handler in _handlers) {
                if (handler.GetState() != LoaderState.FileLoaded || !handler.instance.isDirty) {
                    continue;
                }

                var result = await handler.SaveAsync();
                if (result.IsError()) {
                    Debug.LogError($"Failed to save {handler.GetType()} with result {result}");
                    allSavesSucceeded = false;
                }
            }

            return allSavesSucceeded;
        }

        public async Task<bool> ResetChangesAsync() {

            bool success = true;
            foreach (var handler in _handlers) {
                if (handler.GetState() != LoaderState.FileLoaded || !handler.instance.isDirty) {
                    continue;
                }

                var result = await handler.ResetChangesAsync();
                if (result.IsError()) {
                    Debug.LogError($"Failed to reset {handler.GetType()} with result {result}");
                    success = false;
                }
            }

            return success;
        }

        /// <summary>
        /// Notifies the flushing service that flushing should temporarily be stopped.
        /// </summary>
        /// <param name="o">The object you want to track the blocker to. Usually "this"</param>
        /// <returns>Whether the object was added to the blocking stack. Returns false if it was already added</returns>
        public bool TrackSaveBlocker(Object o) => blockingObjects.Add(o);

        /// <summary>
        /// Notifies the flushing service that save flushes are safe to go again.
        /// </summary>
        /// <param name="o">The object you want to track the blocker to. Usually "this"</param>
        /// <returns>Whether the object was removed from the blocking stack</returns>
        public bool ReleaseSaveBlocker(Object o) => blockingObjects.Remove(o);

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only. <br />
        /// Checks all monitored handlers for changes and applies them to disk.
        /// </summary>
        /// <returns>Whether all saves succeeded</returns>
        public bool FlushSaveFiles() {

            if (blockingObjects.Count > 0) {
                Debug.Log($"Attempted to flush save files, but there were {blockingObjects.Count} objects preventing this from happening.");
                return false;
            }

            bool allSavesSucceeded = true;
            foreach (var handler in _handlers) {
                if (handler.GetState() != LoaderState.FileLoaded || !handler.instance.isDirty) {
                    continue;
                }

                var result = handler.Save();
                if (result.IsError()) {
                    Debug.LogError($"Failed to save {handler.GetType()} with result {result}");
                    allSavesSucceeded = false;
                }
            }

            return allSavesSucceeded;
        }
#endif // UNITY_EDITOR
    }
}
