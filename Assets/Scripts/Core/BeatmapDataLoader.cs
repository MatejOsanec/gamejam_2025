using Beatmap;
using Beatmap.Lightshow;
using BeatmapEditor3D.DataModels;

namespace Core
{
    using System;
    using UnityEngine;
    using UnityEngine.ResourceManagement.AsyncOperations;
    public class BeatmapDataModel
    {
        public AudioInfo AudioInfo { get; set; }
        public BeatmapData BeatmapData { get; set; }
        public LightshowData LightshowData { get; set; }
        public BpmData BpmData { get; set; }
    }
    
    public class BeatmapDataLoader
    {
        private Action<BeatmapDataModel> onComplete;
        private AudioInfo audioInfo;
        private BeatmapData beatmapData;
        private BpmData bpmData;
        private LightshowData lightshowData;
        private int filesLoaded = 0;
        private int filesToLoad = 0;
        
        public void LoadBeatmapData(Action<BeatmapDataModel> onComplete)
        {
            this.onComplete = onComplete;
            LoadJson("Info", r => audioInfo = BeatmapdataParser.ParseBeatmapInfo(r));
            LoadJson("beatmapData", r => beatmapData = BeatmapdataParser.ParseBeatmapData(r));
            LoadJson("AudioData", r => bpmData = BeatmapdataParser.ParseAudioData(r));
            LoadJson("lightshow", r => lightshowData = LightshowJsonParser.ParseLightshowData(r));
        }

        private void CheckAllFilesLoaded()
        {
            if (audioInfo != null && beatmapData != null && lightshowData != null && bpmData != null)
            {
                var combinedData = new BeatmapDataModel
                {
                    AudioInfo = audioInfo,
                    BeatmapData = beatmapData,
                    LightshowData = lightshowData,
                    BpmData = bpmData
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