using System.Collections.Generic;
using Beatmap;
using Core;
using UnityEngine;

public class Init : MonoBehaviour
{
    public AudioController audioController;
    public GameObject notePrefab;
    
    // ======== SETTINGS ========
    
    public float noteSpeed = 1;
    public float placementMultiplier = 5;
    public float preSpawnBeats = 4;

    // ======== SETTINGS ========

    private AllBeatmapData _beatmapData;
    private BeatTracker _tracker;
    private NoteControllerCollection _notes;
    private PrefabSpawner _spawner;
    private bool _initialized;

    void Start()
    {
        Locator.Settings = new Settings(noteSpeed, placementMultiplier, preSpawnBeats);
        Locator.BeatModel = new BeatModel();
        
        Locator.BeatModel.AddBeatListener(BeatDivision.Quarter, BeatListener);

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
        LogBeatmapData(beatmapData.BeatmapData);
        _beatmapData = beatmapData;
        
        audioController.PlayAudio();

        _spawner = new PrefabSpawner(transform);
        _notes = new NoteControllerCollection();
        _tracker = new BeatTracker(_beatmapData.BeatmapData.colorNotes);
        _tracker.OnColorNotePassed += OnColorNotePassed;

        _initialized = true;
        Locator.GameplayInitSignal.Dispatch();
    }

    private void OnColorNotePassed(ColorNote note)
    {
        Debug.Log($"NOTE PASSED: {note.beat}, X: {note.x}, Y: {note.y}, Duration: {note.d}");
        var noteController = _spawner.SpawnNote(notePrefab, note);
        _notes.Add(noteController);
    }

    private void Update()
    {
        if (!_initialized)
        {
            return;
        }
        
        var newBeat = AudioUtils.SamplesToBeats(audioController.Samples, audioController.SampleRate, _beatmapData.AudioInfo.bpm);
        Locator.BeatModel.UpdateBeat(newBeat);
        
        _tracker.Update(newBeat + Locator.Settings.PreSpawnBeats);
        _notes.UpdatePosition(newBeat);

        // temporary fast iteration shit
        Locator.Settings = new Settings(noteSpeed, placementMultiplier, preSpawnBeats);
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