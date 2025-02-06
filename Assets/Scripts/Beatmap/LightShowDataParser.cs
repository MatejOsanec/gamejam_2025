using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beatmap
{
    namespace Lightshow
{
    [Serializable]
    public class LightshowEvent
    {
        public int i; // Maps to floatFxEvents array
    }
    
    [Serializable]
    public class EventBox
    {
        public List<LightshowEvent> l; // List of events within the box
    }

    [Serializable]
    public class EventBoxGroup
    {
        public float b; // Beat (Timing of the group)
        public int g; // Group ID
        public int t; // Translation
        public List<EventBox> e; // List of event boxes
    }
    
    [Serializable]
    public class FloatFxEvents
    {
        public int e; // event id
        public float v; // value
    }
    
    public class BeatmapEventData : IBeatmapObject
    {
        public int EventId;
        public float Value;
        public float Beat { get; }

        
        private float beat;

        public BeatmapEventData(float beat, int eventId, float value)
        {
            this.beat = beat;
            EventId = eventId;
            Value = value;
        }
    }

    [Serializable]
    public class LightshowRootObject
    {
        public List<EventBoxGroup> eventBoxGroups; // Main list of event box groups
        public List<FloatFxEvents> floatFxEvents; // Main list of event box groups
    }

    public class LightshowData
    {
        public List<BeatmapEventData> Events;

        public LightshowData(List<BeatmapEventData> events)
        {
            Events = events;
        }
    }

    public class LightshowJsonParser
    {
        public static LightshowData ParseLightshowData(string jsonString)
        {
            LightshowRootObject data = JsonUtility.FromJson<LightshowRootObject>(jsonString);
            
            List<BeatmapEventData> events = new();


            // Parse event box groups
            foreach (var group in data.eventBoxGroups)
            {
                // Parse event boxes inside the group
                foreach (var box in group.e)
                {
                    // Parse events inside the event box
                    foreach (var evt in box.l)
                    {
                        events.Add(new BeatmapEventData(group.b, group.g, data.floatFxEvents[evt.i].v));
                    }
                }
            }

            return new LightshowData(events);;
        }
    }
}

}