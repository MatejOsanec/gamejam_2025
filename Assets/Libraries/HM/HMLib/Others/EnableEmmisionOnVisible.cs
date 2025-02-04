using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

public class EnableEmmisionOnVisible : MonoBehaviour {

    [SerializeField] ParticleSystem[] _particleSystems = default;

    private ParticleSystem.EmissionModule[] _emmisionModules;

	protected void Awake() {
		
		Assert.IsNotNull(GetComponent<Renderer>());

        	_emmisionModules = new ParticleSystem.EmissionModule[_particleSystems.Length];

		for (int i = 0; i < _particleSystems.Length; i++) {
            _emmisionModules[i] = _particleSystems[i].emission;
            _emmisionModules[i].enabled = false;
		}
	}
	
	protected void OnBecameVisible() {
		
		for (int i = 0; i < _particleSystems.Length; i++) {
            _emmisionModules[i].enabled = true;
		}
	}
	
	protected void OnBecameInvisible() {
		
		for (int i = 0; i < _particleSystems.Length; i++) {
            _emmisionModules[i].enabled = false;
		}
	}
}
