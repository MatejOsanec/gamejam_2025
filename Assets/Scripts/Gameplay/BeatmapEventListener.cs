using Core;
using UnityEngine;

namespace Gameplay
{

    public class BeatmapEventListener : BeatmapCallbackListener
    {
        protected override void OnGameInit()
        {
            Locator.Callbacks.AddEventListener(20, EventTriggeredHandler);    
        }

        private void EventTriggeredHandler(float value)
        {   
            Debug.Log($"EVENT ID 20 value:{value}");
        }
    }
}