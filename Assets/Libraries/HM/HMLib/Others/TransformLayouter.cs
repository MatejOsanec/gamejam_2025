using System;
using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

// Used to position many objects in a regular fashion
public class TransformLayouter : MonoBehaviour {

#if UNITY_EDITOR
    
    [SerializeField] bool _centered = false;
    [SerializeField] bool _localSpace = false;
    [SerializeField] Vector3 _direction;
    [SerializeField] float _perIterationMultiplier = 1.0f;
    [SerializeField] bool _update = false;
    
    [Space(12)]
    [SerializeField] Vector3 _startPosition;
    [SerializeField, NullAllowed] Transform _startFromTransform;
    
    [Space(12)]
    [SerializeField] Transform[] _arrayTransforms;

    private Vector3 _step;
    private Vector3 _localSpaceOffset;

    private void OnValidate() {
        
        if (_startFromTransform != null) {
            _startPosition = _localSpace ? _startFromTransform.localPosition : _startFromTransform.position;
            _startFromTransform = null;
        }
        
        if (!_update || _arrayTransforms.Length < 1) {
            return;
        }
        
        _step = _direction;
        bool evenCount = false;
        
        if (_centered) {
            float spaces = _arrayTransforms.Length - 1.0f;
            evenCount = _arrayTransforms.Length % 2 == 0;
            _step /= spaces;
        }

        float currentMultiplier = 1.0f;
        
        for (int i = 0; i < _arrayTransforms.Length; i++) {
            
            _localSpaceOffset = _localSpace ? _arrayTransforms[i].transform.parent.position : Vector3.zero;
            Vector3 targetPosition = Vector3.zero;
            
            if (_centered) {
                
                if (evenCount) {
                    if (i < 2.0f) {
                        targetPosition = _localSpaceOffset + _startPosition + _step * 0.5f * (Mathf.Floor(i * 0.5f) + 1.0f) * currentMultiplier;
                    } else {
                        targetPosition = _localSpaceOffset + _startPosition + _step * (Mathf.Floor(i * 0.5f) + 1.0f) * currentMultiplier - _step * 0.5f * Mathf.Sign(currentMultiplier);
                    }
                }
                else {
                    targetPosition = _localSpaceOffset + _startPosition + _step * Mathf.Ceil(i * 0.5f) * currentMultiplier;
                }
                
                currentMultiplier *= -_perIterationMultiplier;
            }
            
            else {
                targetPosition = _localSpaceOffset + _startPosition + _step * i * currentMultiplier;
                currentMultiplier *= _perIterationMultiplier;
            }
            
            if (Vector3.SqrMagnitude(_arrayTransforms[i].transform.position - targetPosition) >= 0.001f) {
                _arrayTransforms[i].transform.position = targetPosition;
                EditorUtility.SetDirty(_arrayTransforms[i].transform);
            }
        }
    }
#endif
}

