using strange.extensions.signal.impl;

namespace Core
{
    public static class Locator
    {
        public static Signal GameplayInitSignal = new Signal();
        public static Settings Settings;
        public static BeatModel BeatModel;
    }

    public class Settings
    {
        public float NoteSpeed { get; private set; }
        public float PlacementMultiplier { get; private set; }
        public float PreSpawnBeats { get; private set; }
        
        public Settings(float noteSpeed, float placementMultiplier, float preSpawnBeats)
        {
            NoteSpeed = noteSpeed;
            PlacementMultiplier = placementMultiplier;
            PreSpawnBeats = preSpawnBeats;
        }
    }
}