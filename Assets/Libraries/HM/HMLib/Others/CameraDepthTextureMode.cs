using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDepthTextureMode : MonoBehaviour {

    [SerializeField] DepthTextureMode _depthTextureMode = default;

    protected void Awake() {        
        GetComponent<Camera>().depthTextureMode = _depthTextureMode;
	}	
}