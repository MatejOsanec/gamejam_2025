using TMPro;
using UnityEngine;

// Taken and modified from https://forum.unity.com/threads/textmeshpro-precull-dorebuilds-performance.762968/#post-5083490
public class TextMeshProAutosizeGroup: MonoBehaviour {

    [SerializeField] TMP_Text[] _texts = default;

    protected void Start() {

        if (_texts == null || _texts.Length == 0) {
            return;
        }

        var minFontSize = float.MaxValue;

        foreach (var text in _texts) {
            text.ForceMeshUpdate(ignoreActiveState: true);
            minFontSize = Mathf.Min(minFontSize, text.fontSize);
        }

        // Iterate over all other text objects to set the point size
        foreach (var text in _texts) {
            text.enableAutoSizing = false;
            text.fontSize = minFontSize;
        }
    }
}
