using Beatmap.Lightshow;
using Core;
using UnityEngine;

namespace Gameplay
{

    public class BeatmapEventListener : BeatmapCallbackListener
    {
        protected override void OnGameInit()
        {
            Locator.Callbacks.EventTriggeredSignal.AddListener(EventTriggeredHandler);    
        }

        private void EventTriggeredHandler(BeatmapEventData beatmapEventData)
        {   
            Debug.Log(beatmapEventData);
        }
    }
}