using UnityEngine;

public static class AudioHelpers {

    public static float NormalizedVolumeToDB(float normalizedVolume) {

        return Mathf.Max(-100.0f, Mathf.Log(normalizedVolume, 1.1f));
    }

    public static float DBToNormalizedVolume(float db) {

        return Mathf.Pow(1.1f, db);
    }
}
