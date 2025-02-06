using Beatmap;
using Beatmap.Lightshow;

namespace Core
{
    using System;
    using UnityEngine;
    using UnityEngine.ResourceManagement.AsyncOperations;
    public class AllBeatmapData
    {
        public AudioInfo AudioInfo { get; set; }
        public BeatmapData BeatmapData { get; set; }
        public LightshowData LightshowData { get; set; }
    }
    
    public class BeatmapDataLoader
    {
        private Action<AllBeatmapData> onComplete;
        private AudioInfo audioInfo;
        private BeatmapData beatmapData;
        private LightshowData lightshowData;
        private int filesLoaded = 0;
        private int filesToLoad = 0;
        
        public void LoadBeatmapData(Action<AllBeatmapData> onComplete)
        {
            this.onComplete = onComplete;
            LoadJson("Info", r => audioInfo = BeatmapdataParser.ParseBeatmapInfo(r));
            LoadJson("beatmapData", r => beatmapData = BeatmapdataParser.ParseBeatmapData(r));
            LoadJson("lightshow", r => lightshowData = LightshowJsonParser.ParseLightshowData(r));
        }

        private void CheckAllFilesLoaded()
        {
            if (audioInfo != null && beatmapData != null && lightshowData != null)
            {
                var combinedData = new AllBeatmapData
                {
                    AudioInfo = audioInfo,
                    BeatmapData = beatmapData,
                    LightshowData = lightshowData
                };
                onComplete?.Invoke(combinedData);
            }
        }
        
        private void LoadJson(string fileName, Action<string> onJsonReady)
        {
            JsonLoader.LoadJson(fileName).Completed += handle =>
            {
                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError("Failed to load JSON: " + handle.OperationException);
                    return;
                }

                onJsonReady(handle.Result);
                CheckAllFilesLoaded();
            };
        }
    }
}