using UnityEngine;
using UnityEngine.Playables;

public class JumpReceiver : MonoBehaviour, INotificationReceiver {

    public bool jumpToDestinationValid { get; set; }

    public void OnNotify(Playable origin, INotification notification, object context) {

        if (notification is JumpMarker jumpMarker && jumpToDestinationValid) {
            var destination = jumpMarker.jumpDestination;
            if (destination == null) {
                return;
            }

            var timelinePlayable = origin.GetGraph().GetRootPlayable(0);
            timelinePlayable.SetTime(destination.time);
        }
    }
}
