#ifndef FAKE_MIRROR_VERTEX_DISTORTION_INCLUDED
#define FAKE_MIRROR_VERTEX_DISTORTION_INCLUDED

#include "Cutout3D.cginc"
half _FakeMirrorDistortionNoiseScale;
half _FakeMirrorDistortionStrength;
half3 _FakeMirrorDistortionDirectionality;
half _FakeMirrorDistortionZposMultiplier;

float3 GetFakeMirrorDistortionVertexOffset(float3 worldPos, int lod = 0) {

    worldPos.z *= _FakeMirrorDistortionZposMultiplier;

    float3 offset = Sample3DNoise(worldPos, _FakeMirrorDistortionNoiseScale, lod);
    offset = 2.0 * offset - 1.0;
    offset *= _FakeMirrorDistortionStrength * mul(unity_WorldToObject, half4(_FakeMirrorDistortionDirectionality, 0.0)).xyz;

    return offset;
}

#endif