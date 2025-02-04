using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SignalListener : MonoBehaviour {

	[SerializeField] Signal _signal = default;
	[SerializeField] UnityEvent _unityEvent = default;
	
	protected void OnEnable() {
		
		_signal.Subscribe(HandleEvent);
	}
	
	protected void OnDisable() {

		_signal.Unsubscribe(HandleEvent);
	}

	private void HandleEvent() {
		
		_unityEvent.Invoke();
	}
}
