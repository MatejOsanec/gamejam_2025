namespace BeatmapEditor3D.DataModels {

    using System.Collections.Generic;
    using UnityEngine;

    public class BpmData {

        public List<BpmRegion> regions => _regions;
        public float totalBeats => _lastBeat;
        public int startOffset => _startOffset;
        public int sampleCount => _sampleCount;
        public int frequency => _frequency;

        private readonly List<BpmRegion> _regions = new List<BpmRegion>();
        private readonly int _frequency;
        private readonly int _sampleCount;
        private readonly int _startOffset;
        private readonly float _lastBeat;
        private readonly int _lastSample;

        public BpmData(float bpm, int sampleCount, int frequency, int startOffset = 0) {

            _startOffset = startOffset;
            _frequency = frequency;
            _sampleCount = sampleCount;

            var beats = AudioTimeHelper.SamplesToBeats(sampleCount, _frequency, bpm);
            var clippedSamples = AudioTimeHelper.BeatsToSamples(beats, _frequency, bpm);

            var newRegion = new BpmRegion(0, clippedSamples - 1, 0, beats, frequency);
            _regions.Add(newRegion);

            _lastBeat = _regions[^1].endBeat;
            _lastSample = _regions[^1].endSampleIndex;
        }

        public BpmData(int frequency, int startOffset, IReadOnlyList<BpmRegion> regions) {

            _startOffset = startOffset;
            _frequency = frequency;

            _regions.AddRange(regions);

            _lastBeat = _regions[^1].endBeat;
            _lastSample = _regions[^1].endSampleIndex;

            _sampleCount = _lastSample;
        }

        public BpmRegion GetRegionAtBeat(float beat) {

            beat = Mathf.Clamp(beat, 0, _lastBeat);

            var index = SearchRegionsBinaryByBeat(0, _regions.Count - 1, beat);
            return _regions[index];
        }

        public BpmRegion GetRegionAtSample(int sample) {

            sample = Mathf.Clamp(sample, 0, _lastSample);

            var index = SearchRegionsBinaryBySample(0, _regions.Count - 1, sample);
            return _regions[index];
        }

        public float BeatToSeconds(float beat) {

            return AudioTimeHelper.SamplesToSeconds(BeatToSample(beat), _frequency);
        }

        public float SecondsToBeat(float time) {

            return SampleToBeat(AudioTimeHelper.SecondsToSamples(time, _frequency));
        }

        //TODO: move out of BPM or rename for generic use
        public float FadeEndBeat(float time, float fadeDuration) {

            return SecondsToBeat(BeatToSeconds(time) + fadeDuration);
        }

        public float SampleToBeat(int sampleIndex) {

            var region = GetRegionAtSample(sampleIndex);
            var localSampleIndex = sampleIndex - region.startSampleIndex;

            float localBeats = (localSampleIndex / (float)region.sampleCount) * region.beats;
            return region.startBeat + localBeats;
        }

        public int BeatToSample(float beat) {

            var region = GetRegionAtBeat(beat);
            var localBeat = beat - region.startBeat;

            float localSample = localBeat / region.beats * region.sampleCount;
            return (int)(region.startSampleIndex + localSample);
        }

        private int SearchRegionsBinaryByBeat(int l, int r, float beat) {

            while (true) {
                if (r < l) {
                    return _regions.Count - 1;
                }

                int mid = l + (r - l) / 2;

                var compValue = CompareRegionByBeat(_regions[mid], beat);
                if (compValue == 0) {
                    return mid;
                }

                if (compValue < 0) {
                    r = mid - 1;
                }
                else {
                    l = mid + 1;
                }
            }
        }

        private int SearchRegionsBinaryBySample(int l, int r, int sample) {

            while (true) {
                if (r < l) {
                    return _regions.Count - 1;
                }

                int mid = l + (r - l) / 2;

                var compValue = CompareRegionBySampleIndex(_regions[mid], sample);
                if (compValue == 0) {
                    return mid;
                }

                if (compValue < 0) {
                    r = mid - 1;
                }
                else {
                    l = mid + 1;
                }
            }
        }

        private static int CompareRegionBySampleIndex(BpmRegion region, int sample) {

            if (sample < region.startSampleIndex) {
                return -1;
            }
            if (region.endSampleIndex < sample) {
                return 1;
            }

            return 0;
        }

        private static int CompareRegionByBeat(BpmRegion region, float beat) {

            if (region.startBeat <= beat && beat < region.endBeat) {
                return 0;
            }

            if (beat < region.startBeat) {
                return -1;
            }

            return 1;
        }
    }

    public class BpmRegion {

        public const float kMinBpm = 1;
        public const float kMaxBpm = 1015; // :)

        public int sampleCount => endSampleIndex - startSampleIndex;
        public int samplesPerBeat => (int)(sampleCount / beats);

        // Samples
        public readonly int startSampleIndex;    // Absolute
        public readonly int endSampleIndex;      // Absolute

        // Beats
        public readonly float bpm;
        public readonly float beats;

        public readonly float startBeat;    // Set from outside
        public readonly float endBeat;      // Set from outside

        public readonly int sampleFrequency;

        public BpmRegion(int startSampleIndex, int endSampleIndex, float startBeat, float endBeat, int sampleFrequency) {

            this.startSampleIndex = startSampleIndex;
            this.endSampleIndex = endSampleIndex;
            this.startBeat = startBeat;
            this.endBeat = endBeat;
            this.beats = endBeat - startBeat;
            this.sampleFrequency = sampleFrequency;
            this.bpm = AudioTimeHelper.SamplesToBPM(sampleCount, sampleFrequency, beats);
        }

        public BpmRegion(BpmRegion other, int? startSampleIndex = null, int? endSampleIndex = null, float? startBeat = null, float? endBeat = null, int? sampleFrequency = null) {

            this.startSampleIndex = startSampleIndex ?? other.startSampleIndex;
            this.endSampleIndex = endSampleIndex ?? other.endSampleIndex;
            this.startBeat = startBeat ?? other.startBeat;
            this.endBeat = endBeat ?? other.endBeat;
            this.beats = this.endBeat - this.startBeat;
            this.sampleFrequency = sampleFrequency ?? other.sampleFrequency;
            this.bpm = AudioTimeHelper.SamplesToBPM(sampleCount, this.sampleFrequency, beats);
        }

        public int BeatToSample(float beat) => AudioTimeHelper.BeatToSample(startSampleIndex, endSampleIndex, startBeat, endBeat, beat);
        public float SampleToBeat(int sample) =>AudioTimeHelper.SampleToBeat(startSampleIndex, endSampleIndex, startBeat, endBeat, sample);
    }
}
