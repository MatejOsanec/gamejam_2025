using Gameplay;
using Unity.Mathematics;
using UnityEngine;

namespace Core
{
    public class BeatmapRangeEnabler : BeatmapCallbackListener
    {
        public GameObject targetGo;
        public float[] mins;
        public float[] maxes;
        
        protected override void OnGameInit()
        {
            Locator.Callbacks.AddBeatListener(1, BeatmapCallbackListener);
        }

        private void BeatmapCallbackListener(int beat)
        {
            var numValues = math.min(mins.Length, maxes.Length);
            var enable = false;
            
            for (int i = 0; i < numValues; i++)
            {
                if (beat > mins[i] && beat < maxes[i])
                {
                    enable = true;
                    continue;
                }    
            }
            
            targetGo.SetActive(enable);
        }
    }
}