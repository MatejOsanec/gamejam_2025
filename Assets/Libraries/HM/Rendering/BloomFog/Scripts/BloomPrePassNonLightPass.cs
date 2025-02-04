using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public abstract class BloomPrePassNonLightPass : MonoBehaviour {

    [SerializeField] ExecutionTimeType _executionTimeType = default;

    public enum ExecutionTimeType {
        None,
        BeforeBlur,
        AfterBlur
    }

    public ExecutionTimeType executionTimeType => _executionTimeType;

    public static List<BloomPrePassNonLightPass> bloomPrePassAfterBlurList => _bloomPrePassAfterBlurList;
    public static List<BloomPrePassNonLightPass> bloomPrePassBeforeBlurList => _bloomPrePassBeforeBlurList;

    private static List<BloomPrePassNonLightPass> _bloomPrePassAfterBlurList = new List<BloomPrePassNonLightPass>();
    private static List<BloomPrePassNonLightPass> _bloomPrePassBeforeBlurList = new List<BloomPrePassNonLightPass>();

    private ExecutionTimeType _registeredExecutionTimeType;

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void NoDomainReloadInit() {

        _bloomPrePassAfterBlurList = new List<BloomPrePassNonLightPass>();
        _bloomPrePassBeforeBlurList = new List<BloomPrePassNonLightPass>();
    }
#endif

    protected virtual void OnEnable() {

        Register();
    }

    protected virtual void OnDisable() {

        Unregister();
    }

    protected void Register() {

        if (_registeredExecutionTimeType == _executionTimeType) {
            return;
        }

        if (_registeredExecutionTimeType != ExecutionTimeType.None) {
            Unregister();
        }

        switch (_executionTimeType) {
            case ExecutionTimeType.BeforeBlur:
                _bloomPrePassBeforeBlurList.Add(this);
                break;
            case ExecutionTimeType.AfterBlur:
                _bloomPrePassAfterBlurList.Add(this);
                break;
        }

        _registeredExecutionTimeType = _executionTimeType;
    }

    protected void Unregister() {

        switch (_registeredExecutionTimeType) {
            case ExecutionTimeType.BeforeBlur:
                _bloomPrePassBeforeBlurList.Remove(this);
                break;
            case ExecutionTimeType.AfterBlur:
                _bloomPrePassAfterBlurList.Remove(this);
                break;
        }

        _registeredExecutionTimeType = ExecutionTimeType.None;
    }

    protected virtual void OnValidate() {

        if (isActiveAndEnabled) {
            Register();
        }
        else {
            Unregister();
        }
    }

    public abstract void Render(RenderTexture dest, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix);
}
