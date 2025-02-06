using Beatmap;
using strange.extensions.signal.impl;

namespace Core
{
    using System.Collections.Generic;
    using UnityEngine;
    
    public class NoteControllerCollection
    {
        public Signal<ColorNote> NoteMissSignal = new();
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
        
        public void UpdateNotes(float currentBeat)
        {
            for (int i = noteControllers.Count - 1; i >= 0; i--)
            {
                var noteController = noteControllers[i];
                noteController.UpdatePosition(currentBeat);
    
                if (noteController.ZPosition <= 0)
                {
                    NoteMissSignal.Dispatch(noteController.noteData);
                    var toRemove = noteController.gameObject;
                    noteControllers.RemoveAt(i);
                    Object.Destroy(toRemove);
                }
            }
        }
    }
}