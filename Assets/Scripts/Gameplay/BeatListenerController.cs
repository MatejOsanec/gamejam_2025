using Core;
using UnityEngine;

namespace Gameplay
{
    public class BeatListenerController : MonoBehaviour
    {
        private MeshRenderer meshRenderer;
        public BeatDivision beatDivision = BeatDivision.Quarter;
        void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            Locator.GameplayInitSignal.AddListener(OnGameInit);
        }

        private void OnGameInit()
        {
            Locator.GameplayInitSignal.RemoveListener(OnGameInit);
            Locator.BeatModel.AddBeatListener((int)beatDivision, BeatListener);
        }

        private void BeatListener(int beat)
        {
            if (meshRenderer != null)
            {
                meshRenderer.enabled = !meshRenderer.enabled;
            }
        }
    }
}