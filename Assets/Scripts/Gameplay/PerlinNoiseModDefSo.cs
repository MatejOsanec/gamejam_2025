using System;
using System.Linq;
using Core;
using UnityEngine;

namespace Gameplay
{
    [CreateAssetMenu(fileName = "PerlinNoiseModDefSo", menuName = "ScriptableObjects/PerlinNoiseModDefSo", order = 1)]
    public class PerlinNoiseModDefSo : ModulationDefSo
    {
        // just hiding original params
        public new AnimationCurve movementCurve { get; private set; }
        public new float syncMultiplier { get; private set; }
        public new float offset { get; private set; }
        // -----

        [Range(0, 1)] public float paramY = 1;
        
        public override float GetProgress()
        {
            return Mathf.PerlinNoise(Locator.BeatModel.GetBeatProgress(8f), paramY);
        }

        public override float GetProgressWithCustomOffset(float overrideOffset)
        {
            return Mathf.PerlinNoise(Locator.BeatModel.GetShapedBeatProgress(Waveform.Sinusoidal, 8f, overrideOffset), paramY) * 2 - 1;
        }
    }
}