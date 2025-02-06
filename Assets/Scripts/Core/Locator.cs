using System;
using System.Collections.Generic;
using strange.extensions.signal.impl;
using UnityEngine;

namespace Core
{
    public static class Locator
    {
        public static Settings Settings;
        public static BeatModel BeatModel;
    }

    public class Settings
    {
        public float NoteSpeed { get; private set; }
        public float PlacementMultiplier { get; private set; }
        public float PreSpawnBeats { get; private set; }
        
        public Settings(float noteSpeed, float placementMultiplier, float preSpawnBeats)
        {
            NoteSpeed = noteSpeed;
            PlacementMultiplier = placementMultiplier;
            PreSpawnBeats = preSpawnBeats;
        }
    }
    
    public class BeatModel
    {
        public float CurrentBeat = 0;
        public readonly Signal<int, int> AnyBeatSignal = new();
        
        private readonly Dictionary<int, Signal<int>> _onBeatSignals = new();
        private readonly Dictionary<int, int> _lastProcessedBeats = new();

        private readonly List<int> _beatDivisions;

        public BeatModel(List<int> divisions)
        {
            _beatDivisions = divisions;
            foreach (var division in _beatDivisions)
            {
                _lastProcessedBeats[division] = -1;
                _onBeatSignals[division] = new Signal<int>();
            }    
        }

        public void AddBeatListener(int division, Action<int> callback)
        {
            _onBeatSignals[division].AddListener(callback);    
        }

        public void UpdateBeat(float beat)
        {
            CurrentBeat = beat;
            
            foreach (var division in _beatDivisions)
            {
                int currentDividedBeat = Mathf.FloorToInt(CurrentBeat * division);
                
                if (currentDividedBeat != _lastProcessedBeats[division])
                {
                    AnyBeatSignal.Dispatch(currentDividedBeat, division);
                    _onBeatSignals[division].Dispatch(currentDividedBeat);
                    _lastProcessedBeats[division] = currentDividedBeat;
                }
            }
        }
    }
}