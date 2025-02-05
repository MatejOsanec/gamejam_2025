using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;
using Zenject;

namespace HMUI {

    [RequireComponent(typeof(EventSystemListener))]
    public class ScrollView : MonoBehaviour {

        [SerializeField] RectTransform _viewport = default;
        [SerializeField] RectTransform _contentRectTransform = default;

        [Space]
        [SerializeField] ScrollViewDirection _scrollViewDirection = default;

        [Space]
        [SerializeField] Button _pageUpButton = default;
        [SerializeField] Button _pageDownButton = default;
        [SerializeField] VerticalScrollIndicator _verticalScrollIndicator = default;
        [SerializeField] VerticalScrollController _verticalScrollController = default;

        [Space]
        [SerializeField] float _smooth = 8.0f;
#if BS_TOURS
        [SerializeField] float _joystickScrollSpeed = 360.0f;
#else
        [SerializeField] float _joystickScrollSpeed = 60.0f;
#endif
        [SerializeField] float _joystickQuickSnapMaxTime = 0.7f;

        [Space]
        public ScrollType _scrollType = ScrollType.PageSize;

        [Space]
        [DrawIf("_scrollType", value: ScrollType.FixedCellSize)]
        public float _fixedCellSize = 10.0f;
        [DrawIf("_scrollType", value: ScrollType.FocusItems)]
        public float _scrollItemRelativeThresholdPosition = 0.4f;
        [DrawIf("_scrollType", value: ScrollType.FocusItems, orValue: ScrollType.PageSize)]
        public float _pageStepNormalizedSize = 0.7f;

#if BS_TOURS

        public event Action didGainScrollFocus;
        public event Action didLoseScrollFocus;

        private bool _needsScrolling;
        private bool _joystickScrollingEnabled = true;
#endif

        private bool _scrollingLastFrame;
        private bool _isHoveredByPointer;
        private bool _shouldAnimate;

        private const float kThumbstickThreshold = 0.01f;

        [Inject] readonly IVRPlatformHelper _platformHelper = default;

        public enum ScrollType {
            PageSize,
            FixedCellSize,
            FocusItems,
        }

        private enum ScrollDirection {
            None,
            Up,
            Down,
            Left,
            Right
        }

        private enum ScrollViewDirection {
            Vertical,
            Horizontal
        }

        public event Action<float> scrollPositionChangedEvent;

        public RectTransform viewportTransform => _viewport;
        public RectTransform contentTransform => _contentRectTransform;
        public float position => _scrollViewDirection == ScrollViewDirection.Vertical ? _contentRectTransform.anchoredPosition.y : _contentRectTransform.anchoredPosition.x;
        public float scrollableSize => Mathf.Max(contentSize - scrollPageSize, 0.0f);

        private ButtonBinder _buttonBinder;
        private float _destinationPos;
        private float[] _scrollFocusPositions;
        private EventSystemListener _eventSystemListener;

        private float scrollPageSize => _scrollViewDirection == ScrollViewDirection.Vertical ? _viewport.rect.height : _viewport.rect.width;
        private float contentSize => _scrollViewDirection == ScrollViewDirection.Vertical ? _contentRectTransform.rect.height : _contentRectTransform.rect.width;
        private ScrollDirection _lastJoystickScrollDirection = ScrollDirection.None;
        private float _joystickScrollStartTime;


