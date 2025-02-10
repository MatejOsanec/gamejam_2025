using Core;
using UnityEngine;

namespace Gameplay
{
    [CreateAssetMenu(fileName = "ModulationDefSo", menuName = "ScriptableObjects/ModulationDefSo", order = 1)]
    public class ModulationDefSo : ScriptableObject
    {
        public AnimationCurve MovementCurve => movementCurve;
        public float Offset => offset;
        public float SyncMultiplier => syncMultiplier;
        
        [SerializeField] private AnimationCurve movementCurve;
        [SerializeField] private float syncMultiplier = 1f;
        [SerializeField] [Range(0,1)] private  float offset = 0f;

        public virtual float GetProgress()
        {
            if (Locator.BeatModel == null)
            {
                return 0;
            }
            return Locator.BeatModel.GetCurvedBeatProgress(this);
        }
        
        public virtual float GetProgressWithCustomOffset(float overrideOffset)
        {
            if (Locator.BeatModel == null)
            {
                return 0;
            }
            return Locator.BeatModel.GetCurvedBeatProgressWithOffset(this, overrideOffset);
        }
    }
}