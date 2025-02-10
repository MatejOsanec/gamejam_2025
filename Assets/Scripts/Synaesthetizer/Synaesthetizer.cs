using Beatmap;
using Beatmap.Lightshow;
using UnityEngine;
using Core;

public class Synaesthetizer : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve[] _envelopes;
    [SerializeField]
    private int _envelopeIndexA;
    [SerializeField]
    private int _envelopeIndexB;
    [SerializeField]
    private int _envelopeIndexC;
    [SerializeField]
    private float _envelopeSpeed;
    [SerializeField]
    private float _initialAmplitudeA;
    [SerializeField]
    private float _initialAmplitudeB;
    [SerializeField]
    private float _initialAmplitudeC;

    [SerializeField] private TextureProcessor _texProcessor;
    [SerializeField] private Init _initor;
    private float _timerA, _timerB, _timerC;
    private float _envelopeValueA, _envelopeValueB, _envelopeValueC;
    private bool _initialized = false;
    void Start()
    {
        _timerA = 0.0f;
        _timerB = 0.0f;
        _timerC = 0.0f;
        _texProcessor.amplitudeA = _initialAmplitudeA;
        _texProcessor.amplitudeB = _initialAmplitudeB;
        _texProcessor.amplitudeC = _initialAmplitudeC;
    }

    // Update is called once per frame
    void Update()
    {
        if (_initor._initialized)
        {
            if (!_initialized)
            {
                Locator.Callbacks.AddBeatListener(BeatDivision.Whole, BeatListener);
                Locator.Callbacks.AddBeatListener(BeatDivision.Half, BeatHalfListener);
                _initialized = true;
            }
            _timerA += _envelopeSpeed*Time.deltaTime;
            _timerB += _envelopeSpeed*Time.deltaTime;
            _timerB += _envelopeSpeed*Time.deltaTime;
            _envelopeValueA = _envelopes[_envelopeIndexA].Evaluate(_timerA);
            _envelopeValueB = _envelopes[_envelopeIndexB].Evaluate(_timerB);
            _envelopeValueC = _envelopes[_envelopeIndexC].Evaluate(_timerC);
            _texProcessor.amplitudeA = _envelopeValueA;
            _texProcessor.amplitudeB = _envelopeValueB;
        }
    }

    private void BeatListener(int beat)
    {
        _timerB = 0.0f;
    }
    private void BeatHalfListener(int beat)
    {
        _timerA = 0.0f;
    }

    private void NoteMissHandler(ColorNote n)
    {
        //_timerA = 0.0f;
    }

}
