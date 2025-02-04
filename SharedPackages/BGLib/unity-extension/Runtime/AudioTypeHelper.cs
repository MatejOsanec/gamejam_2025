using System.IO;
using UnityEngine;

public static class AudioTypeHelper {

    public static AudioType GetAudioTypeFromPath(string path) {

        return Path.GetExtension(path).ToLower() == ".wav" ? AudioType.WAV : AudioType.OGGVORBIS;
    }
}
