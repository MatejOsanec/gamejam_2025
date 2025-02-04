using UnityEngine;

#if UNITY_EDITOR
using System.Collections.Generic;
#endif

public class ColorArrayLightWithIdsGroupEntry : MonoBehaviour {

    public LightGroupSO[] lightGroups => _lightGroups;

#pragma warning disable CS0414
    [SerializeField] ColorArrayLightWithIds _colorArrayLightWithIds = default;

    [Space]
    [SerializeField] LightGroupSO[] _lightGroups = default;
    [SerializeField] int[] _excludedLightIds = default;
#pragma warning restore CS0414


#if UNITY_EDITOR

    protected void OnValidate() {

        if (_colorArrayLightWithIds == null) {
            return;
        }

        var lightIntensitiesWithId = new List<ColorArrayLightWithIds.ColorArrayLightWithId>();
        var excludeLightIdsHashSet = new HashSet<int>(_excludedLightIds);

        foreach (var lightGroup in _lightGroups) {
            for (var lightId = lightGroup.startLightId; lightId < lightGroup.startLightId + lightGroup.numberOfElements; lightId++) {
                if (!excludeLightIdsHashSet.Contains(lightId)) {
                    lightIntensitiesWithId.Add(new ColorArrayLightWithIds.ColorArrayLightWithId(lightIntensitiesWithId.Count, lightId));
                }
            }
        }

        _colorArrayLightWithIds.SetColorArrayLightWithIdData(lightIntensitiesWithId.ToArray());
    }
#endif
}
