using Beatmap;

namespace Core
{
    using UnityEngine;

    public class NoteController : MonoBehaviour
    {
        public ColorNote noteData;
        private float _speed;

        public float ZPosition => transform.position.z;

        public void Setup(ColorNote noteData, float speed)
        {
            this.noteData = noteData;
            this._speed = speed;
        }

        public void UpdatePosition(float currentBeat)
        {
            if (noteData == null)
            {
                Debug.LogError("NoteData is not set. Please call SetupMethod first.");
                return;
            }

            // Calculate the new Y position
            float newZ = (noteData.beat - currentBeat) * _speed;

            // Update the position of the note
            var noteTransform = transform;
            var position = noteTransform.position;
            position = new Vector3(position.x, position.y, newZ);
            noteTransform.position = position;
        }
    }
}