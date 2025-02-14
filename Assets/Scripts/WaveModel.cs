using Core;
using strange.extensions.signal.impl;

public class WaveModel
{
    public Signal<int> WaveChangedSignal = new();
    public int CurrentWaveId => _currentWaveId;

    private int _currentWaveId = 0;

    private int[] waveBeatStarts = new[] {0, 240, 500, };

    public void Update()
    {
        var nextWaveIndex = _currentWaveId + 1;

        if (nextWaveIndex >= waveBeatStarts.Length)
        {
            return;
        }

        if (Locator.BeatModel.CurrentBeat >= waveBeatStarts[nextWaveIndex])
        {
            _currentWaveId++;
            WaveChangedSignal.Dispatch(_currentWaveId);
        }
    }

    public int GetWaveByBeat(float beat)
    {
        var currentWave = 0;
        foreach (var waveBeatStart in waveBeatStarts)
        {
            if (waveBeatStart >= beat)
            {
                break;
            }

            currentWave++;
        }

        return currentWave;
    }
}