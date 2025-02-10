using System;
using System.Linq;
using Core;
using UnityEngine;

namespace Gameplay
{
    [Serializable]
    public class ModulationSequenceItem
    {
        public float startBeat;
        public ModulationDefSo ModulationDef;
    }

    [CreateAssetMenu(fileName = "ModulationSequenceDefSo", menuName = "ScriptableObjects/ModulationSequenceDefSo",
        order = 1)]
    public class ModulationSequenceDefSo : ModulationDefSo
    {
        [HideInInspector] public new AnimationCurve movementCurve { get; private set; }
        [HideInInspector] public new float syncMultiplier { get; private set; }
        [HideInInspector] public new float offset { get; private set; }

        public ModulationSequenceItem[] modItems;
        public bool sorted = false;

        public override float GetProgress()
        {
            if (Locator.BeatModel == null)
            {
                return 0;
            }

            TrySortSequences();

            var modItem = FindCurrentModulation(Locator.BeatModel.CurrentBeat);
            return modItem != null ? modItem.ModulationDef.GetProgress() : 0;
        }

        public override float GetProgressWithCustomOffset(float overrideOffset)
        {
            if (Locator.BeatModel == null)
            {
                return 0;
            }
            
            TrySortSequences();

            var modItem = FindCurrentModulation(Locator.BeatModel.CurrentBeat);
            if (modItem == null)
            {
                return 0;
            }
            return Locator.BeatModel.GetCurvedBeatProgressWithOffset(modItem.ModulationDef, overrideOffset);
        }

        private void TrySortSequences()
        {
            if (!sorted)
            {
                modItems.OrderBy(m => m.startBeat);
                sorted = true;
            }
        }

        private ModulationSequenceItem FindCurrentModulation(float beat)
        {
            return modItems.LastOrDefault(x => x.startBeat < beat);
        }
    }
}