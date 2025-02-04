using UnityEngine;

namespace Core
{
    public class AudioController : MonoBehaviour
    {
        // Reference to the AudioSource component
        public AudioSource audioSource;

        // Property to get the current sample position
        public int Samples
        {
            get
            {
                if (audioSource != null)
                {
                    return audioSource.timeSamples;
                }
                return 0;
            }
        }
        
        public int SampleRate
        {
            get
            {
                if (audioSource != null && audioSource.clip != null)
                {
                    return audioSource.clip.frequency;
                }
                return 0;
            }
        }

        // Property to get the current playhead time in seconds
        public float Time
        {
            get
            {
                if (audioSource != null)
                {
                    return audioSource.time;
                }
                return 0f;
            }
        }

        // Method to play the audio
        public void PlayAudio()
        {
            if (audioSource != null)
            {
                audioSource.Play();
            }
        }

        // Method to stop the audio
        public void StopAudio()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }

        // Method to pause the audio
        public void PauseAudio()
        {
            if (audioSource != null)
            {
                audioSource.Pause();
            }
        }
    }
}