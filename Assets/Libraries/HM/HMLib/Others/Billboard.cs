using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

[ExecuteInEditMode]
public class Billboard : MonoBehaviour {
	    
    [SerializeField] RotationMode _rotationMode = RotationMode.AllAxis;
    [SerializeField] bool _flipDirection = default;

    public enum RotationMode {
        AllAxis,
        XAxis,
        YAxis,
        ZAxis,
    }

	private Transform _transform;

	protected void Awake() {

		_transform = transform;
	}

    private void OnWillRenderObject() {

        Vector3 targetPos = Camera.current.transform.position;
        Vector3 thisPos = _transform.position;

        switch (_rotationMode) {

            case RotationMode.XAxis: 
                targetPos.x = thisPos.x;
                break;
                    
            case RotationMode.YAxis: 
                targetPos.y = thisPos.y;
                break;
            
            case RotationMode.ZAxis: 
                targetPos.z = thisPos.z;
                break;                
        }

        if (_flipDirection) {
            _transform.LookAt(2.0f * thisPos - targetPos);
        }
        else {
            _transform.LookAt(targetPos);
        }
	}
}