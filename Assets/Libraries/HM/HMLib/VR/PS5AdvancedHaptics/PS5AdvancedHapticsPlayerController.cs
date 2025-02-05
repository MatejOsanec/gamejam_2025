using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Libraries.HM.HMLib.VR;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR;


public class PS5AdvancedHapticsPlayerController : IHapticFeedbackPlayer, IInitializable {

    //Each source can play only one vibration at a time, so we have to reuse them from multiple sounds.
   readonly HapticsAudioClipPlayer.Pool _hapticsPlayerPool;
   readonly ICoroutineStarter _coroutineStarter;

#if (DEVELOPMENT_BUILD || UNITY_EDITOR) && UNITY_PS5
    private Dictionary<string, AudioClip> _hapticClips = new Dictionary<string, AudioClip>();
#endif
    private readonly Dictionary<(XRNode,HapticPresetSO), HapticsAudioClipPlayer> _activePlayers = new Dictionary<(XRNode, HapticPresetSO), HapticsAudioClipPlayer>();

    public void Initialize() {

#if DEVELOPMENT_BUILD && UNITY_PS5
        HotloadHapticClips();
#endif
    }

    public void PlayHapticFeedback(XRNode node, HapticPresetSO hapticPreset) {

#if DEVELOPMENT_BUILD && UNITY_PS5
        //Replace clip with hotloaded one
        if (_hapticClips.TryGetValue(hapticPreset._ps5HapticsClip.name, out AudioClip clip)) {
            hapticPreset._ps5HapticsClip = clip;
        }
#endif

        if (!hapticPreset._continuous) {
            PlayOneShotHapticPreset(node, hapticPreset);
            return;
        }
        PlayContinuousHapticPreset(node, hapticPreset);
    }

    public bool CanPlayHapticPreset(HapticPresetSO hapticPreset, XRNode node) {

        return hapticPreset.hasPS5HapticsClip;
    }
    private void PlayContinuousHapticPreset(XRNode node, HapticPresetSO hapticPreset) {

        if (_activePlayers.TryGetValue((node, hapticPreset), out HapticsAudioClipPlayer hapticsPlayer)) {
            hapticsPlayer.TriggerContinuousHaptic();
            return;
        }
        hapticsPlayer = _hapticsPlayerPool.Spawn();
        _activePlayers[(node,hapticPreset)] = hapticsPlayer;
        hapticsPlayer.PlayHapticsPreset(node,hapticPreset,
            (player) => {
                _activePlayers.Remove((node,hapticPreset));
                OnHapticPlayFinishedCallback(player);
            });
    }

    private void PlayOneShotHapticPreset(XRNode node, HapticPresetSO hapticPreset) {

        if (_activePlayers.TryGetValue((node, hapticPreset), out HapticsAudioClipPlayer hapticsPlayer)) {
            hapticsPlayer.RestartHaptic();
            return;
        }
        hapticsPlayer = _hapticsPlayerPool.Spawn();
        _activePlayers[(node,hapticPreset)] = hapticsPlayer;
        hapticsPlayer.PlayHapticsPreset(node,hapticPreset,
            (player) => {
                _activePlayers.Remove((node,hapticPreset));
                OnHapticPlayFinishedCallback(player);
            });
    }

    private void OnHapticPlayFinishedCallback(HapticsAudioClipPlayer player) {

        _hapticsPlayerPool.Despawn(player);
    }

#if DEVELOPMENT_BUILD && UNITY_PS5
    private void HotloadHapticClips() {

        try {
            _hapticClips = new Dictionary<string, AudioClip>();
            string[] hapticFiles = Directory.GetFiles("/host/haptics/", "*.wav", SearchOption.TopDirectoryOnly);
            if (hapticFiles.Length <= 0) {
                Debug.Log("Haptics Hotreload: No .wav files were found in /host/haptics/");
                return;
            }

            foreach (string clipPath in hapticFiles) {
                _coroutineStarter.StartCoroutine(GetAudioClipFromLocalDisk(clipPath));
            }
        }
        catch (Exception e) {
            Debug.LogWarning($"[DEVELOPMENT-BUILD-ONLY] Exception during Haptics Hotload: {e.Message}");
        }
    }

    IEnumerator GetAudioClipFromLocalDisk(string path) {

            //This looks like the proper way how to load AudioClips from disk at runtime.
            //The clips are loaded from File Serving Directory that is set in PS5 Target Manager.
            //This should allow hotloading of AudioClips without having to rebuild game for faster iteration.
            Debug.Log($"Trying to load clip {path}.");
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.WAV)) {
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.ConnectionError) {
                    Debug.LogError(www.error);
                }
                else {
                    AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                    string clipName = Path.GetFileName(path);
                    clipName = clipName.Replace(".wav","");
                    myClip.name = clipName;
                    _hapticClips[clipName] = myClip;
                    Debug.Log($"Loaded clip {clipName} with length {myClip.length}");
                }
            }
    }
    #endif
}
