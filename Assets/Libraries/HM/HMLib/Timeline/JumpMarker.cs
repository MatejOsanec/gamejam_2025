using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[DisplayName("Jump/JumpMarker")]
public class JumpMarker : Marker, INotification {

    [SerializeField] JumpDestinationMarker _destination = default;

    public PropertyName id { get; }

    public JumpDestinationMarker jumpDestination => _destination;

#if UNITY_EDITOR
    private void OnValidate() {

        if (parent == null) {
            return;
        }

        // Automatically finds paired with first destination marker - good for now, might be cool to create editor if needed
        var markers = parent.GetMarkers().OfType<JumpDestinationMarker>().ToList();
        if (markers.Count == 0) {
            return;
        }

        _destination = markers[0];
    }
#endif
}
