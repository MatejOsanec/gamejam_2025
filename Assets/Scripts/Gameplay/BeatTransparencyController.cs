using Core;
using UnityEngine;

namespace Gameplay
{
    public enum Waveform
    {
        Linear,
        Sinusoidal,
        Square,
        Sawtooth
    }
    
    public class BeatTransparencyController : MonoBehaviour
    {
        public float beatMultiplier = 4; 
        public Waveform waveform = Waveform.Linear; 
        private MeshRenderer meshRenderer;
        private Material material;

        void Start()
        {
            Locator.GameplayInitSignal.AddListener(OnGameInit);
        }
        
        void OnGameInit()
        {
            Locator.GameplayInitSignal.RemoveListener(OnGameInit);
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                material = meshRenderer.material;
            }
        }

        void Update()
        {
            if (material != null)
            {
                // Get the current beat
                float currentBeat = Locator.BeatModel.CurrentBeat;
                float transparency = 0f;
                // Calculate transparency based on the selected waveform
                switch (waveform)
                {
                    case Waveform.Linear:
                        transparency = Mathf.Abs((currentBeat % beatMultiplier) - 1) / beatMultiplier;
                        break;
                    case Waveform.Sinusoidal:
                        transparency = (Mathf.Sin(currentBeat * Mathf.PI / beatMultiplier) + 1) / 2;
                        break;
                    case Waveform.Square:
                        transparency = (Mathf.Floor(currentBeat % beatMultiplier) < beatMultiplier / 2) ? 1f : 0f;
                        break;
                    case Waveform.Sawtooth:
                        transparency = (currentBeat % beatMultiplier) / beatMultiplier;
                        break;
                }
                // Set the material's alpha value
                Color color = material.color;
                color.a = transparency;
                material.color = color;
            }
        }
    }
}