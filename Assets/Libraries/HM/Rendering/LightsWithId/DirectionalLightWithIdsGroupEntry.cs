using System;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalLightWithIdsGroupEntry : MonoBehaviour {

#pragma warning disable CS0414
    [SerializeField] DirectionalLightWithIds _directionalLightWithIds = default;
    [Space]
    [SerializeField] List<GroupLightData> _groupLightData = default;
    [SerializeField] List<LightIntensitiesWithId> _individualLightData = default;
    [SerializeField] int[] _excludedLightIds = default;
    [Space]
    [SerializeField] ColorMixAndWeightingApproach _groupLightsWeighting = ColorMixAndWeightingApproach.Maximum;
#pragma warning restore CS0414

    public List<GroupLightData> groupLightData => _groupLightData;
    public List<LightIntensitiesWithId> individualLightData => _individualLightData;
    public ColorMixAndWeightingApproach groupLightsWeighting => _groupLightsWeighting;

    [Serializable]
    public class GroupLightData {

        [SerializeField] LightGroupSO _lightGroup = default;
        [SerializeField] float _groupIntensity = default;

        public LightGroupSO lightGroup => _lightGroup;
        public float groupIntensity => _groupIntensity;

        public GroupLightData(LightGroupSO lightGroup) {

            _lightGroup = lightGroup;
            _groupIntensity = 1;
        }
    }

    [Serializable]
    public class LightIntensitiesWithId {

        [SerializeField] int _lightId;
        [SerializeField] float _intensity = default;

        public int lightId => _lightId;
        public float intensity => _intensity;

        public LightIntensitiesWithId(int lightId, float lightIntensity) {

            _lightId = lightId;
            _intensity = lightIntensity;
        }
    }

#if UNITY_EDITOR
    private void OnValidate() {

        if (_directionalLightWithIds == null) {
            return;
        }

        var lightIntensitiesWithId = new List<RuntimeLightWithIds.LightIntensitiesWithId>();
        var excludeLightIdsHashSet = new HashSet<int>(_excludedLightIds);
        var individualLightIdsHashSet = new HashSet<int>();
        var omittedFromGroupCountIdsHashSet = new HashSet<int>();

        var groupLightMultiplier = 1.0f;

        // Individual lights
        foreach (var lightData in _individualLightData) {
            if (!excludeLightIdsHashSet.Contains(lightData.lightId)) {
                lightIntensitiesWithId.Add(new RuntimeLightWithIds.LightIntensitiesWithId(lightData.lightId, lightData.intensity));
                individualLightIdsHashSet.Add(lightData.lightId);
            }
        }

        // Light Groups
        foreach (var lightGroupData in _groupLightData) {
            omittedFromGroupCountIdsHashSet.Clear();
            if (lightGroupData.lightGroup == null) {
                continue;
            }

            var lightGroup = lightGroupData.lightGroup;

            switch (_groupLightsWeighting) {

                case ColorMixAndWeightingApproach.Maximum:
                    groupLightMultiplier = 1.0f;
                    break;

                case ColorMixAndWeightingApproach.FractionAndSum:
                    for (var lightId = lightGroup.startLightId; lightId < lightGroup.startLightId + lightGroup.numberOfElements; lightId++) {
                        if (individualLightIdsHashSet.Contains(lightId) || excludeLightIdsHashSet.Contains(lightId)) {
                            omittedFromGroupCountIdsHashSet.Add(lightId);
                        }
                    }
                    var groupCount = lightGroup.numberOfElements - omittedFromGroupCountIdsHashSet.Count;
                    if (groupCount != 0) {
                        groupLightMultiplier = 1.0f / groupCount;
                    }
                    break;
            }

            for (var lightId = lightGroup.startLightId; lightId < lightGroup.startLightId + lightGroup.numberOfElements; lightId++) {
                // individual light entries already handled, so they are skipped here
                if (!excludeLightIdsHashSet.Contains(lightId) && !individualLightIdsHashSet.Contains(lightId)) {

                    lightIntensitiesWithId.Add(new RuntimeLightWithIds.LightIntensitiesWithId(lightId, lightGroupData.groupIntensity * groupLightMultiplier));
                }
            }
        }

        _directionalLightWithIds.SetNewLightIntensityData(lightIntensitiesWithId.ToArray());
    }
#endif
}
