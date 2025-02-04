#ifndef BLUE_NOISE_INCLUDED
#define BLUE_NOISE_INCLUDED

sampler2D _GlobalBlueNoiseTex;
float2 _GlobalBlueNoiseParams;
float _GlobalRandomValue;

inline half4 ComputeNoiseScreenPos(half4 nonStereoScreenPos){

    nonStereoScreenPos.xy = nonStereoScreenPos.xy * _GlobalBlueNoiseParams.xy + _GlobalRandomValue.xx * nonStereoScreenPos.w + unity_ObjectToWorld._m03_m13;
    return nonStereoScreenPos;
}

float BlueNoise(half4 noiseScreenPos) {

    return (tex2Dproj(_GlobalBlueNoiseTex, noiseScreenPos).r - 0.5) / 255.0;
}

float BlueNoiseUV(half2 uv) {

    uv.xy = uv.xy * _GlobalBlueNoiseParams.xy + _GlobalRandomValue.xx;
    return (tex2D(_GlobalBlueNoiseTex, uv).r - 0.5) / 255.0;
}

#endif
