using Core;
using UnityEngine;

namespace Gameplay
{
    public abstract class BeatmapCallbackListener : MonoBehaviour
    {
        private bool _initialized = false;

        void Start()
        {
            Locator.Callbacks.GameplayInitSignal.AddListener(OnInit);
        }

        private void OnInit()
        {
            _initialized = true;
            OnGameInit();
        }

        protected abstract void OnGameInit();
    }
}