using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Zenject;
using HMUI;
using Libraries.HM.HMLib.VR;

namespace VRUIControls {

    public class VRInputModule : UnityEngine.EventSystems.BaseInputModule, IVRInputModule {

        [SerializeField] VRPointer _vrPointer = default;
        [SerializeField] HapticPresetSO _rumblePreset = default;

        [Inject] readonly HapticFeedbackManager _hapticFeedbackManager = default;

        public bool useMouseForPressInput { get; set; }

        private const int kMouseLeftId = -1;
        private const float kMinPressValue = 0.9f;

        public event Action<GameObject> onProcessMousePressEvent;
        public event Action<PointerEventData> pointerDidClickEvent;

        private readonly Dictionary<int, PointerEventData> _pointerData = new Dictionary<int, PointerEventData>();
        private readonly List<Component> _componentList = new List<Component>(20);
        private readonly MouseState _mouseState = new MouseState();

        
        private static readonly Comparison<RaycastResult> _raycastComparer = RaycastComparer;

        protected override void OnDisable() {

            base.OnDisable();
            ClearSelection();
        }

        protected bool GetPointerData(int id, out PointerEventData data, bool create) {

            if (!_pointerData.TryGetValue(id, out data) && create) {
                data = new PointerEventData(eventSystem) {
                    pointerId = id,
                };
                _pointerData.Add(id, data);
                return true;
            }
            return false;
        }

        protected virtual MouseState GetMousePointerEventData(int id) {

            // Populate the left button...
            bool created = GetPointerData(kMouseLeftId, out var leftData, true);

            leftData.Reset();
            leftData.button = PointerEventData.InputButton.Left;

            var vrController = _vrPointer.lastSelectedVrController;

            if (vrController.active) {
                leftData.pointerCurrentRaycast = new RaycastResult() {
                    worldPosition = vrController.viewAnchorTransform.position,
                    worldNormal = vrController.viewAnchorTransform.forward
                };
                var scroll = vrController.thumbstick * VRPointer.kScrollMultiplier;
                scroll.x *= -1; // x is inverted WTF
                leftData.scrollDelta = scroll;
            }

            eventSystem.RaycastAll(leftData, m_RaycastResultCache);
            m_RaycastResultCache.Sort(_raycastComparer);
            var raycast = FindFirstRaycast(m_RaycastResultCache);
            leftData.pointerCurrentRaycast = raycast;
            m_RaycastResultCache.Clear();

            var pos = raycast.screenPosition;
            if (created) {
                leftData.delta = new Vector2(0, 0);
            }
            else {
                leftData.delta = pos - leftData.position;
            }
            leftData.position = pos;

            PointerEventData.FramePressState framePressedState = PointerEventData.FramePressState.NotChanged;
            if (vrController.active) {

                float pressedValue = vrController.triggerValue;

                ButtonState buttonState = _mouseState.GetButtonState(PointerEventData.InputButton.Left);
                if (buttonState.pressedValue < kMinPressValue && pressedValue >= kMinPressValue) {
                    framePressedState = PointerEventData.FramePressState.Pressed;
                }
                else if (buttonState.pressedValue >= kMinPressValue && pressedValue < kMinPressValue) {
                    framePressedState = PointerEventData.FramePressState.Released;
                }
                buttonState.pressedValue = pressedValue;
            }

            _mouseState.SetButtonState(PointerEventData.InputButton.Left, framePressedState, leftData);

            return _mouseState;
        }

        protected PointerEventData GetLastPointerEventData(int id) {

            PointerEventData data;
            GetPointerData(id, out data, false);
            return data;
        }

