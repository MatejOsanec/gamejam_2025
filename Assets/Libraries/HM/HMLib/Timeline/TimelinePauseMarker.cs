using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[DisplayName("Timeline/TimelinePauseMarker")]
public class TimelinePauseMarker : Marker, INotification {

    public PropertyName id { get; }
}
