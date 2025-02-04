#ifndef MAIN_EFFECT_INCLUDED
#define MAIN_EFFECT_INCLUDED

#include "UnityCG.cginc"
#include "BlueNoise.cginc"
#include "MainEffectHelpers.cginc"

UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
half4 _MainTex_ST;
half2 _MainTex_TexelSize;

UNITY_DECLARE_SCREENSPACE_TEXTURE(_BloomTex);

half _BloomIntensity;
half _Contrast;
half _Fade;

struct v2f {

    half4 pos : SV_POSITION;
    half2 uv0 : TEXCOORD0;
    half2 uv1 : TEXCOORD1;
    
    UNITY_VERTEX_OUTPUT_STEREO
};

v2f vert(appdata_img v) {

    v2f o;

    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_OUTPUT(v2f, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    
    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv0 = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);
    o.uv1 = o.uv0;
    #if UNITY_UV_STARTS_AT_TOP
    if (_MainTex_TexelSize.y < 0)
        o.uv1.y = 1 - o.uv1.y;
    #endif
    return o;
}

fixed4 frag(v2f i) : SV_Target {

    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

    float4 baseCol = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv0);
    float alpha = baseCol.a;
    alpha += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv0 + _MainTex_TexelSize.xy * half2(0.0h, -0.5h)).a;
    alpha += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv0 + _MainTex_TexelSize.xy * half2(0.5h, 0.5h)).a;
    alpha += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv0 + _MainTex_TexelSize.xy * half2(-0.5h, 0.5h)).a;

    alpha *= 0.25;

    const float3 bloomCol = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_BloomTex, i.uv1).rgb * _BloomIntensity;
    
    // White boost
    baseCol.rgb = WhiteBoost(baseCol.rgb, alpha);

    // Noise
    float noise = BlueNoiseUV(i.uv0 + half2(0.1, 0.2));
    // Move noise a little bit so it doesn't combine with other noise.

    baseCol.rgb += bloomCol + noise;

    return baseCol * _Fade;
}

#endif
