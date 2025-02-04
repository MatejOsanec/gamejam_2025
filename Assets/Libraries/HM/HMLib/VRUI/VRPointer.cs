namespace VRUIControls {

    using System;
    using System.Text;
    using UnityEngine;
    using UnityEngine.EventSystems;

    [RequireComponent(typeof(EventSystem))]
    public class VRPointer : MonoBehaviour {

        [SerializeField] VRController _leftVRController = default;
        [SerializeField] VRController _rightVRController = default;
        [SerializeField] VRLaserPointer _laserPointerPrefab = default;
        [SerializeField] Transform _cursorPrefab = default;
        [SerializeField] float _defaultLaserPointerLength = 10.0f;
#pragma warning disable CS0414
#if !BS_TOURS || UNITY_EDITOR
#if BS_TOURS
        [HideInInspector]
#endif
        [SerializeField] float _laserPointerWidth = 0.01f;
#endif
#pragma warning restore CS0414

        public event Action<VRController> lastUsedControllerChangedEvent;
        public GameObject pointingOver => _currentPointerData?.pointerCurrentRaycast.gameObject;
        public Vector2 flatCanvasWorldPosition => _currentPointerData?.position ?? Vector2.zero;
        public Transform cursorTransform => _cursorTransform;
        public string state {
            get {
                StringBuilder sb = new StringBuilder();
                if (_currentPointerData != null) {
                    sb.AppendFormat("\ndevice id: {0}", _lastSelectedVrController.node);
                    sb.AppendFormat("\nentered: {0}", _currentPointerData.pointerEnter == null ? "none" : _currentPointerData.pointerEnter.name);
                    sb.AppendFormat("\npressed: {0}", _currentPointerData.pointerPress == null ? "none" : _currentPointerData.pointerPress.name);
                    sb.AppendFormat("\ndragged: {0}", _currentPointerData.pointerDrag == null ? "none" : _currentPointerData.pointerDrag.name);
                    sb.AppendFormat("\nselected: {0}", _eventSystem.currentSelectedGameObject == null ? "none" : _eventSystem.currentSelectedGameObject.name);
                }
                return sb.ToString();
            }
        }
        public VRController lastSelectedVrController => _lastSelectedVrController;
        public Vector3 cursorPosition {
            get {
                if (_lastSelectedControllerWasRight) {
                    return _rightCursorTransform == null ? Vector3.zero : _rightCursorTransform.position;
                }
                return _leftCursorTransform == null ? Vector3.zero : _leftCursorTransform.position;
            }
        }
        public const float kScrollMultiplier = 1.0f;
        private const float kTriggerThreshold = 0.1f;

        private PointerEventData _currentPointerData = null;
        private VRLaserPointer _laserPointer;
        private Transform _cursorTransform;
        private VRLaserPointer _leftLaserPointer;
        private VRLaserPointer _rightLaserPointer;
        private Transform _leftCursorTransform;
        private Transform _rightCursorTransform;
        private EventSystem _eventSystem;
        private VRController _lastSelectedVrController;


        private bool _lastSelectedControllerWasRight = true;
        private bool _rightControllerWasReleased = true;
        private bool _leftControllerWasReleased = true;

        private bool _hasLaserPointers;
        private bool _hasCursors;

        protected void Awake() {

            _eventSystem = GetComponent<EventSystem>();
            _hasLaserPointers = CreateLaserPointers();
            _hasCursors = CreateCursors();
            if (_lastSelectedControllerWasRight || !_leftVRController.active) {
                SelectRightController();
            }
            else {
                SelectLeftController();
            }
        }

        protected void LateUpdate() {

            bool isLeftControllerDown = IsLeftControllerDown();
            bool isRightControllerDown = IsRightControllerDown();
            if (isLeftControllerDown && _leftVRController.active && _lastSelectedControllerWasRight) {
                HideLaserPointersAndCursors();
                SelectLeftController();
            }
            if (isRightControllerDown && _rightVRController.active && !_lastSelectedControllerWasRight) {
                HideLaserPointersAndCursors();
                SelectRightController();
            }
            if (_eventSystem.enabled && !_laserPointer.gameObject.activeSelf) {
                EnabledLastSelectedPointer();
            }
            else if (!_eventSystem.enabled && _laserPointer.gameObject.activeSelf) {
                HideLaserPointersAndCursors();
            }
        }

        private bool IsLeftControllerDown() {

            float leftControllerTrigger = _leftVRController.triggerValue;
            bool leftControllerIsReleased = leftControllerTrigger < kTriggerThreshold;
            bool isLeftControllerDown = !leftControllerIsReleased && _leftControllerWasReleased;
            _leftControllerWasReleased = leftControllerIsReleased;
            return isLeftControllerDown;
        }

        private bool IsRightControllerDown() {

            float rightControllerTrigger = _rightVRController.triggerValue;
            bool rightControllerIsReleased = rightControllerTrigger < kTriggerThreshold;
            bool isRightControllerDown = !rightControllerIsReleased && _rightControllerWasReleased;
            _rightControllerWasReleased = rightControllerIsReleased;
            return isRightControllerDown;
        }

        private void SelectRightController() {

            _lastSelectedControllerWasRight = true;
            _lastSelectedVrController = _rightVRController;
            lastUsedControllerChangedEvent?.Invoke(_rightVRController);
            EnabledLastSelectedPointer();
        }

        private void SelectLeftController() {

            _lastSelectedControllerWasRight = false;
            _lastSelectedVrController = _leftVRController;
            lastUsedControllerChangedEvent?.Invoke(_leftVRController);
            EnabledLastSelectedPointer();
        }

        private void EnabledLastSelectedPointer() {

            if (_lastSelectedControllerWasRight) {
                _laserPointer = _rightLaserPointer;
                _cursorTransform = _rightCursorTransform;
            }
            else {
                _laserPointer = _leftLaserPointer;
                _cursorTransform = _leftCursorTransform;
            }
            _laserPointer.gameObject.SetActive(true);
        }

        private bool CreateLaserPointers() {

            if (_laserPointerPrefab == null) {
                return false;
            }
            _leftLaserPointer = Instantiate(_laserPointerPrefab, _leftVRController.viewAnchorTransform, false);
            SetupLaserPointer(_leftLaserPointer);

            _rightLaserPointer = Instantiate(_laserPointerPrefab, _rightVRController.viewAnchorTransform, false);
            SetupLaserPointer(_rightLaserPointer);
            return true;
        }

        private void SetupLaserPointer(VRLaserPointer laserPointer) {

#if BS_TOURS
            laserPointer.SetLength(_defaultLaserPointerLength);
#else
            laserPointer.SetLocalPosition(new Vector3(0.0f, 0.0f, _defaultLaserPointerLength / 2));
            laserPointer.SetLocalScale(new Vector3(_laserPointerWidth, _laserPointerWidth, _defaultLaserPointerLength));
            laserPointer.SetFadeDistance(1.0f);
#endif
            laserPointer.gameObject.SetActive(false);
        }
        private bool CreateCursors() {

            if (_cursorPrefab == null) {
                return false;
            }
            _leftCursorTransform = Instantiate(_cursorPrefab, transform, false);
            _leftCursorTransform.gameObject.SetActive(false);
            _rightCursorTransform = Instantiate(_cursorPrefab, transform, false);
            _rightCursorTransform.gameObject.SetActive(false);
            return true;
        }

        private void RefreshLaserPointerAndLaserHit(PointerEventData pointerData) {

            if (float.IsNaN(pointerData.pointerCurrentRaycast.worldPosition.x)) {
                return;
            }

            if (pointerData.pointerCurrentRaycast.gameObject != null) {
                if (_laserPointer != null) {
#if BS_TOURS
                    _laserPointer.SetLength(pointerData.pointerCurrentRaycast.distance);
#else
                    _laserPointer.SetLocalPosition(new Vector3(0.0f, 0.0f, pointerData.pointerCurrentRaycast.distance / 2));
                    _laserPointer.SetLocalScale(new Vector3(_laserPointerWidth, _laserPointerWidth, pointerData.pointerCurrentRaycast.distance));
                    // Cannot pass 0, because GlowingPointer.shader uses it in division
                    _laserPointer.SetFadeDistance(Mathf.Epsilon);
#endif
                }
                if (_cursorTransform != null) {
                    _cursorTransform.position = pointerData.pointerCurrentRaycast.worldPosition;
                    _cursorTransform.gameObject.SetActive(true);
                }
            }
            else {
                if (_laserPointer != null) {
#if BS_TOURS
                    _laserPointer.SetLength(_defaultLaserPointerLength);
#else
                    _laserPointer.SetLocalPosition(new Vector3(0.0f, 0.0f, _defaultLaserPointerLength / 2));
                    _laserPointer.SetLocalScale(new Vector3(_laserPointerWidth, _laserPointerWidth, _defaultLaserPointerLength));
                    _laserPointer.SetFadeDistance(1.0f);
#endif
                }
                if (_cursorTransform != null) {
                    _cursorTransform.gameObject.SetActive(false);
                }
            }
        }

        private void OnApplicationFocus(bool hasFocus) {

            if (!hasFocus) {
                HideCursors();
            }
        }

        private void HideLaserPointersAndCursors() {

            HideLaserPointers();
            HideCursors();
        }

        private void HideLaserPointers() {

            if (!_hasLaserPointers) {
                return;
            }
            _leftLaserPointer.gameObject.SetActive(false);
            _rightLaserPointer.gameObject.SetActive(false);

        }

        private void HideCursors() {

            if (!_hasCursors) {
                return;
            }
            _leftCursorTransform.gameObject.SetActive(false);
            _rightCursorTransform.gameObject.SetActive(false);
        }

        public void Process(PointerEventData pointerEventData) {

            // Swap controller when selected is no longer active and the other controller is active
            if (!_lastSelectedVrController.active) {
                if (_lastSelectedControllerWasRight && _leftVRController.active) {
                    HideLaserPointersAndCursors();
                    SelectLeftController();
                }
                else if (!_lastSelectedControllerWasRight && _rightVRController.active) {
                    HideLaserPointersAndCursors();
                    SelectRightController();
                }
            }
            // If no controller is active, we don't refresh laser pointer
            if (!_lastSelectedVrController.active) {
                return;
            }
            _currentPointerData = pointerEventData;
            RefreshLaserPointerAndLaserHit(_currentPointerData);
        }
    }
}
