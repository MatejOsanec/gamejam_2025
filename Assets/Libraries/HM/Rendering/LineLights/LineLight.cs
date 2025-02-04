using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LineLight : MonoBehaviour {

    [SerializeField] Vector3 _p0 = default;
    [SerializeField] Vector3 _p1 = default;
    [SerializeField] Color _color = default;

    public Vector3 p0 => _p0;
    public Vector3 p1 => _p1;
    public Color color => _color;

    public static List<LineLight> lineLights => _lineLights;
    private static List<LineLight> _lineLights = new List<LineLight>(16);

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void NoDomainReloadInit() {

        _lineLights = new List<LineLight>(16);
    }
#endif

    protected void OnEnable() {

        _lineLights.Add(this);
    }

    protected void OnDisable() {

        _lineLights.Remove(this);
    }

    protected void OnDrawGizmos() {

        Vector3 tp0 = transform.TransformPoint(_p0);
        Vector3 tp1 = transform.TransformPoint(_p1);
        Gizmos.DrawLine(tp0, tp1);
    }
}
