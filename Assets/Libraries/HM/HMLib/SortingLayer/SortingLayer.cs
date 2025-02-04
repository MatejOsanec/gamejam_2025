using UnityEngine;
using System.Collections;

public class SortingLayer : MonoBehaviour {

	[SerializeField] Renderer _renderer = default;

#if UNITY_EDITOR
	public new Renderer renderer {
		get { return _renderer; }
	}
#else
    public Renderer renderer {
		get { return _renderer; }
	}
#endif


	private void Reset() {
		_renderer = GetComponent<Renderer>();
	}
}
