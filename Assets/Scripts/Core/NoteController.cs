using Beatmap;

namespace Core
{
    using UnityEngine;

    public class NoteController : MonoBehaviour
    {
        private ColorNote noteData;
        private float speed;

        public float ZPosition => transform.position.z;

        public void Setup(ColorNote noteData, float speed)
        {
            this.noteData = noteData;
            this.speed = speed;
        }

        public void UpdatePosition(float currentBeat)
        {
            if (noteData == null)
            {
                Debug.LogError("NoteData is not set. Please call SetupMethod first.");
                return;
            }

            // Calculate the new Y position
            float newZ = (noteData.beat - currentBeat) * speed;

            // Update the position of the note
            var noteTransform = transform;
            var position = noteTransform.position;
            position = new Vector3(position.x, position.x, newZ);
            noteTransform.position = position;
        }
    }
}