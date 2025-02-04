using UnityEngine;
using Zenject;

public class DeactivateMenuControllersOnFocusCaptureOrTrackingLost : MonoBehaviour, IVerboseLogger {

    [SerializeField] private VRController[] _vrControllers;

    public string loggerPrefix => "InputFocus";

    // It won't work in Editor with headset connected but not used
#if !UNITY_EDITOR || !BS_IGNORE_VR_FOCUS_LOST_EVENTS
    public bool forceEnabled {
        get {
            return _forceEnabled;
        }
        set {
            _forceEnabled = value;
            UpdateMenuControllersState();
        }
    }

    private bool _forceEnabled = false;
    private IVRPlatformHelper _vrPlatformHelper;

    [Inject]
    private void Init(IVRPlatformHelper vrPlatformHelper) {

        _vrPlatformHelper = vrPlatformHelper;
    }

    protected void Awake() {

        UpdateMenuControllersState();
    }

    protected void LateUpdate() {

        // Late update to allow controllers to update their positions, before activating/deactivating
        UpdateMenuControllersState();
    }

    private void UpdateMenuControllersState() {

        bool hasInputFocus = _vrPlatformHelper.hasInputFocus;
        foreach (var vrController in _vrControllers) {
            // Controllers can become null on HMD unmounting
            if (vrController == null) {
                continue;
            }
            // TODO: We already have an event for connect/disconnect controllers. Trying to query the position is unnecessary work: BS-13319
            bool isTracking = _vrPlatformHelper.GetNodePose(vrController.node, vrController.nodeIdx, out Vector3 _, out Quaternion _);
            bool shouldControllersBeActive = hasInputFocus && isTracking;
            SetActiveMenuController(_forceEnabled || shouldControllersBeActive, vrController);
        }
    }

    private void SetActiveMenuController(bool active, VRController vrController) {

        if (vrController.gameObject.activeSelf != active) {
            this.Log($"{ (active ? "Activating" : "Deactivating" )} MenuControllers '{vrController.gameObject.name}'");
            vrController.gameObject.SetActive(active);
        }
    }
#endif
}
