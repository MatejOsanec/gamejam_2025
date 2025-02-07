using Beatmap;
using Beatmap.Lightshow;
using Core;

public class Examples
{
    public Examples()
    {
        Locator.Callbacks.AddBeatListener(BeatDivision.Quarter, BeatHandler); // triggers BeatHandler every Quarter note, passes beatcount as int param
        
        Locator.Callbacks.NoteSpawnedSignal.AddListener(NoteSpawnedHandler); // triggers note spawned (currently 4 beats ahead its time in beatmap)
        Locator.Callbacks.NoteMissSignal.AddListener(NoteMissHandler); // triggers note missed (exactly on time in beatmap)
        
        Locator.Callbacks.EventTriggeredSignal.AddListener(EventTriggeredHandler); // triggers when any event is passes, passes whole BeatmapEventData as param
        Locator.Callbacks.AddEventListener(19, SpecificEventHandler); // triggers when event with specific id passes, passes the 0-1 float value of event directly

        var beatMultiplier = 3;
        Locator.BeatModel.GetBeatProgress(beatMultiplier); // returns 0 - float of any beat multiple / division based on multiplier you provide, very easy to turn into sinus or other eased movement, or use animation curve
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

    
