using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class NoVRInputModule : IVRInputModule {

#pragma warning disable 0067
    public event Action<GameObject> onProcessMousePressEvent;
    public event Action<PointerEventData> pointerDidClickEvent;
#pragma warning restore 0067
}
