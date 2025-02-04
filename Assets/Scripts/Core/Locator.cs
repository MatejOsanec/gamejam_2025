namespace Core
{
    public static class Locator
    {
        public static Settings Settings;
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