using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Assertions;
#if UNITY_EDITOR
using UnityEditor;

#endif

[ExecuteInEditMode]
public class DirectionalLight : MonoBehaviour {

    public const int kMaxLights = 5;
    [ColorUsage(showAlpha: false)] public Color color;
    public float intensity;
    public float radius = 50.0f;

    public static List<DirectionalLight> lights => _lights;

    private static List<DirectionalLight> _lights = new List<DirectionalLight>(kMaxLights * 2);
    private static DirectionalLight _mainLight;

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void NoDomainReloadInit() {

        _lights = new List<DirectionalLight>(kMaxLights * 2);
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

        int steps = 12;
        var forward = transform.forward;
        var center = transform.position;
        Vector3 prevPoint = new Vector3(0.0f, 0.0f, 0.0f);

        float size = HandleUtility.GetHandleSize(center) * 0.25f;

        for (int i = 0; i < steps; i++) {
            var a = (Mathf.PI * 2.0f * i) / (steps - 1.0f);
            var point = new Vector3(Mathf.Sin(a) * size, Mathf.Cos(a) * size, 0.0f);
            point = transform.TransformPoint(point);
            Gizmos.DrawLine(point, point + forward * size * 4.0f);

            if (i > 0) {
                Gizmos.DrawLine(prevPoint, point);
            }

            prevPoint = point;
        }
        
        Gizmos.DrawWireSphere(center, radius);
    }
#endif
}
