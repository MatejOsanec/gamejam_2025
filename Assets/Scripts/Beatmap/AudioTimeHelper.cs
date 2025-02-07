using System;
using UnityEngine;

namespace BeatmapEditor3D.DataModels {

    public static class AudioTimeHelper {

        public const float kMaxBeat = 3000.0f;
        public const int kBeatsPerWholeBar = 4;
        public const int kMaxBeatSubdivision = 256;
        public const float kMinBeatDifference = 1.0f / kMaxBeatSubdivision - 0.002f;
        public const int kSubdivisionMultiplier = 128;
        private const float kSecondsPerMinute =  60f;

        public static bool IsBeatSame(float a, float b) {

            return Mathf.Abs(a - b) < kMinBeatDifference;
        }

        public static int SecondsToSamples(float seconds, int frequency) {

            return Mathf.FloorToInt(seconds * frequency);
        }

        public static float SecondsToBPM(float seconds, float beats) {

            return beats / seconds * kSecondsPerMinute;
        }

        public static float SecondsToBeats(float seconds, float bpm = 1) {

            return seconds / kSecondsPerMinute * bpm;
        }

        public static float SamplesToSeconds(int samples, int frequency) {

            return samples / (float) frequency;
        }

        public static float SamplesToBPM(int samples, int frequency, float beats) {

            return SecondsToBPM(SamplesToSeconds(samples, frequency), beats);
        }

        public static float SamplesToBeats(int samples, int frequency, float bpm) {

            return SecondsToBeats(SamplesToSeconds(samples, frequency), bpm);
        }

        public static float BeatsToSeconds(float beats, float bpm) {

            return beats / bpm * kSecondsPerMinute;
        }

        public static int BeatsToSamples(float beats, int frequency, float bpm) {

            return Mathf.FloorToInt(BeatsToSeconds(beats, bpm) * frequency);
        }

        public static float ChangeBeatBySubdivision(float currentBeat, int subdivision, int subdivisionDelta, int minValue = 0) {

            int newBeatAsSubdivisionsCount = Math.Max(Mathf.RoundToInt(currentBeat / kBeatsPerWholeBar * subdivision + subdivisionDelta), minValue);
            return newBeatAsSubdivisionsCount * (kBeatsPerWholeBar / (float)subdivision);
        }

        public static float RoundToBeat(float beat, int subdivision) {

            return Mathf.Max(Mathf.RoundToInt(beat / kBeatsPerWholeBar * subdivision), 0) * (kBeatsPerWholeBar / (float)subdivision);
        }

        public static float RoundToBeatRelative(float beat, float relativeBeat, int subdivision) {

            return  relativeBeat + RoundToBeat(beat - relativeBeat, subdivision);
        }

        public static float RoundDownToBeat(float beat, int subdivision) {

            return Mathf.Max(Mathf.Floor(beat / kBeatsPerWholeBar * subdivision), 0) * (kBeatsPerWholeBar / (float)subdivision);
        }

        public static float RoundUpToBeat(float beat, int subdivision) {

            return Mathf.Max(Mathf.Ceil(beat / kBeatsPerWholeBar * subdivision), 0) * (kBeatsPerWholeBar / (float)subdivision);
        }

        public static int BeatToSample(int startSampleIndex, int endSampleIndex, float startBeat, float endBeat, float beat) => (int)Mathf.Lerp(startSampleIndex, endSampleIndex, Mathf.InverseLerp(startBeat, endBeat, beat));
        public static float SampleToBeat(int startSampleIndex, int endSampleIndex, float startBeat, float endBeat, int sample) => Mathf.Lerp(startBeat, endBeat, Mathf.InverseLerp(startSampleIndex, endSampleIndex, sample));
    }
}
