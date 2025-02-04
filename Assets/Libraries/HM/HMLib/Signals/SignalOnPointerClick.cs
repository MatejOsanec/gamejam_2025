using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
///  Triggers a signal when pointer is clicked in any UI element.
/// </summary>
/// <remarks>
/// This class is used in a toggles, but if they are disabled, they will trigger the signal anyway.
/// Use SignalOnToggleChange instead if the toggle can be disabled
/// </remarks>
public class SignalOnPointerClick : MonoBehaviour, IPointerClickHandler {

    [SerializeField] [SignalSender] Signal _inputFieldClickedSignal = default;

    public void OnPointerClick(PointerEventData eventData) {
        _inputFieldClickedSignal.Raise();
    }
}
