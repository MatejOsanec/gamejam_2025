using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class LightConstants {

    public enum BakeId {
        A = 1,
        B = 2,
        C = 3,
        D = 4,
        E = 5,
        F = 6
    }

    [DoesNotRequireDomainReloadInit]
    public static readonly IReadOnlyList<BakeId> allBakeIds = Enum.GetValues(typeof(BakeId)).Cast<BakeId>().ToList();

    /// <summary>
    /// First safe light id to use, numbers smaller than kBaseLightId are considered reserved.
    /// </summary>
    public const int kBaseLightId = 25;

    public const string kLightmapLightBakeIdPrefix = "_LightmapLightBakeId";
    public const string kLightProbeLightBakeIdPrefix = "_LightProbeLightBakeId";

    public static int GetLightmapLightBakeIdPropertyId(BakeId bakeId) {

        return Shader.PropertyToID($"{kLightmapLightBakeIdPrefix}{bakeId}");
    }

    public static int GetLightProbeLightBakeIdPropertyId(BakeId bakeId) {

        return Shader.PropertyToID($"{kLightProbeLightBakeIdPrefix}{bakeId}");
    }

    public static int GetComputeFieldPropertyId(string fieldName) {

        return Shader.PropertyToID($"{fieldName}");
    }
}