        protected void Awake() {

            _eventSystemListener = GetComponent<EventSystemListener>();
            _eventSystemListener.pointerDidEnterEvent += HandlePointerDidEnter;
            _eventSystemListener.pointerDidExitEvent += HandlePointerDidExit;

            _buttonBinder = new ButtonBinder();
            if (_pageUpButton != null) {
                _buttonBinder.AddBinding(_pageUpButton, PageUpButtonPressed);
            }
            if (_pageDownButton != null) {
                _buttonBinder.AddBinding(_pageDownButton, PageDownButtonPressed);
            }

            if (_verticalScrollController != null) {
                _verticalScrollController.updateScrollPositionEvent += ScrollToByPercentage;
            }

            UpdateContentSize();

            // Focus Items
            if (_scrollType == ScrollType.FocusItems) {
                var items = GetComponentsInChildren<ItemForFocussedScrolling>(includeInactive: true);
                switch (_scrollViewDirection) {
                    case ScrollViewDirection.Vertical:
                        _scrollFocusPositions = items.Select(item => WorldPositionToScrollViewPosition(item.transform.position).y).OrderBy(i => i).ToArray();
                        break;
                    case ScrollViewDirection.Horizontal:
                        _scrollFocusPositions = items.Select(item => WorldPositionToScrollViewPosition(item.transform.position).x).OrderBy(i => i).ToArray();
                        break;
                }
            }

            RefreshButtons();
            _shouldAnimate = false;
        }

        protected void OnDestroy() {

            _buttonBinder?.ClearBindings();

            if (_verticalScrollController != null) {
                _verticalScrollController.updateScrollPositionEvent -= ScrollToByPercentage;
            }

            if (_eventSystemListener != null) {
                _eventSystemListener.pointerDidEnterEvent -= HandlePointerDidEnter;
                _eventSystemListener.pointerDidExitEvent -= HandlePointerDidExit;
            }
        }

        private void CheckScrollInput() {

            if (_platformHelper.hasInputFocus) {
                Vector2 anyJoystickInput = _platformHelper.GetAnyJoystickMaxAxis();
                if (anyJoystickInput.sqrMagnitude > kThumbstickThreshold) {
                    HandleJoystickWasNotCenteredThisFrame(anyJoystickInput);
                    _scrollingLastFrame = true;
                }
                else if (_scrollingLastFrame) {
                    _scrollingLastFrame = false;
                    HandleJoystickWasCenteredThisFrame();
                }
            } else if (_scrollingLastFrame) {
                _scrollingLastFrame = false;
                HandleJoystickWasCenteredThisFrame();
            }
        }

        protected void Update() {

#if BS_TOURS
            if (_isHoveredByPointer && _joystickScrollingEnabled) {
                CheckScrollInput();
            }
#else
            if (_isHoveredByPointer) {
                CheckScrollInput();
            }
#endif
            if (!_shouldAnimate) {
                if (!_isHoveredByPointer) {
                    enabled = false;
                }
                return;
            }
            var anchoredPosition = _contentRectTransform.anchoredPosition;
            var currentPos = _scrollViewDirection == ScrollViewDirection.Vertical ? anchoredPosition.y : anchoredPosition.x;
            var adjustedDestination = _scrollViewDirection == ScrollViewDirection.Vertical ? _destinationPos : -_destinationPos;

            var pos = Mathf.Lerp(currentPos, adjustedDestination, Time.deltaTime * _smooth);
            if (Mathf.Abs(currentPos - _destinationPos) < 0.01f) {
                pos = adjustedDestination;
                _shouldAnimate = false;
            }

            _contentRectTransform.anchoredPosition = _scrollViewDirection == ScrollViewDirection.Vertical ?
                new Vector2(0.0f, pos) :
                new Vector2(pos, 0.0f);
            scrollPositionChangedEvent?.Invoke(pos);

            UpdateVerticalScrollIndicator(Mathf.Abs(pos));
        }

