using Beatmap;

namespace Core
{
    using System;
    using UnityEngine;
    using UnityEngine.ResourceManagement.AsyncOperations;
    public class AllBeatmapData
    {
        public AudioInfo AudioInfo { get; set; }
        public BeatmapData BeatmapData { get; set; }
    }
    
    public class BeatmapDataLoader
    {
        private Action<AllBeatmapData> onComplete;
        private AudioInfo audioInfo;
        private BeatmapData beatmapData;
        private int filesLoaded = 0;
        
        public void LoadBeatmapData(Action<AllBeatmapData> onComplete)
        {
            this.onComplete = onComplete;
            JsonLoader.LoadJson("Info").Completed += handle => OnDataLoaded(handle, r => audioInfo = JsonParser.ParseBeatmapInfo(r));
            JsonLoader.LoadJson("beatmapData").Completed += handle => OnDataLoaded(handle, r => beatmapData = JsonParser.ParseBeatmapData(r));
        }

        private void OnDataLoaded(AsyncOperationHandle<string> handle, Action<string> parseAction)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("Failed to load JSON: " + handle.OperationException);
                return;
            }
            parseAction(handle.Result);
            CheckAllFilesLoaded();
        }
        
        private void CheckAllFilesLoaded()
        {
            filesLoaded++;
            if (filesLoaded == 2) // Adjust this if more files are added
            {
                var combinedData = new AllBeatmapData
                {
                    AudioInfo = audioInfo,
                    BeatmapData = beatmapData
                };
                onComplete?.Invoke(combinedData);
            }
        }
    }
}