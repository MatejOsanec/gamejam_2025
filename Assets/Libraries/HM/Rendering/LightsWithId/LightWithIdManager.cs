using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[ExecuteAlways]
public class LightWithIdManager : MonoBehaviour {

    public event System.Action didChangeSomeColorsThisFrameEvent;

    public const int kMaxLightId = 500;

    private readonly List<ILightWithId>[] _lights = new List<ILightWithId>[kMaxLightId + 1];
    private readonly Color?[] _colors = new Color?[kMaxLightId + 1];
    private readonly List<ILightWithId> _lightsToUnregister = new List<ILightWithId>(100);

    private bool _didChangeSomeColorsThisFrame;

    protected void LateUpdate() {

        foreach (var lightWithId in _lightsToUnregister) {
            _lights[lightWithId.lightId].Remove(lightWithId);
        }
        _lightsToUnregister.Clear();

        if (_didChangeSomeColorsThisFrame) {
            didChangeSomeColorsThisFrameEvent?.Invoke();
        }
        _didChangeSomeColorsThisFrame = false;
    }

    public void RegisterLight(ILightWithId lightWithId) {

        if (lightWithId.isRegistered) {
            return;
        }

        var lightId = lightWithId.lightId;

        Assert.IsTrue(lightId >= -1 && lightId <= kMaxLightId);

        if (lightId == -1) {
            return;
        }

        if (_lights[lightId] == null) {
            _lights[lightId] = new List<ILightWithId>(10);
        }

        _lights[lightId].Add(lightWithId);
        _lightsToUnregister.Remove(lightWithId);

        if (_colors[lightId].HasValue) {
            lightWithId.ColorWasSet(_colors[lightId].Value);
        }
        else {
#if UNITY_EDITOR
            if (Application.isPlaying) {
#endif
                lightWithId.ColorWasSet(Color.clear);
#if UNITY_EDITOR
            }
#endif
        }

        lightWithId.__SetIsRegistered();
    }

    public void UnregisterLight(ILightWithId lightWithId) {

        if (!lightWithId.isRegistered) {
            return;
        }

        var lightId = lightWithId.lightId;

        if (lightId == -1) {
            return;
        }

        if (_lights[lightId] == null) {
            return;
        }

        _lightsToUnregister.Add(lightWithId);
        lightWithId.__SetIsUnRegistered();
    }

    public void SetColorForId(int lightId, Color color) {

        _colors[lightId] = color;

        _didChangeSomeColorsThisFrame = true;

        var lights = _lights[lightId];

        if (lights == null) {
            return;
        }

        for (int i = 0; i < lights.Count; i++) {

            var lightWithId = lights[i];
            if (!lightWithId.isRegistered) {
                continue;
            }
            lightWithId.ColorWasSet(color);
        }
    }

    public Color GetColorForId(int lightId, bool initializeIfNull = false) {

        if (_colors[lightId] != null) {
            return _colors[lightId].Value;
        }
        if (initializeIfNull) {
            SetColorForId(lightId, Color.clear);
        }
        return Color.clear;
    }

#if UNITY_EDITOR
    public bool IsColorSetForId(int lightId) {

        return _colors[lightId] != null;
    }

    public void RequestUpdate() {

        _didChangeSomeColorsThisFrame = true;
        didChangeSomeColorsThisFrameEvent?.Invoke();
    }

    public List<ILightWithId>[] GetLightsArray() => _lights;
#endif
}
