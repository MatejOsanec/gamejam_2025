using Beatmap;

namespace Core
{
    using UnityEngine;

    public class PrefabSpawner
    {
        private readonly Transform _parentTransform;

        public PrefabSpawner(Transform parentTransform)
        {
            _parentTransform = parentTransform;
        }

        public NoteController SpawnNote(GameObject prefab, ColorNote noteData)
        {
            var go = InstantiatePrefab(prefab, _parentTransform, new Vector3(noteData.x - 1, 0, noteData.y - 1) * Locator.Settings.PlacementMultiplier);
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
            instance.transform.position = position;

            return instance;
        }
    }
}