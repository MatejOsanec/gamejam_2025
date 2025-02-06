using Beatmap;
using strange.extensions.signal.impl;

namespace Core
{
    using System.Collections.Generic;
    
    public class BeatmapObjectTracker<T> where T : IBeatmapObject
    {
        public readonly Signal<T> ObjectPassedSignal = new Signal<T>();
        
        private readonly List<T> _colorNotes;
        private int _lastIndex = 0;
        private float _lastBeat = 0f;
        private readonly float _spawnPredelay = 0f;

        public BeatmapObjectTracker(List<T> colorNotes, float spawnPredelay = 0)
        {
            _colorNotes = colorNotes;
            _spawnPredelay = spawnPredelay;
        }
        
        public void Update(float newBeat)
        { 
            while (_lastIndex < _colorNotes.Count && _colorNotes[_lastIndex].Beat <= newBeat + _spawnPredelay)
            {
                ObjectPassedSignal.Dispatch(_colorNotes[_lastIndex]);
                _lastIndex++;
            }
        }
    }
}