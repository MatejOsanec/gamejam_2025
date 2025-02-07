using Beatmap;
using Beatmap.Lightshow;
using Core;
using Gameplay;
using UnityEngine;

public class Examples
{
    public Examples()
    {
        Locator.Callbacks.AddBeatListener(BeatDivision.Quarter, BeatHandler); // triggers BeatHandler every Quarter note, passes beatcount as int param
        
        Locator.Callbacks.NoteSpawnedSignal.AddListener(NoteSpawnedHandler); // triggers note spawned (currently 4 beats ahead its time in beatmap)
        Locator.Callbacks.NoteMissSignal.AddListener(NoteMissHandler); // triggers note missed (exactly on time in beatmap)
        
        Locator.Callbacks.EventTriggeredSignal.AddListener(EventTriggeredHandler); // triggers when any event is passes, passes whole BeatmapEventData as param
        Locator.Callbacks.AddEventListener(19, SpecificEventHandler); // triggers when event with specific id passes, passes the 0-1 float value of event directly

        var beatMultiplier = 4;
        Locator.BeatModel.GetBeatProgress(beatMultiplier); // returns 0 - float of any beat multiple / division based on multiplier you provide
        Locator.BeatModel.GetShapedBeatProgress(Waveform.Square, beatMultiplier); // returns 0 - float shaped as basic wavefroms - sinus / quare
        Locator.BeatModel.GetCurvedBeatProgress(new AnimationCurve(), beatMultiplier); // returns whatever as it syncs any animation curve to beat
    }

    private void SpecificEventHandler(float v)
    {
        
    }

    private void EventTriggeredHandler(BeatmapEventData e)
    {
        
    }

    private void NoteMissHandler(ColorNote n)
    {
        
    }

    private void NoteSpawnedHandler(ColorNote n)
    {
        
    }

    private void BeatHandler(int b)
    {
        
    }
}

    
