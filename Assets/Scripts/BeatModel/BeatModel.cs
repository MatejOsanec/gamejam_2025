using System;
using System.Collections.Generic;
using System.Linq;
using strange.extensions.signal.impl;
using UnityEngine;

namespace Core
{
    public enum BeatDivision
    {
        Whole = 1,
        Half = 2,
        Quarter = 4,
        Eighth = 8,
        Sixteenth = 16,
        ThirtySecond = 32,
        SixtyFourth = 64,
        OneTwentyEighth = 128,
        TripletHalf = 3,
        TripletQuarter = 6,
        TripletEighth = 12,
        TripletSixteenth = 24,
        TripletThirtySecond = 48,
        TripletSixtyFourth = 96,
        DottedHalf = 3,
        DottedQuarter = 6,
        DottedEighth = 12,
        DottedSixteenth = 24,
        DottedThirtySecond = 48,
        DottedSixtyFourth = 96
    }
    
    public static class BeatDivisionExtensions
    {
        public static List<int> ToIntList(this IEnumerable<BeatDivision> divisions)
        {
            return divisions.Select(d => (int)d).ToList();
        }
    }
    
    public class BeatModel
    {
        public float CurrentBeat = 0;
        public readonly Signal<int, int> AnyBeatSignal = new();
        
        private readonly Dictionary<int, Signal<int>> _onBeatSignals = new();
        private readonly Dictionary<int, int> _lastProcessedBeats = new();

        private void InitializeDivision(int division)
        {
            if (!_onBeatSignals.ContainsKey(division))
            {
                _lastProcessedBeats[division] = -1;
                _onBeatSignals[division] = new Signal<int>();
            }
        }
        public void AddBeatListener(int division, Action<int> callback)
        {
            InitializeDivision(division);
            _onBeatSignals[division].AddListener(callback);
        }
        
        public void UpdateBeat(float beat)
        {
            CurrentBeat = beat;
            
            foreach (var division in _onBeatSignals.Keys)
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