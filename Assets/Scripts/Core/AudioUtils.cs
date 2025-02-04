namespace Core
{
    public static class AudioUtils
    {
        /// <summary>
        /// Converts samples to beats.
        /// </summary>
        /// <param name="samples">The number of audio samples.</param>
        /// <param name="sampleRate">The sample rate of the audio (samples per second).</param>
        /// <param name="bpm">The tempo of the audio in beats per minute.</param>
        /// <returns>The number of beats corresponding to the given samples.</returns>
        public static float SamplesToBeats(int samples, int sampleRate, float bpm)
        {
            // Calculate the number of seconds for the given samples
            float seconds = (float)samples / sampleRate;
            // Calculate beats per second
            float beatsPerSecond = bpm / 60f;
            // Calculate the number of beats
            float beats = seconds * beatsPerSecond;
            return beats;
        }
    }
}