        protected void SetContentSize(float contentSize) {

            bool needsScrolling = contentSize > scrollPageSize + 0.01f;
            if (_scrollViewDirection == ScrollViewDirection.Vertical) {
                _contentRectTransform.sizeDelta = new Vector2(_contentRectTransform.sizeDelta.x, contentSize);
            }
            else if (_scrollViewDirection == ScrollViewDirection.Horizontal) {
                _contentRectTransform.sizeDelta = new Vector2(contentSize, _contentRectTransform.sizeDelta.y);
            }

            // Vertical scroll indicator
            if (_verticalScrollIndicator != null) {
                _verticalScrollIndicator.gameObject.SetActive(needsScrolling);

                var rect = _viewport.rect;
                _verticalScrollIndicator.normalizedPageHeight = _scrollViewDirection == ScrollViewDirection.Vertical ? rect.height / contentSize : rect.width / contentSize;
            }

            if (_pageUpButton != null) {
                _pageUpButton.gameObject.SetActive(needsScrolling);
            }
            if (_pageDownButton != null) {
                _pageDownButton.gameObject.SetActive(needsScrolling);
            }
#if BS_TOURS
            _needsScrolling = needsScrolling;
#endif
            RefreshButtons();
        }

        public void UpdateContentSize() {

            switch (_scrollViewDirection) {
                case ScrollViewDirection.Vertical:
                    SetContentSize(_contentRectTransform.rect.height);
                    break;
                case ScrollViewDirection.Horizontal:
                    SetContentSize(_contentRectTransform.rect.width);
                    break;
            }

            ScrollTo(0.0f, animated: false);
        }

        public void ScrollToEnd(bool animated) {

            ScrollTo(contentSize - scrollPageSize, animated);
        }

        public void ScrollToWorldPosition(Vector3 worldPosition, float pageRelativePosition, bool animated) {

            var newDestinationPosY = WorldPositionToScrollViewPosition(worldPosition).y;
            newDestinationPosY -= pageRelativePosition * scrollPageSize;
            ScrollTo(newDestinationPosY, animated);
        }

        public void ScrollToWorldPositionIfOutsideArea(Vector3 worldPosition, float pageRelativePosition, float relativeBoundaryStart, float relativeBoundaryEnd, bool animated) {

            var newDestinationPos = WorldPositionToScrollViewPosition(worldPosition).y;

            var startBoundary = _destinationPos + relativeBoundaryStart * scrollPageSize;
            var endBoundary = _destinationPos + relativeBoundaryEnd * scrollPageSize;

            if (newDestinationPos > startBoundary && newDestinationPos < endBoundary) {
                return;
            }

            newDestinationPos -= pageRelativePosition * scrollPageSize;

            ScrollTo(newDestinationPos, animated);
        }

        public void ScrollToByPercentage(float value) {

            var contentDelta = contentSize - scrollPageSize;
            var newDestinationPos = contentDelta * value;
            ScrollTo(newDestinationPos, true);
        }

        public void ScrollTo(float destinationPos, bool animated) {

            SetDestinationPos(destinationPos);

            if (!animated) {
                switch (_scrollViewDirection) {
                    case ScrollViewDirection.Vertical:
                        _contentRectTransform.anchoredPosition = new Vector2(0.0f, _destinationPos);
                        scrollPositionChangedEvent?.Invoke(_destinationPos);
                        break;
                    case ScrollViewDirection.Horizontal:
                        _contentRectTransform.anchoredPosition = new Vector2(-_destinationPos, 0.0f);
                        scrollPositionChangedEvent?.Invoke(-_destinationPos);
                        break;
                }
            }

            RefreshButtons();

            _shouldAnimate = true;
            enabled = true;
        }

#if BS_TOURS
        public void SetJoystickScrollingEnabled(bool enabled) {

            _joystickScrollingEnabled = enabled;
        }
#endif

        private Vector2 WorldPositionToScrollViewPosition(Vector3 worldPosition) {

            return (Vector2)_viewport.transform.InverseTransformPoint(_contentRectTransform.position) - (Vector2)_viewport.transform.InverseTransformPoint(worldPosition);
        }

        private void SetDestinationPos(float value) {

            var contentDelta = contentSize - scrollPageSize;
            if (contentDelta < 0.0f) {
                _destinationPos = 0.0f;
                return;
            }

            _destinationPos = Mathf.Clamp(value, 0.0f, contentDelta);
        }

