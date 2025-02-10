using Core;
using UnityEngine;

namespace Gameplay
{
    [CreateAssetMenu(fileName = "ModulationDefSo", menuName = "ScriptableObjects/ModulationDefSo", order = 1)]
    public class ModulationDefSo : ScriptableObject
    {
        public AnimationCurve movementCurve;
        public float syncMultiplier = 1f;
        [Range(0,1)]
        public float offset = 0f;

        public float GetProgress()
        {
            if (Locator.BeatModel == null)
            {
                return 0;
            }
            return Locator.BeatModel.GetCurvedBeatProgress(movementCurve, syncMultiplier, offset);
        }
        
        public float GetProgressWithCustomOffset(float overrideOffset)
        {
            if (Locator.BeatModel == null)
            {
                return 0;
            }
            return Locator.BeatModel.GetCurvedBeatProgress(movementCurve, syncMultiplier, overrideOffset);
        }
    }
}