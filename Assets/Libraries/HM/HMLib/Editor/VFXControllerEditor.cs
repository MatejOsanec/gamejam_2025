using System;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

[CustomEditor(typeof(VFXController))]
    public class VFXControllerEditor : Editor {
        
        private VFXController _myTarget;
        private bool _playMode;
        private double _previewStart;
        private double _lastEditorTime;
        
        private float _playbackTime;
        private bool _isSimulating;
        
        private bool _simulate;
        private float _time;
        private float _duration;
        private bool[] _originalAutoSeed;
        private ParticleSystem[] _particleSystems;
        
        private EditorCoroutine _runningSimulationRoutine;

        public override void OnInspectorGUI() {

            base.OnInspectorGUI();
            serializedObject.Update ();
            _myTarget = (VFXController)target;
            EditorGUILayout.Space(15.0f);
            
            // Preview Play
            if (GUILayout.Button("Preview VFX")) {
                    
                _playMode = true;
                _simulate = true;

                if (_runningSimulationRoutine != null) {
                    EditorCoroutineUtility.StopCoroutine(_runningSimulationRoutine);
                }
                _runningSimulationRoutine = EditorCoroutineUtility.StartCoroutine(SimulationCoroutine(), owner: this);
            }
            
            EditorGUILayout.Space(8.0f);
            _simulate = EditorGUILayout.Toggle("Simulate", _simulate);
            
            EditorGUI.indentLevel++;
            if (_simulate) {
                
                if (_duration > 0) { // slider option
                    _time = EditorGUILayout.Slider("Relative Time", _time / _duration, 0.0f, 1.0f) * _duration;
                }
                _time = EditorGUILayout.FloatField("Simulation Time", _time);
                _duration = EditorGUILayout.FloatField("Simulation Duration", _duration);
                
                EditorGUILayout.Space(8.0f);
            }
            EditorGUI.indentLevel--;

            if (_simulate) { // turning simulate ON
                if (!_isSimulating) {
                    AnimationMode.StartAnimationMode();
                    _isSimulating = true;
                    
                    _particleSystems = _myTarget.particleSystems;

                    if (_particleSystems != null && _particleSystems.Length > 0) {
                        
                        _originalAutoSeed = new bool[_particleSystems.Length];

                        for (int i = 0; i < _particleSystems.Length; i++) {
                            _originalAutoSeed[i] = _particleSystems[i].useAutoRandomSeed;
                            if (_originalAutoSeed[i]) {
                                _particleSystems[i].randomSeed = (uint)UnityEngine.Random.Range(0, 10000);
                                _particleSystems[i].useAutoRandomSeed = false;
                            }
                        }
                    }

                    if (_duration <= 0) {
                        CalculateDuration();
                    }
                }
            }
            else { 
                if (_isSimulating) {
                    EndSimulation();
                }
            }
            
            if (!EditorApplication.isPlaying && AnimationMode.InAnimationMode() && _isSimulating && !_playMode) {
                UpdateAnimation();
                UpdateParticles();
            }
            
        }

        private void UpdateAnimation() {
            AnimationMode.BeginSampling();
            AnimationMode.SampleAnimationClip(_myTarget.gameObject, _myTarget.animation.clip, _playMode ? _playbackTime : _time);
            AnimationMode.EndSampling();
            SceneView.RepaintAll();
        }

        private void UpdateParticles() {
            if (_particleSystems != null && _particleSystems.Length > 0) {
                foreach (var ps in _particleSystems) {

                    ps.Simulate(_playMode ? _playbackTime : _time, withChildren: false, restart: true, fixedTimeStep: true);
                }
            }
        }

        private IEnumerator SimulationCoroutine() {

            _playbackTime = 0.0f;
            _previewStart = EditorApplication.timeSinceStartup;

            if (_particleSystems != null && _particleSystems.Length > 0) {
                for (int i = 0; i < _particleSystems.Length; i++) {
                    if (_originalAutoSeed[i]) {
                        _particleSystems[i].randomSeed = (uint)UnityEngine.Random.Range(0, 10000);
                    }
                }
            }

            while (_playbackTime < _duration) {
            
                _playbackTime = (float) (EditorApplication.timeSinceStartup - _previewStart);
                UpdateAnimation();
                UpdateParticles();
                yield return null;
            }
            
            _playbackTime = _duration;
            UpdateAnimation();
            UpdateParticles();
            _playMode = false;
        
            _runningSimulationRoutine = null;
        }

        private void CalculateDuration() {
            float maxDuration = 0.0f;

            foreach (var particleSystem in _particleSystems) {
                var main = particleSystem.main;
                var approximateDuration = main.startLifetime.constantMax + main.duration;
                if (approximateDuration > maxDuration) {
                    maxDuration = approximateDuration;
                }
            }

            if (_myTarget.animation != null && _myTarget.animation.clip.length > maxDuration) {
                maxDuration = _myTarget.animation.clip.length;
            }
            
            _duration = maxDuration;
        }

        private void EndSimulation() {
            
            _time = 0.0f;

            UpdateParticles();
            AnimationMode.StopAnimationMode();

            if (_runningSimulationRoutine != null) {
                EditorCoroutineUtility.StopCoroutine(_runningSimulationRoutine);
                _runningSimulationRoutine = null;
            }

            if (_particleSystems != null && _particleSystems.Length > 0) {
                for (int i = 0; i < _particleSystems.Length; i++) {
                    _myTarget.particleSystems[i].useAutoRandomSeed = _originalAutoSeed[i];
                }
            }

            _isSimulating = false;
        }
        
        private void OnDisable()
        {
            EndSimulation();
        }

        // private void OnEnable() {
        //     _originalAutoSeed = new bool[_myTarget.particleSystems.Length];
        // }
    }
