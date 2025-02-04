using UnityEngine;

public interface ILightWithId {

    bool isRegistered { get; }

    void __SetIsRegistered();
    void __SetIsUnRegistered();

    int lightId { get; }
    void ColorWasSet(Color color);
}
