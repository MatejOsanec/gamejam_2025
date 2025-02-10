using Core;
using UnityEngine;

namespace Gameplay
{
    [CreateAssetMenu(fileName = "PerlinNoiseModDefSo", menuName = "ScriptableObjects/PerlinNoiseModDefSo", order = 1)]
    public class PerlinNoiseModDefSo : ModulationDefSo
    {
        [Range(0, 1)] public float paramY = 1;
        
        public override float GetProgress()
        {
            return Mathf.PerlinNoise(Locator.BeatModel.GetBeatProgress(SyncMultiplier, Offset), paramY) * 2 - 1;
        }

        public override float GetProgressWithCustomOffset(float overrideOffset)
        {
            return Mathf.PerlinNoise(Locator.BeatModel.GetBeatProgress(SyncMultiplier, Offset), paramY) * 2 - 1;
        }
    }
}