using Core;
using UnityEngine;

namespace Gameplay
{
    public abstract class BeatmapCallbackListener : MonoBehaviour
    {
        protected bool _initialized = false;

        void Start()
        {
            if (Locator.Model.Initialized)
            {
                _initialized = true;
                OnGameInit();    
            }
            else
            {
                Locator.Callbacks.GameplayInitSignal.AddListener(OnInit);
            }
        }

        private void OnInit()
        {
            Locator.Callbacks.GameplayInitSignal.RemoveListener(OnInit);
            _initialized = true;
            OnGameInit();
        }

        protected abstract void OnGameInit();
    }
}