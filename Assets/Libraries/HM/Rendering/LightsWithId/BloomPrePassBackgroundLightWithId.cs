using UnityEngine;

public class BloomPrePassBackgroundLightWithId : LightWithIdMonoBehaviour {

    [SerializeField] BloomPrePassBackgroundColor _bloomPrePassBackgroundColor = default;

    public Color color => _bloomPrePassBackgroundColor.color;

    public override void ColorWasSet(Color newColor) {

        _bloomPrePassBackgroundColor.color = newColor;
    }
}
