using Beatmap;

namespace Core
{
    using UnityEngine;

    public class GameStateManager
    {
        public GameState GameState { get; private set; }

        private float _startBeat;
        private Transform[] initSceneGameobjects;
        private Transform[] gameSceneGameobjects;
        private AudioController audioController;

        public GameStateManager(Transform[] initObjects, Transform[] gameObjects, AudioController audioCtrl, float startBeat)
        {
            initSceneGameobjects = initObjects;
            gameSceneGameobjects = gameObjects;
            audioController = audioCtrl;
            _startBeat = startBeat;
        }

        public void SetState(GameState gameState)
        {
            SetActiveByState(gameState, GameState.Init, initSceneGameobjects);
            SetActiveByState(gameState, GameState.Game, gameSceneGameobjects);

            if (gameState == GameState.Game)
            {
                audioController.PlayAudio();
                audioController.audioSource.timeSamples = Locator.Model.BpmData.GetRegionAtBeat(_startBeat).BeatToSample(_startBeat);

                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        OnColorNotePassed(new ColorNote() { x = x, y = y, beat = 8 });
                    }
                }
            }

            GameState = gameState;
        }

        public void SetActiveByState(GameState targetState, GameState currentState, Transform[] gameobjects)
        {
            foreach (var go in gameobjects)
            {
                bool enable = targetState == currentState;
                if (!go.gameObject.activeSelf && enable)
                {
                    go.gameObject.SetActive(true);
                }else if (!enable)
                {
                    go.gameObject.SetActive(false);    
                }
            }
        }

        private void OnColorNotePassed(ColorNote colorNote)
        {
            // Implement your logic for handling color notes here
        }
    }

    public enum GameState
    {
        Init,
        Game
    }
}