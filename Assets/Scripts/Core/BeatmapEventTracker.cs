using System;
using Beatmap;
using Beatmap.Lightshow;
using strange.extensions.signal.impl;

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
            spawnSignals[eventId].AddListener(callback);    
        }

        private void HandleEventPassed(BeatmapEventData eventData)
        {
            spawnSignals[eventData.EventId].Dispatch(eventData.Value);    
        }
    }
}