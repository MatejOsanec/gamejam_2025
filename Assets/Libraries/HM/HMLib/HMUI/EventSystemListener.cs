using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HMUI {

    [RequireComponent(typeof(Graphic))]
    public class EventSystemListener : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        public event Action<PointerEventData> pointerDidEnterEvent;
        public event Action<PointerEventData> pointerDidExitEvent;

        public void OnPointerEnter(PointerEventData eventData) {

            pointerDidEnterEvent?.Invoke(eventData);
        }

        public void OnPointerExit(PointerEventData eventData) {

            pointerDidExitEvent?.Invoke(eventData);
        }
    }
}
