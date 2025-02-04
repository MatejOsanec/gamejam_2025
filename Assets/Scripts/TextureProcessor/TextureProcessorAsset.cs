using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class TextureProcessorAsset : PlayableAsset, ITimelineClipAsset {

    public TextureProcessorBehaviour _template;

    public ClipCaps clipCaps => ClipCaps.AutoScale | ClipCaps.Extrapolation | ClipCaps.Blending;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject) {

        var playable = ScriptPlayable<TextureProcessorBehaviour>.Create(graph, _template);
        return playable;
    }
}
