using Beatmap;

namespace Core
{
    using UnityEngine;

    public class PrefabSpawner
    {
        private readonly Transform _parentTransform;
        private readonly Material[] _starfishMaterials;
        private readonly float[] spawnHeights = { 0f, 1.1f, 1.6f };

        public PrefabSpawner(Transform parentTransform, Material[] starfishMaterials)
        {
            _parentTransform = parentTransform;
            _starfishMaterials = starfishMaterials;
        }

        public NoteController SpawnBaddie(GameObject prefab, ColorNote noteData)
        {
            Debug.Log($"Spawning baddie at position: ({noteData.x - 1}, {noteData.y})");

            var go = InstantiatePrefab(prefab, _parentTransform, new Vector3((noteData.x - 1) * 0.5f, spawnHeights[noteData.y], 0));
            var noteController = go.AddComponent<NoteController>();
            noteController.Setup(noteData, Locator.Settings.NoteSpeed);

            var ctrl = go.GetComponent<BaddieController>();
            if (ctrl != null)
            {
                ctrl.SetMaterial(GetRandomMaterial());
            }
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
        
        Material GetRandomMaterial()
        {
            int randomIndex = Random.Range(0, _starfishMaterials.Length);
            return _starfishMaterials[randomIndex];
        }
    }
}