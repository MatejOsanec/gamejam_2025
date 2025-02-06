using System;
using Beatmap;
using Beatmap.Lightshow;
using Core;
using UnityEngine;

public class Init : MonoBehaviour
{
    public AudioController audioController;
    public GameObject notePrefab;

    // ======== SETTINGS ========

    public float CURRENT_BEAT = 0;
    public float noteSpeed = 1;
    public float placementMultiplier = 5;
    public float preSpawnBeats = 4;

    // ======== SETTINGS ========

    private AllBeatmapData _beatmapData;
    public bool _initialized;

    void Start()
    {
        Locator.Settings = new Settings(noteSpeed, placementMultiplier, preSpawnBeats);
        Locator.BeatModel = new BeatModel();

        Locator.Callbacks.AddBeatListener(BeatDivision.Quarter, BeatListener);

        Debug.Log("sdfsdf");

        var loader = new BeatmapDataLoader();
        loader.LoadBeatmapData(OnBeatmapLoaded);
    }

    private void BeatListener(int beat)
    {
        Debug.Log($"BEAT: {beat}");
    }

    private void OnBeatmapLoaded(AllBeatmapData beatmapData)
    {
        _beatmapData = beatmapData;

        Locator.NoteTracker = new BeatmapObjectTracker<ColorNote>(_beatmapData.BeatmapData.colorNotes, Locator.Settings.PreSpawnBeats);
        Locator.EventTracker = new BeatmapEventTracker(_beatmapData.LightshowData.Events);
        Locator.PrefabSpawner = new PrefabSpawner(transform);
        Locator.NoteControllerCollection = new NoteControllerCollection();

        Locator.Callbacks.NoteSpawnedSignal.AddListener(OnColorNotePassed);
        Locator.Callbacks.NoteMissSignal.AddListener(OnColorNoteMiss);
        Locator.Callbacks.GameplayInitSignal.Dispatch();

        audioController.PlayAudio();

        _initialized = true;
    }

    private void OnColorNotePassed(ColorNote note)
    {
        Debug.Log($"NOTE SPAWNED: {note.beat}, X: {note.x}, Y: {note.y}, Direction: {note.d}");
        var noteController = Locator.PrefabSpawner.SpawnNote(notePrefab, note);
        Locator.NoteControllerCollection.Add(noteController);
    }

    private void OnColorNoteMiss(ColorNote note)
    {
        Debug.Log($"NOTE MISS: {note.beat}, X: {note.x}, Y: {note.y}, Direction: {note.d}");
    }

    private void Update()
    {
        if (!_initialized)
        {
            return;
        }

        var newBeat = AudioUtils.SamplesToBeats(audioController.Samples, audioController.SampleRate, _beatmapData.AudioInfo.bpm);
        Locator.BeatModel.UpdateBeat(newBeat);

        Locator.NoteTracker.Update(newBeat);
        Locator.NoteControllerCollection.UpdateNotes(newBeat);

        Locator.EventTracker.Update(newBeat);

        // temporary fast iteration shit
        Locator.Settings = new Settings(noteSpeed, placementMultiplier, preSpawnBeats);
        CURRENT_BEAT = Locator.BeatModel.CurrentBeat;
    }

    private void OnDestroy()
    {
        Locator.Callbacks.RemoveAllListeners();
    }

    private void LogAudioInfo(AudioInfo audioInfo)
    {
        Debug.Log("AUDIO INFO LOADED: " + audioInfo.bpm);
        Debug.Log("AUDIO INFO LOADED: " + audioInfo.audioDataFilename);
    }

    private void LogBeatmapData(BeatmapData beatmapData)
    {
        Debug.Log("======================================= Color Notes =======================================");
        foreach (var note in beatmapData.colorNotes)
        {
            Debug.Log($"Beat: {note.beat}, X: {note.x}, Y: {note.y}, Duration: {note.d}");
        }

        Debug.Log("======================================= Bomb Notes =======================================");
        foreach (var note in beatmapData.bombNotes)
        {
            Debug.Log($"Beat: {note.b}, X: {note.x}, Y: {note.y}");
        }

        Debug.Log("======================================= Obstacles =======================================");
        foreach (var obstacle in beatmapData.obstacles)
        {
            Debug.Log($"Beat: {obstacle.b}, X: {obstacle.x}, Y: {obstacle.y}, Depth: {obstacle.d}, Width: {obstacle.w}, Height: {obstacle.h}");
        }
    }
}
