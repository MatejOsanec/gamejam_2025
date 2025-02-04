using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class BloomFogEnvironmentParams : PersistentScriptableObject {

    public float attenuation = 0.1f;
    public float offset = 0.0f;
    public float heightFogStartY = -300.0f;
    public float heightFogHeight = 10.0f;
    [Tooltip("Limits the maximum multiplication of the bloom at low intensities")] public float autoExposureLimit = 1000.0f;
    [Tooltip("Makes AE behave inverted at low light situations, making bloom stronger the more lights are on")] public bool legacyAutoExposure = false;
    [Min(0.0f)] public float noteSpawnIntensity = 1.0f;

}
