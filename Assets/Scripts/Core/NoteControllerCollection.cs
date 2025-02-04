namespace Core
{
    using System.Collections.Generic;
    using UnityEngine;
    
    public class NoteControllerCollection
    {
        private List<NoteController> noteControllers = new List<NoteController>();
        
        public void Add(NoteController noteController)
        {
            if (noteController == null)
            {
                Debug.LogError("Cannot add a null NoteController.");
                return;
            }
            noteControllers.Add(noteController);
        }
        
        public void Remove(NoteController noteController)
        {
            if (noteController == null)
            {
                Debug.LogError("Cannot remove a null NoteController.");
                return;
            }
            noteControllers.Remove(noteController);
        }
        
        public void UpdatePosition(float currentBeat)
        {
            foreach (var noteController in noteControllers)
            {
                noteController.UpdatePosition(currentBeat);
            }
        }
    }
}