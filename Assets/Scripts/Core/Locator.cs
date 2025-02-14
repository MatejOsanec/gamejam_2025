using System;
using Beatmap;
using Beatmap.Lightshow;
using strange.extensions.signal.impl;

namespace Core
{
    public static class Locator
    {
        public static bool Initialized { get; set; }
        public static WaveModel WaveModel { get; set; }

        public static Settings Settings;
        public static BeatModel BeatModel;
        public static Callbacks Callbacks = new();
        public static BeatmapObjectTracker<ColorNote> NoteTracker;
        public static BeatmapEventTracker EventTracker;
        public static NoteControllerCollection NoteControllerCollection;
        public static PrefabSpawner PrefabSpawner;
        public static BeatmapDataModel Model;
        public static GameStateManager GameStateManager;
    }

    public class Settings
    {
        public float NoteSpeed { get; private set; }
        public float PlacementMultiplier { get; private set; }
        public float PreSpawnBeats { get; private set; }
        
        public Settings(float noteSpeed, float placementMultiplier, float preSpawnBeats)
        {
            NoteSpeed = noteSpeed;
            PlacementMultiplier = placementMultiplier;
            PreSpawnBeats = preSpawnBeats;
        }
    }

    public class Callbacks
    {
        public readonly Signal GameplayInitSignal = new Signal();
        public Signal<ColorNote> NoteSpawnedSignal => Locator.NoteTracker.ObjectPassedSignal;
        public Signal<ColorNote> NoteMissSignal => Locator.NoteControllerCollection.NoteMissSignal;
        public Signal<BeatmapEventData> EventTriggeredSignal => Locator.EventTracker.ObjectPassedSignal;
        
        public void AddBeatListener(BeatDivision division, Action<int> callback) => Locator.BeatModel.AddBeatListener((int)division, callback);
        public void AddBeatListener(int division, Action<int> callback) => Locator.BeatModel.AddBeatListener(division, callback);

        public void RemoveBeatListener(BeatDivision division, Action<int> callback) => Locator.BeatModel.RemoveBeatListener((int)division, callback);
        public void RemoveBeatListener(int division, Action<int> callback) => Locator.BeatModel.RemoveBeatListener(division, callback);
        
        public void AddEventListener(int eventId, Action<float> callback) => Locator.EventTracker.AddEventListener(eventId, callback);

        public void RemoveAllListeners()
        {
            Locator.EventTracker.ObjectPassedSignal.RemoveAllListeners();    
            Locator.NoteTracker.ObjectPassedSignal.RemoveAllListeners(); 
            Locator.NoteControllerCollection.NoteMissSignal.RemoveAllListeners();
            Locator.BeatModel.RemoveAllListeners();
        }

    }
}