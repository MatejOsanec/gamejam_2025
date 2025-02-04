using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackClipType(typeof(TextureProcessorAsset))]
[TrackBindingType(typeof(TextureProcessor))]
[TrackColor(1.0f, 1.0f, 1.0f)]
public class TextureProcessorTrack : TrackAsset {
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount) {
        return ScriptPlayable<TextureProcessorMixerBehaviour>.Create(graph, inputCount);
    }
}
