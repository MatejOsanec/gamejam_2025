using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

public class EnableOnVisible : MonoBehaviour {

	public event System.Action<bool> VisibilityChangedEvent;

	public Behaviour[] _components;
	
	protected void Awake() {
		
		 Assert.IsNotNull(GetComponent<Renderer>());
		for (int i = 0; i < _components.Length; i++) {
			_components[i].enabled = false;
		}
	}
	
	protected void OnBecameVisible() {
		
		for (int i = 0; i < _components.Length; i++) {
			_components[i].enabled = true;
		}

		if (VisibilityChangedEvent != null) {
			VisibilityChangedEvent(true);
		}
	}
	
	protected void OnBecameInvisible() {
		
		for (int i = 0; i < _components.Length; i++) {
			_components[i].enabled = false;
		}

		if (VisibilityChangedEvent != null) {
			VisibilityChangedEvent(false);
		}
	}	
}