        private void UpdateVerticalScrollIndicator(float posY) {

            if (_verticalScrollIndicator != null) {
                _verticalScrollIndicator.progress = posY / (contentSize - scrollPageSize);
            }
        }

        private void PageUpButtonPressed() {

            float newDestinationPos = _destinationPos;

            switch (_scrollType) {
                case ScrollType.FocusItems: {
                    var threshold = _destinationPos + _scrollItemRelativeThresholdPosition * scrollPageSize;
                    newDestinationPos = _scrollFocusPositions.Where(pos => pos < threshold).DefaultIfEmpty(_destinationPos).Max();
                    newDestinationPos -= _pageStepNormalizedSize * scrollPageSize;
                    break;
                }
                case ScrollType.FixedCellSize: {
                    newDestinationPos -= _fixedCellSize * (Mathf.RoundToInt(scrollPageSize / _fixedCellSize) - 1);
                    newDestinationPos = Mathf.FloorToInt(newDestinationPos / _fixedCellSize) * _fixedCellSize;
                    break;
                }
                case ScrollType.PageSize: {
                    newDestinationPos -= _pageStepNormalizedSize * scrollPageSize;
                    break;
                }
            }

            SetDestinationPos(newDestinationPos);
            RefreshButtons();

            _shouldAnimate = true;
            enabled = true;
         }

         private void PageDownButtonPressed() {

             float newDestinationPos = _destinationPos;

             switch (_scrollType) {
                 case ScrollType.FocusItems: {
                     var threshold = _destinationPos + (1.0f - _scrollItemRelativeThresholdPosition) * scrollPageSize;
                     newDestinationPos = _scrollFocusPositions.Where(pos => pos > threshold).DefaultIfEmpty(_destinationPos + scrollPageSize).Min();
                     newDestinationPos -= (1.0f - _pageStepNormalizedSize) * scrollPageSize;
                     break;
                 }
                 case ScrollType.FixedCellSize: {
                     newDestinationPos += _fixedCellSize * (Mathf.RoundToInt(scrollPageSize / _fixedCellSize) - 1);
                     newDestinationPos = Mathf.CeilToInt(newDestinationPos / _fixedCellSize) * _fixedCellSize;
                     break;
                 }
                 case ScrollType.PageSize: {
                     newDestinationPos += _pageStepNormalizedSize * scrollPageSize;
                     break;
                 }
             }

             SetDestinationPos(newDestinationPos);
             RefreshButtons();

             _shouldAnimate = true;
             enabled = true;
         }

        private void RefreshButtons() {

            if (_pageUpButton != null) {
                _pageUpButton.interactable = (_destinationPos > 0.001f);
            }
            if (_pageDownButton != null) {
                _pageDownButton.interactable = (_destinationPos < contentSize - scrollPageSize - 0.001f);
            }
        }

        private void HandlePointerDidEnter(PointerEventData eventData) {

            _isHoveredByPointer = true;
            enabled = true;
#if BS_TOURS
            if (_needsScrolling) {
                didGainScrollFocus?.Invoke();
            }
#endif
        }

        private void HandlePointerDidExit(PointerEventData eventData) {

            _isHoveredByPointer = false;
#if BS_TOURS
            if (_needsScrolling) {
                didLoseScrollFocus?.Invoke();
            }
#endif
        }

        private void HandleJoystickWasNotCenteredThisFrame(Vector2 deltaPos) {

            if (_lastJoystickScrollDirection == ScrollDirection.None) {
                _joystickScrollStartTime = Time.time;
            }

            _lastJoystickScrollDirection = ResolveScrollDirection(deltaPos);

            float newDestinationPos = _destinationPos;
            switch (_scrollViewDirection) {
                case ScrollViewDirection.Vertical:
                    newDestinationPos -= deltaPos.y * Time.deltaTime * _joystickScrollSpeed;
                    break;
                case ScrollViewDirection.Horizontal:
                    newDestinationPos += deltaPos.x * Time.deltaTime * _joystickScrollSpeed;
                    break;
            }

            SetDestinationPos(newDestinationPos);
            RefreshButtons();
            _shouldAnimate = true;
        }

