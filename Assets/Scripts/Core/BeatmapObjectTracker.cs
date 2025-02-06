using Beatmap;
using strange.extensions.signal.impl;

namespace Core
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class BeatmapObjectTracker<T> where T : IBeatmapObject
    {
        public readonly Signal<T> ObjectPassedSignal = new Signal<T>();

        private List<T> ColorNotes;
        private int _lastIndex = 0;
        private float _lastBeat = 0f;
        
        public event Action<ColorNote> OnColorNotePassed;
        
        public BeatmapObjectTracker(List<T> colorNotes)
        {
            ColorNotes = colorNotes;
        }
        
        public void Update(float newBeat)
        {
            if (newBeat < _lastBeat)
            {
                Debug.LogWarning("New beat is less than the last beat. Ignoring update.");
                return;
            }
            _lastBeat = newBeat;
            
            while (_lastIndex < ColorNotes.Count && ColorNotes[_lastIndex].Beat <= newBeat)
            {
                ObjectPassedSignal.Dispatch(ColorNotes[_lastIndex]);
                _lastIndex++;
            }
        }
    }
}