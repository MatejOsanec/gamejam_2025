using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[ExecuteAlways]
public abstract class LightWithIds : MonoBehaviour {

    [Inject] LightWithIdManager _lightManager = default;

    [Serializable]
    public abstract class LightWithId : ILightWithId {

        [SerializeField] int _lightId = default;

        public int lightId => _lightId;
        public Color color => _color;
        public bool isRegistered => _isRegistered;

        private Color _color;
        private bool _isRegistered = false;
        private LightWithIds _parentLightWithIds;

        public void __SetIsRegistered() => _isRegistered = true;
        public void __SetIsUnRegistered() => _isRegistered = false;

        protected LightWithId() {}

        protected LightWithId(int lightId) {

            _lightId = lightId;
        }

        public void __SetParentLightWithIds(LightWithIds parentLightWithIds) {

            _parentLightWithIds = parentLightWithIds;
        }

        public virtual void ColorWasSet(Color newColor) {

            _color = newColor;
            _parentLightWithIds.MarkChildrenColorAsSet();
        }

#if UNITY_EDITOR
        public void __SetColor(Color newColor) {

            _color = newColor;
        }
#endif
    }

    public IEnumerable<LightWithId> lightWithIds => _lightWithIds;

    private IEnumerable<LightWithId> _lightWithIds;

    private bool _isRegistered;
    private bool _childrenColorWasSet;

    protected virtual void Awake() {

        SetNewLightsWithIds(GetLightWithIds());
    }

    protected void Start() {

        RegisterForColorChanges();
    }

    protected virtual void OnEnable() {

        RegisterForColorChanges();
    }

    public void MarkChildrenColorAsSet() {

        _childrenColorWasSet = true;
    }

    protected void SetNewLightsWithIds(IEnumerable<LightWithId> lightsWithIds) {

        UnregisterFromColorChanges();
        _lightWithIds = lightsWithIds;
        RegisterForColorChanges();
    }
    
    protected abstract IEnumerable<LightWithId> GetLightWithIds();

    private void RegisterForColorChanges() {

        if (_isRegistered || !enabled || _lightWithIds == null) {
            return;
        }

        if (_lightManager == null) {

#if UNITY_EDITOR
            if (!Application.isPlaying) {
                _lightManager = FindObjectOfType<LightWithIdManager>();
                if (_lightManager == null) {
                    return;
                }
            }
            else {
                return;
            }
#else
            return;
#endif
        }

        _lightManager.didChangeSomeColorsThisFrameEvent += HandleLightManagerDidChangeSomeColorsThisFrame;

        foreach (var item in _lightWithIds) {
            item.__SetParentLightWithIds(this);
            _lightManager.RegisterLight(item);
        }

        _isRegistered = true;
    }

    private void UnregisterFromColorChanges() {

        if (!_isRegistered) {
            return;
        }

        if (_lightManager == null) {

#if UNITY_EDITOR
            if (!Application.isPlaying) {
                _lightManager = FindObjectOfType<LightWithIdManager>();
                if (_lightManager == null) {
                    return;
                }
            }
            else {
                return;
            }
#else
            return;
#endif
        }

        _lightManager.didChangeSomeColorsThisFrameEvent -= HandleLightManagerDidChangeSomeColorsThisFrame;

#if UNITY_EDITOR
        // Application.isPlaying is here to surface any errors in Editor but to fix exception (_lightWithIds is null) during Edit mode
        if (Application.isPlaying || _lightWithIds != null) {
#endif
            foreach (var item in _lightWithIds) {
                _lightManager.UnregisterLight(item);
            }
#if UNITY_EDITOR
        }
#endif

        _isRegistered = false;
    }

    protected void OnDisable() {

        UnregisterFromColorChanges();
    }

    private void HandleLightManagerDidChangeSomeColorsThisFrame() {

        if (!_childrenColorWasSet) {
            return;
        }
        _childrenColorWasSet = false;
        ProcessNewColorData();
    }

    protected abstract void ProcessNewColorData();
}
