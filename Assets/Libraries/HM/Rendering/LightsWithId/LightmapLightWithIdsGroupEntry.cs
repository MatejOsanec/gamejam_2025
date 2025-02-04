using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LightmapLightWithIdsGroupEntry : MonoBehaviour {

#pragma warning disable CS0414
    [SerializeField] LightmapLightWithIds _lightmapLightWithIds = default;
    [Space]
    [SerializeField] List<GroupLightData> _groupLightData = default;
    [SerializeField] List<LightIntensitiesWithId> _individualLightData = default;
    [SerializeField] int[] _excludedLightIds = default;
    [Space]
    [SerializeField] ColorMixAndWeightingApproach _groupLightsWeighting = ColorMixAndWeightingApproach.Maximum;

    public LightmapLightWithIds lightmapLightWithIds => _lightmapLightWithIds;
    public List<GroupLightData> groupLightData => _groupLightData;
    public List<LightIntensitiesWithId> individualLightData => _individualLightData;
    public ColorMixAndWeightingApproach groupLightsWeighting => _groupLightsWeighting;

#pragma warning restore CS0414

    [Serializable]
    public class GroupLightData {

        [SerializeField] LightGroupSO _lightGroup = default;
        [SerializeField] float _groupIntensity = default;
        [SerializeField] float _groupProbeHighlightsIntensityMultiplier = default;

        public LightGroupSO lightGroup => _lightGroup;
        public float groupIntensity => _groupIntensity;
        public float groupProbeHighlightsIntensityMultiplier => _groupProbeHighlightsIntensityMultiplier;

        public GroupLightData(LightGroupSO lightGroup, float groupIntensity, float groupProbeHighlightsIntensityMultiplier) {

            _lightGroup = lightGroup;
            _groupIntensity = groupIntensity;
            _groupProbeHighlightsIntensityMultiplier = groupProbeHighlightsIntensityMultiplier;
        }
    }

    [Serializable]
    public class LightIntensitiesWithId {

        [SerializeField] int _lightId;
        [SerializeField] float _intensity = default;
        [SerializeField] float _probeHighlightsIntensityMultiplier = 1.0f;

        public int lightId => _lightId;
        public float intensity => _intensity;
        public float probeHighlightsIntensityMultiplier => _probeHighlightsIntensityMultiplier;

        public LightIntensitiesWithId(int lightId, float intensity, float probeHighlightsIntensityMultiplier) {

            _lightId = lightId;
            _intensity = intensity;
            _probeHighlightsIntensityMultiplier = probeHighlightsIntensityMultiplier;
        }
    }

#if UNITY_EDITOR

    public void SetGroupLightData(GroupLightData[] groupLightData) {

        _groupLightData = new List<GroupLightData> (groupLightData);
        EditorUtility.SetDirty(this);
    }

    public void SetIndividualLightData(LightIntensitiesWithId[] individualLightData) {

        _individualLightData = new List<LightIntensitiesWithId> (individualLightData);
        EditorUtility.SetDirty(this);
    }

    public void SetExcludedLightIds(int[] lightIds) {

        _excludedLightIds = lightIds;
        EditorUtility.SetDirty(this);
    }

    private void OnValidate() {

        if (_lightmapLightWithIds == null) {
            return;
        }

        var lightIntensitiesWithId = new List<LightmapLightWithIds.LightIntensitiesWithId>();
        var excludeLightIdsHashSet = new HashSet<int>(_excludedLightIds);
        var individualLightIdsHashSet = new HashSet<int>();
        var omittedFromGroupCountIdsHashSet = new HashSet<int>();

        var groupLightMultiplier = 1.0f;

        // Individual lights
        foreach (var lightData in _individualLightData) {
            if (!excludeLightIdsHashSet.Contains(lightData.lightId)) {
                lightIntensitiesWithId.Add(new LightmapLightWithIds.LightIntensitiesWithId(lightData.lightId, lightData.intensity, lightData.probeHighlightsIntensityMultiplier));
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

                    lightIntensitiesWithId.Add(new LightmapLightWithIds.LightIntensitiesWithId(lightId, lightGroupData.groupIntensity * groupLightMultiplier, lightGroupData.groupProbeHighlightsIntensityMultiplier));
                }
            }
        }

        _lightmapLightWithIds.SetNewLightIntensityData(lightIntensitiesWithId.ToArray());
    }
#endif
}
