using Core;
using UnityEngine;
using Gameplay;

public class Synaesthetizer : BeatmapCallbackListener
{
    [SerializeField]
    private ModulationDefSo modA;
    [SerializeField]
    private ModulationDefSo modB;
    [SerializeField]
    private ModulationDefSo modC;
    [SerializeField]
    private float _initialAmplitudeA;
    [SerializeField]
    private float _initialAmplitudeB;
    [SerializeField]
    private float _initialAmplitudeC;

    [SerializeField] private TextureProcessor _texProcessor;


    protected override void OnGameInit()
    {
        
        _texProcessor.amplitudeA = _initialAmplitudeA;
        _texProcessor.amplitudeB = _initialAmplitudeB;
        _texProcessor.amplitudeC = _initialAmplitudeC;
    }
    
    void Update()
    {
        if (_initialized && Locator.GameStateManager.GameState == GameState.Game)
        {

            _texProcessor.amplitudeA = modA.GetProgress();
            _texProcessor.amplitudeB = modB.GetProgress();
            _texProcessor.amplitudeC = modC.GetProgress();
        }
    }
}