        private bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold) {

            if (!useDragThreshold) {
                return true;
            }

            return (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
        }

        protected virtual void ProcessMove(PointerEventData pointerEvent) {

            var targetGO = pointerEvent.pointerCurrentRaycast.gameObject;
            HandlePointerExitAndEnter(pointerEvent, targetGO);

            for (int i = 0; i < pointerEvent.hovered.Count; ++i) {
                ExecuteEvents.Execute(pointerEvent.hovered[i], pointerEvent, ExecuteEvents.pointerMoveHandler);
            }
        }

        protected virtual void ProcessDrag(PointerEventData pointerEvent) {

            bool moving = pointerEvent.IsPointerMoving();

            if (moving && pointerEvent.pointerDrag != null && !pointerEvent.dragging && ShouldStartDrag(pointerEvent.pressPosition, pointerEvent.position, eventSystem.pixelDragThreshold, pointerEvent.useDragThreshold)) {
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
                pointerEvent.dragging = true;
            }

            // Drag notification
            if (pointerEvent.dragging && moving && pointerEvent.pointerDrag != null) {
                // Before doing drag we should cancel any pointer down state
                // And clear selection!
                if (pointerEvent.pointerPress != pointerEvent.pointerDrag) {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                    pointerEvent.eligibleForClick = false;
                    pointerEvent.pointerPress = null;
                    pointerEvent.rawPointerPress = null;
                }
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
            }
        }

        public override bool IsPointerOverGameObject(int pointerId) {

            var lastPointer = GetLastPointerEventData(pointerId);
            if (lastPointer != null) {
                return lastPointer.pointerEnter != null;
            }

            return false;
        }

        protected void ClearSelection() {

            var baseEventData = GetBaseEventData();

            foreach (var pointer in _pointerData.Values) {
                // clear all selection
                HandlePointerExitAndEnter(pointer, null);
            }

            _pointerData.Clear();
            if (eventSystem != null) {
                eventSystem.SetSelectedGameObject(null, baseEventData);
            }
        }

        public override string ToString() {

            var sb = new StringBuilder("<b>Pointer Input Module of type: </b>" + GetType());
            sb.AppendLine();
            foreach (var pointer in _pointerData) {
                if (pointer.Value == null)
                    continue;
                sb.AppendLine("<B>Pointer:</b> " + pointer.Key);
                sb.AppendLine(pointer.Value.ToString());
            }
            return sb.ToString();
        }

        protected void DeselectIfSelectionChanged(GameObject currentOverGo, BaseEventData pointerEvent) {

            // Selection tracking
            var selectHandlerGO = ExecuteEvents.GetEventHandler<ISelectHandler>(currentOverGo);
            // if we have clicked something new, deselect the old thing
            // leave 'selection handling' up to the press event though.
            if (selectHandlerGO == eventSystem.currentSelectedGameObject) {
                return;
            }

            eventSystem.SetSelectedGameObject(selectHandlerGO, pointerEvent);
        }

        public override void Process() {

            var mouseData = GetMousePointerEventData(0);
            var leftButtonData = mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;

            // Process the first mouse button fully
            if (enabled) {
                ProcessMousePress(leftButtonData);
                ProcessMove(leftButtonData.buttonData);
                ProcessDrag(leftButtonData.buttonData);

                if (!Mathf.Approximately(leftButtonData.buttonData.scrollDelta.sqrMagnitude, 0.0f)) {
                    var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(leftButtonData.buttonData.pointerCurrentRaycast.gameObject);
                    ExecuteEvents.ExecuteHierarchy(scrollHandler, leftButtonData.buttonData, ExecuteEvents.scrollHandler);
                }
            }

            _vrPointer.Process(leftButtonData.buttonData);
        }

        protected bool SendUpdateEventToSelectedObject() {

            if (eventSystem.currentSelectedGameObject == null)
                return false;

            var data = GetBaseEventData();
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
            return data.used;
        }

        protected void ProcessMousePress(MouseButtonEventData data) {

            var pointerEvent = data.buttonData;
            var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;
            bool pressImmediately = false;

            // PointerDown notification
            if (data.PressedThisFrame()) {
                pointerEvent.eligibleForClick = true;
                pointerEvent.delta = Vector2.zero;
                pointerEvent.dragging = false;
                pointerEvent.useDragThreshold = true;
                pointerEvent.pressPosition = pointerEvent.position;
                pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

                DeselectIfSelectionChanged(currentOverGo, pointerEvent);

                // search for the control that will receive the press
                // if we can't find a press handler set the press
                // handler to be what would receive a click.
                var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);

                // didn't find a press handler... search for a click handler
                if (newPressed == null) {
                    newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
                }

                if (currentOverGo != null && newPressed != null) {
                    onProcessMousePressEvent?.Invoke(currentOverGo);
                }

                float time = Time.unscaledTime;

                if (newPressed == pointerEvent.lastPress) {
                    var diffTime = time - pointerEvent.clickTime;
                    if (diffTime < 0.3f)
                        ++pointerEvent.clickCount;
                    else
                        pointerEvent.clickCount = 1;

                    pointerEvent.clickTime = time;
                }
                else {
                    pointerEvent.clickCount = 1;
                }

                pointerEvent.pointerPress = newPressed;
                pointerEvent.rawPointerPress = currentOverGo;

                pointerEvent.clickTime = time;

                // Save the drag handler as well
                pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

                if (pointerEvent.pointerDrag != null)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);

                pointerEvent.eligibleForClick = true;
                // Clicks are executed on pointer down.
                pressImmediately = pointerEvent.pointerPress != null && pointerEvent.pointerPress.GetComponent<IPointerClickHandler>() != null;
            }

