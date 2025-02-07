using Beatmap;

namespace Core
{
    using UnityEngine;

    public class PrefabSpawner
    {
        private readonly Transform _parentTransform;
        private float[] spawnHeights = { 0f, 1.1f, 1.6f };

        public PrefabSpawner(Transform parentTransform)
        {
            _parentTransform = parentTransform;
        }

        public NoteController SpawnNote(GameObject prefab, ColorNote noteData)
        {
            Debug.Log($"Spawning note at position: ({noteData.x - 1}, {noteData.y})");
            // Use noteData.x and noteData.y independently for the position
            var go = InstantiatePrefab(prefab, _parentTransform, new Vector3((noteData.x - 1) * 0.5f, spawnHeights[noteData.y], 0));
            var noteController = go.AddComponent<NoteController>();
            noteController.Setup(noteData, Locator.Settings.NoteSpeed);
            return noteController;
        }
        
        public GameObject InstantiatePrefab(GameObject prefab, Transform parentTransform, Vector3 position)
        {
            if (prefab == null || parentTransform == null)
            {
                Debug.LogError("Prefab or parent transform is not provided.");
                return null;
            }

            GameObject instance = Object.Instantiate(prefab, parentTransform, false);
            instance.transform.localPosition = position;

            return instance;
        }
    }
}