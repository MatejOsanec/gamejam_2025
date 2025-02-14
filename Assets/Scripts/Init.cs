using System.Collections;
using Beatmap;
using Core;
using UnityEngine;

public class Init : MonoBehaviour
{
    public bool spawnTestMode = false;
    public Transform gameplayTransform;
    public AudioController audioController;
    public Material[] starfishMaterials;
    public Transform[] initSceneGameobjects;
    public Transform[] gameSceneGameobjects;

    // ======== SETTINGS ========
    [Header("DEBUG STUFF")]
    public float START_BEAT = 0;
    public float CURRENT_BEAT = 0;

    [Header("SETINGS")]
    public GameObject[] floorBaddiePrefabs;
    public GameObject[] midBaddiePrefabs;
    public float noteSpeed = 1;
    public float placementMultiplier = 1;
    public float preSpawnBeats = 4;

    // ======== SETTINGS ========

    public bool _initialized;

    void Start()
    {
        Locator.GameStateManager = new GameStateManager(initSceneGameobjects, gameSceneGameobjects, audioController, START_BEAT);
        Locator.Settings = new Settings(noteSpeed, placementMultiplier, preSpawnBeats);
        Locator.WaveModel = new WaveModel();
        Locator.BeatModel = new BeatModel();
        Locator.PrefabSpawner = new PrefabSpawner(gameplayTransform, starfishMaterials);
        Locator.NoteControllerCollection = new NoteControllerCollection();

        Locator.Callbacks.AddBeatListener(BeatDivision.Quarter, BeatListener);

        Debug.Log("sdfsdf");

        var loader = new BeatmapDataLoader();
        loader.LoadBeatmapData(OnBeatmapLoaded);
    }

    private void BeatListener(int beat)
    {
        if (!spawnTestMode || beat % 4 != 0)
        {
            return;
        }
        
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                OnColorNotePassed(new ColorNote { x = x, y = y, beat = Locator.BeatModel.CurrentBeat + 8 });
            }    
        }
    }

    private void OnBeatmapLoaded(BeatmapDataModel model)
    {
        
        Locator.Model = model;
        Locator.NoteTracker = new BeatmapObjectTracker<ColorNote>(model.BeatmapData.colorNotes, Locator.Settings.PreSpawnBeats);
        Locator.EventTracker = new BeatmapEventTracker(model.LightshowData.Events);
        
        Locator.Callbacks.NoteSpawnedSignal.AddListener(OnColorNotePassed);
        Locator.Callbacks.NoteMissSignal.AddListener(OnColorNoteMiss);
        Locator.Initialized = true;
        Locator.Callbacks.GameplayInitSignal.Dispatch();

        _initialized = true;

        Locator.GameStateManager.SetState(GameState.Init);
        StartCoroutine(ChangeStateWithDelay(5, GameState.Game));
    }
    
    private IEnumerator ChangeStateWithDelay(float delay, GameState state)
    {
        yield return new WaitForSeconds(delay);
        // Set the next state
        Locator.GameStateManager.SetState(state);
    }

    private void OnColorNotePassed(ColorNote note)
    {
        Debug.Log($"NOTE SPAWNED: {note.beat}, X: {note.x}, Y: {note.y}, Direction: {note.d}");

        var isFloor = note.y == 0;


        var noteController = Locator.PrefabSpawner.SpawnBaddie(isFloor ? floorBaddiePrefabs[Locator.WaveModel.CurrentWaveId] : midBaddiePrefabs[Locator.WaveModel.CurrentWaveId], note);
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

        var newBeat = Locator.Model.BpmData.SampleToBeat(audioController.Samples);
        Locator.BeatModel.UpdateBeat(newBeat);

        if (!spawnTestMode)
        {
            Locator.NoteTracker.Update(newBeat);
        }

        Locator.WaveModel.Update();
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
}