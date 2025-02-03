using BeatmapData;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Init : MonoBehaviour
{
    void Start()
    {
        JsonLoader.LoadBeatmap("beatmap").Completed += OnJsonLoaded;
    }
    
    private void OnJsonLoaded(AsyncOperationHandle<string> handle)
    {
        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError("Failed to load JSON: " + handle.OperationException);
            return;
        }
        
        Beatmap beatmap = JsonParser.ParseBeatmapData(handle.Result);

        Debug.Log("======================================= Color Notes =======================================");
        foreach (var note in beatmap.colorNotes)
        {
            Debug.Log($"Beat: {note.b}, X: {note.x}, Y: {note.y}, Duration: {note.d}");
        }

        Debug.Log("======================================= Bomb Notes =======================================");
        foreach (var note in beatmap.bombNotes)
        {
            Debug.Log($"Beat: {note.b}, X: {note.x}, Y: {note.y}");
        }

        Debug.Log("======================================= Obstacles =======================================");
        foreach (var obstacle in beatmap.obstacles)
        {
            Debug.Log($"Beat: {obstacle.b}, X: {obstacle.x}, Y: {obstacle.y}, Depth: {obstacle.d}, Width: {obstacle.w}, Height: {obstacle.h}");
        }
    }
}