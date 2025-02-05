using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LineLightManager : MonoBehaviour {

    private const int kMaxNumberOfLights = 4;

    private readonly Vector4[] _points = new Vector4[kMaxNumberOfLights];
    private readonly Vector4[] _dirs = new Vector4[kMaxNumberOfLights];
    private readonly float[] _dirLengths = new float[kMaxNumberOfLights];
    private readonly Vector4[] _colors = new Vector4[kMaxNumberOfLights];

    
    private static readonly int _activeLineLightsCountID = Shader.PropertyToID("_ActiveLineLightsCount");

    
    private static readonly int _lineLightPointsID = Shader.PropertyToID("_LineLightPoints");

    
    private static readonly int _lineLightDirsID = Shader.PropertyToID("_LineLightDirs");
    
    
    private static readonly int _lineLightDirLengthsID = Shader.PropertyToID("_LineLightDirLengths");

    
    private static readonly int _lineLightColorsID = Shader.PropertyToID("_LineLightColors");

    protected void Update() {

        var lineLights = LineLight.lineLights;
        var activeLightsCount = Mathf.Min(kMaxNumberOfLights, lineLights.Count);

        for (int i = 0; i < activeLightsCount; i++) {

            var lineLight = lineLights[i];
            var lineLightTransform = lineLight.transform;
            Vector3 tp0 = lineLightTransform.TransformPoint(lineLight.p0);
            Vector3 tp1 = lineLightTransform.TransformPoint(lineLight.p1);

            _points[i] = tp0;
            _dirs[i] = tp1 - tp0;
            _dirLengths[i] = _dirs[i].magnitude;
            _colors[i] = lineLight.color;
        }

        for (int i = activeLightsCount; i < kMaxNumberOfLights; i++) {
            
            _colors[i] = Color.clear;
        }

        Shader.SetGlobalInt(_activeLineLightsCountID, activeLightsCount);
        Shader.SetGlobalVectorArray(_lineLightPointsID, _points);
        Shader.SetGlobalVectorArray(_lineLightDirsID, _dirs);
        Shader.SetGlobalFloatArray(_lineLightDirLengthsID, _dirLengths);
        Shader.SetGlobalVectorArray(_lineLightColorsID, _colors);
    }

}
