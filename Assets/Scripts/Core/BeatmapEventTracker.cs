using System;
using Beatmap.Lightshow;
using strange.extensions.signal.impl;
using UnityEngine;

namespace Core
{
    using System.Collections.Generic;
    
    public class BeatmapEventTracker : BeatmapObjectTracker<BeatmapEventData>
    {
        private Dictionary<int, Signal<float>> spawnSignals = new();
            
        public BeatmapEventTracker(List<BeatmapEventData> eventData) : base(eventData)
        {   
            foreach (var beatmapEventData in eventData)
            {
                if (!spawnSignals.ContainsKey(beatmapEventData.EventId))
                {
                    spawnSignals[beatmapEventData.EventId] = new Signal<float>();
                }
            }
            
            ObjectPassedSignal.AddListener(HandleEventPassed);
        }

        public void AddEventListener(int eventId, Action<float> callback)
        {
            if (spawnSignals.ContainsKey(eventId))
            {
                spawnSignals[eventId].AddListener(callback);
            }
            else
            {
                // Log an error with the list of available keys
                string availableKeys = string.Join(", ", spawnSignals.Keys);
                Debug.LogError($"Event ID {eventId} not found. Available event IDs: {availableKeys}");
            }
        }

        private void HandleEventPassed(BeatmapEventData eventData)
        {
            spawnSignals[eventData.EventId].Dispatch(eventData.Value);    
        }
    }
}