using System;
using Core;
using UnityEngine;

namespace Gameplay
{

    public class OnBeatActivator : BeatmapCallbackListener
    {
        public float targetBeat = 120;
        public Transform targetGameObject;

        protected override void OnGameInit()
        {
            Locator.Callbacks.AddBeatListener(1, OnBeatHandler);
            targetGameObject.gameObject.SetActive(false);
        }

        private void OnBeatHandler(int beat)
        {
            if (beat > targetBeat)
            {
                targetGameObject.gameObject.SetActive(true);
                Locator.Callbacks.RemoveBeatListener(1, OnBeatHandler);
            }
        }
    }
}