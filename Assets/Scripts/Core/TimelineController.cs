using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;
using Gameplay;
using UnityEngine.Playables;

public class TimelineController : MonoBehaviour
{
    private bool _playing = false;
    [SerializeField]
    private PlayableDirector _playableDirector;

    // Update is called once per frame
    void Update()
    {
        if (!_playing && Locator.GameStateManager.GameState == GameState.Game)
        {
            _playableDirector.time = 0f;
            _playableDirector.Evaluate();
            _playableDirector.Play();
            _playing = true;

        }
        if (_playing && Locator.GameStateManager.GameState != GameState.Game)
        {
            _playing = false;
            {
                _playableDirector.time = 0f;
                _playableDirector.Stop();
                _playableDirector.Evaluate();
            }
        }
    }
}
