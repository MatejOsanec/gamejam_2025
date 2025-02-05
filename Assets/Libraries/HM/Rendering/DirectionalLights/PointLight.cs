using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Assertions;
#if UNITY_EDITOR
using UnityEditor;

#endif

[ExecuteInEditMode]
public class PointLight : MonoBehaviour {

    public const int kMaxLights = 1;

    public Color color;
    public float intensity;

    public static List<PointLight> lights => _lights;

    private static List<PointLight> _lights = new List<PointLight>(kMaxLights * 2);

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void NoDomainReloadInit() {

        _lights = new List<PointLight>(kMaxLights * 2);
    }
#endif

    protected void OnEnable() {

        Assert.IsFalse(_lights.Contains(this));

        _lights.Add(this);
    }

    protected void OnDisable() {

        _lights.Remove(this);
    }

#if UNITY_EDITOR
    protected void OnDrawGizmosSelected() {

        Gizmos.color = Color.yellow;

        var center = transform.position;
        Gizmos.DrawWireSphere(center, HandleUtility.GetHandleSize(center) * 0.25f);
    }
#endif
}
