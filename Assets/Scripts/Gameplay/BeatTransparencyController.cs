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
    
    public class BeatTransparencyController : BeatmapCallbackListener
    {
        public float beatMultiplier = 4; 
        public Waveform waveform = Waveform.Linear; 
        private MeshRenderer meshRenderer;
        private Material material;

        void Update()
        {
            if (material != null)
            {
                // Get the current beat
                float transparency = 0f;
                
                // Calculate transparency based on the selected waveform
                switch (waveform)
                {
                    case Waveform.Linear:
                        transparency = Mathf.Abs(Locator.BeatModel.GetBeatProgress(beatMultiplier));
                        break;
                    case Waveform.Sinusoidal:
                        transparency = Mathf.Sin(Locator.BeatModel.GetBeatProgress(beatMultiplier)) / 2f;
                        break;
                    case Waveform.Square:
                        transparency = Mathf.Floor(Locator.BeatModel.GetBeatProgress(beatMultiplier)) < 0.5 ? 1f : 0f;
                        break;
                    case Waveform.Sawtooth:
                        transparency = Locator.BeatModel.GetBeatProgress(beatMultiplier);
                        break;
                }
                // Set the material's alpha value
                Color color = material.color;
                color.a = transparency;
                material.color = color;
            }
        }

        protected override void OnGameInit()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                material = meshRenderer.material;
            }
        }
    }
}