            // PointerUp notification
            if (data.ReleasedThisFrame() || pressImmediately) {
                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                // see if we mouse up on the same element that we clicked on...
                var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                // PointerClick and Drop events
                if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick) {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
                    pointerDidClickEvent?.Invoke(pointerEvent);
                }
                else if (pointerEvent.pointerDrag != null && pointerEvent.dragging) {
                    ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
                }

                // Deselect on release
                // Maybe it will break something but we don't care about "selection" now
                eventSystem.SetSelectedGameObject(null, null);

                pointerEvent.eligibleForClick = false;
                pointerEvent.pointerPress = null;
                pointerEvent.rawPointerPress = null;

                if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

                pointerEvent.dragging = false;
                pointerEvent.pointerDrag = null;

                // redo pointer enter / exit to refresh state
                // so that if we moused over something that ignored it before
                // due to having pressed on something else
                // it now gets it.
                if (currentOverGo != pointerEvent.pointerEnter) {
                    HandlePointerExitAndEnter(pointerEvent, null);
                    HandlePointerExitAndEnter(pointerEvent, currentOverGo);
                }
            }
        }

        // walk up the tree till a common root between the last entered and the current entered is found
        // send exit events up to (but not including) the common root. Then send enter events up to
        // (but not including the common root).
        new protected void HandlePointerExitAndEnter(PointerEventData currentPointerData, GameObject newEnterTarget) {

            // if we have no target / pointerEnter has been deleted
            // just send exit events to anything we are tracking
            // then exit
            if (newEnterTarget == null || currentPointerData.pointerEnter == null) {
                for (int i = 0; i < currentPointerData.hovered.Count; ++i) {
                    ExecuteEvents.Execute(currentPointerData.hovered[i], currentPointerData, ExecuteEvents.pointerExitHandler);
                }

                currentPointerData.hovered.Clear();

                if (newEnterTarget == null) {
                    currentPointerData.pointerEnter = newEnterTarget;
                    return;
                }
            }

            // if we have not changed hover target
            if (currentPointerData.pointerEnter == newEnterTarget && newEnterTarget) {
                return;
            }

            var commonRoot = FindCommonRoot(currentPointerData.pointerEnter, newEnterTarget);

            // and we already an entered object from last time
            if (currentPointerData.pointerEnter != null) {
                // send exit handler call to all elements in the chain
                // until we reach the new target, or null!
                var t = currentPointerData.pointerEnter.transform;

                while (t != null) {
                    // if we reach the common root break out!
                    if (commonRoot != null && commonRoot.transform == t)
                        break;

                    ExecuteEvents.Execute(t.gameObject, currentPointerData, ExecuteEvents.pointerExitHandler);
                    currentPointerData.hovered.Remove(t.gameObject);
                    t = t.parent;
                }
            }

            // Don't issue enter calls if user interaction is disables.
            if (!enabled) {
                return;
            }

            // now issue the enter call up to but not including the common root
            currentPointerData.pointerEnter = newEnterTarget;
            if (newEnterTarget != null) {
                var t = newEnterTarget.transform;
                bool rumbled = false;
                var blocked = false;

                //iterate trough parents up to commonRoot
                while (t != null) {

                    _componentList.Clear();
                    t.gameObject.GetComponents(_componentList);

                    for (int i = 0; i < _componentList.Count; i++) {
                        var selectable = _componentList[i] as Selectable;
                        var interactable = _componentList[i] as HMUI.Interactable;
                        var canvasGroup = _componentList[i] as CanvasGroup;

                        blocked = blocked
                                  || (selectable != null && !selectable.interactable)
                                  || (interactable != null && !interactable.interactable)
                                  || (canvasGroup != null && !canvasGroup.interactable);

                        rumbled = rumbled
                                  || (selectable != null && selectable.isActiveAndEnabled && selectable.interactable)
                                  || (interactable != null && interactable.isActiveAndEnabled && interactable.interactable);
                    }

                    //do not check game objects beyond common root
                    if (t.gameObject == commonRoot) {
                        break;
                    }

                    ExecuteEvents.Execute(t.gameObject, currentPointerData, ExecuteEvents.pointerEnterHandler);
                    currentPointerData.hovered.Add(t.gameObject);

                    //check next
                    t = t.parent;
                }

                if (!blocked && rumbled) {
                    _hapticFeedbackManager.PlayHapticFeedback(_vrPointer.lastSelectedVrController.node, _rumblePreset);
                }
            }
        }

        // Taken and modified from Unity EventSystem class
        private static int RaycastComparer(RaycastResult lhs, RaycastResult rhs)
        {
            if (lhs.module != rhs.module)
            {
                var lhsEventCamera = lhs.module.eventCamera;
                var rhsEventCamera = rhs.module.eventCamera;
                if (lhsEventCamera != null && rhsEventCamera != null && lhsEventCamera.depth != rhsEventCamera.depth)
                {
                    // need to reverse the standard compareTo
                    if (lhsEventCamera.depth < rhsEventCamera.depth)
                        return 1;
                    if (lhsEventCamera.depth == rhsEventCamera.depth)
                        return 0;

                    return -1;
                }

                if (lhs.module.sortOrderPriority != rhs.module.sortOrderPriority)
                    return rhs.module.sortOrderPriority.CompareTo(lhs.module.sortOrderPriority);

                if (lhs.module.renderOrderPriority != rhs.module.renderOrderPriority)
                    return rhs.module.renderOrderPriority.CompareTo(lhs.module.renderOrderPriority);
            }

            if (!Mathf.Approximately(lhs.distance, rhs.distance)) {
                return lhs.distance.CompareTo(rhs.distance);
            }

            if (lhs.sortingLayer != rhs.sortingLayer)
            {
                // Uses the layer value to properly compare the relative order of the layers.
                var rid = UnityEngine.SortingLayer.GetLayerValueFromID(rhs.sortingLayer);
                var lid = UnityEngine.SortingLayer.GetLayerValueFromID(lhs.sortingLayer);
                return rid.CompareTo(lid);
            }

            if (lhs.sortingOrder != rhs.sortingOrder) {
                return rhs.sortingOrder.CompareTo(lhs.sortingOrder);
            }

            // comparing depth only makes sense if the two raycast results have the same root canvas (case 912396)
            if (lhs.depth != rhs.depth && lhs.module.rootRaycaster == rhs.module.rootRaycaster) {
                return rhs.depth.CompareTo(lhs.depth);
            }

            return lhs.index.CompareTo(rhs.index);
        }
    }
}
