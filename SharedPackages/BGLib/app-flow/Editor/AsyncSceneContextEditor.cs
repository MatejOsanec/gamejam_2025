using BGLib.AppFlow.Initialization;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(AsyncSceneContext))]
public class AsyncSceneContextEditor : MonoBehaviour
{

    protected string[] PropertyNames => new string[] { "" };

    protected string[] PropertyDisplayNames => new string[] { "" };

    protected string[] PropertyDescriptions => new string[] { "" };
}
