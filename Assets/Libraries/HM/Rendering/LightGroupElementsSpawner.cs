using UnityEngine;

[ExecuteAlways][RequireComponent(typeof(LightGroup))]
public class LightGroupElementsSpawner : MonoBehaviour {

    [SerializeField] internal GameObject _lightPrefab = default;
    [SerializeField] internal bool _useAlternatePrefab = false;
    [SerializeField][DrawIf("_useAlternatePrefab", true)][NullAllowedIf("_useAlternatePrefab", equalsTo: false)] internal GameObject _alternateLightPrefab = default;
}
