using System;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IVRInputModule {

    event Action<GameObject> onProcessMousePressEvent;
    event Action<PointerEventData> pointerDidClickEvent;
}
