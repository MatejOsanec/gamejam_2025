using UnityEngine;
using UnityEngine.Playables;

public class TimelinePauseReceiver : MonoBehaviour, INotificationReceiver {

#if !BS_TOURS
    public event System.Action timelinePauseEvent;
#endif
#if BS_TOURS || UNITY_EDITOR
#pragma warning disable CS0414
    [FutureField]
    [SerializeField] PlayableDirector _playableDirector;
#pragma warning restore CS0414
#endif

    public void OnNotify(Playable origin, INotification notification, object context) {

        if (notification is TimelinePauseMarker) {
#if BS_TOURS
            _playableDirector.Pause();
#else
            timelinePauseEvent?.Invoke();
#endif
        }
    }
}
