using Beatmap;

namespace Core
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class BeatTracker
    {
        public List<ColorNote> ColorNotes { get; private set; }
        private int lastIndex = 0;
        private float lastBeat = 0f;
        
        public event Action<ColorNote> OnColorNotePassed;
        
        public BeatTracker(List<ColorNote> colorNotes)
        {
            ColorNotes = colorNotes;
        }
        
        public void Update(float newBeat)
        {
            if (newBeat < lastBeat)
            {
                Debug.LogWarning("New beat is less than the last beat. Ignoring update.");
                return;
            }
            lastBeat = newBeat;
            
            while (lastIndex < ColorNotes.Count && ColorNotes[lastIndex].beat <= newBeat)
            {
                OnColorNotePassed?.Invoke(ColorNotes[lastIndex]);
                lastIndex++;
            }
        }
    }
}