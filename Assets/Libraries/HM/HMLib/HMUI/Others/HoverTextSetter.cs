using UnityEngine;
using UnityEngine.EventSystems;

namespace HMUI {

public class HoverTextSetter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] HoverTextController _hoverTextController = default;
    [SerializeField] string _text = default;

    public string text { get => _text; set => _text = value; }

    public void OnPointerEnter(PointerEventData eventData) {

        _hoverTextController.ShowText(_text);
    }

    public void OnPointerExit(PointerEventData eventData) {

        _hoverTextController.HideText();
    }

    protected void OnDisable() {

        _hoverTextController.HideText();
    }
}

}
