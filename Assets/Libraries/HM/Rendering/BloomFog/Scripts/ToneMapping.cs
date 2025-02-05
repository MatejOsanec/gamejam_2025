using UnityEngine;

public enum ToneMapping {
    None = 0,
    Aces = 1,
}

public static class ToneMappingExtensions {

    
    private static readonly string[] _shaderKeywordMap = new[] { "", "ACES_TONE_MAPPING" };

    public static void SetShaderKeyword(this ToneMapping toneMapping) {

        if (toneMapping == ToneMapping.Aces) {
            Shader.EnableKeyword(_shaderKeywordMap[(int)ToneMapping.Aces]);
        }
        else {
            Shader.DisableKeyword(_shaderKeywordMap[(int)ToneMapping.Aces]);
        }
    }
}