        private void HandleJoystickWasCenteredThisFrame() {

            float newDestinationPos = _destinationPos;
            var scrollTime = Time.time - _joystickScrollStartTime;

            switch (_scrollType) {
                case ScrollType.FocusItems: {
                    if (_lastJoystickScrollDirection == ScrollDirection.Up || _lastJoystickScrollDirection == ScrollDirection.Right) {
                        var threshold = _destinationPos + _scrollItemRelativeThresholdPosition * scrollPageSize;
                        newDestinationPos = _scrollFocusPositions.Where(pos => pos < threshold).DefaultIfEmpty(_destinationPos).Max();
                        if (scrollTime < _joystickQuickSnapMaxTime) {
                            newDestinationPos -= _pageStepNormalizedSize * scrollPageSize;
                        }
                    }
                    else if (_lastJoystickScrollDirection == ScrollDirection.Down || _lastJoystickScrollDirection == ScrollDirection.Left) {
                        var threshold = _destinationPos + (1.0f - _scrollItemRelativeThresholdPosition) * scrollPageSize;
                        newDestinationPos = _scrollFocusPositions.Where(pos => pos > threshold).DefaultIfEmpty(_destinationPos + scrollPageSize).Min();
                        if (scrollTime < _joystickQuickSnapMaxTime) {
                            newDestinationPos -= (1.0f - _pageStepNormalizedSize) * scrollPageSize;
                        }
                    }
                    break;
                }
                case ScrollType.FixedCellSize: {
                    var offset = _lastJoystickScrollDirection == ScrollDirection.Up || _lastJoystickScrollDirection == ScrollDirection.Left ? -1.0f : 0.0f;
                    newDestinationPos = Mathf.CeilToInt(newDestinationPos / _fixedCellSize + offset) * _fixedCellSize;
                    break;
                }
                case ScrollType.PageSize: {
                    var offset = 0.0f;
                    var scrollPageHeightAfterCut = 1f;
                    if (scrollTime < _joystickQuickSnapMaxTime) {
                        if (_lastJoystickScrollDirection == ScrollDirection.Up || _lastJoystickScrollDirection == ScrollDirection.Left) {
                            offset = -1.0f;
                        }
                        else if (_lastJoystickScrollDirection == ScrollDirection.Down || _lastJoystickScrollDirection == ScrollDirection.Right) {
                            offset = 1.0f;
                        }
                        scrollPageHeightAfterCut = scrollPageSize * _pageStepNormalizedSize;
                    }
                    newDestinationPos = Mathf.RoundToInt(newDestinationPos / scrollPageHeightAfterCut + offset) * scrollPageHeightAfterCut;
                    break;
                }
            }

            _lastJoystickScrollDirection = ScrollDirection.None;
            SetDestinationPos(newDestinationPos);
            RefreshButtons();
            _shouldAnimate = true;
        }

        private ScrollDirection ResolveScrollDirection(Vector2 deltaPos) {

            switch (_scrollViewDirection) {
                case ScrollViewDirection.Vertical:
                    if (deltaPos.y > 0.0f) {
                        return ScrollDirection.Up;
                    }

                    if (deltaPos.y < 0.0f) {
                        return ScrollDirection.Down;
                    }
                    break;
                case ScrollViewDirection.Horizontal:
                    if (deltaPos.x > 0.0f) {
                        return ScrollDirection.Right;
                    }

                    if (deltaPos.x < 0.0f) {
                        return ScrollDirection.Left;
                    }
                    break;
            }

            return ScrollDirection.None;
        }
    }
}
