#ifndef CUTOUT_3D_INCLUDED
#define CUTOUT_3D_INCLUDED

sampler3D _CutoutTex;

float CutoutSample(float3 worldPos, float cutoutTexScale) {
    return tex3D(_CutoutTex, worldPos.xyz * cutoutTexScale).a;
}

float Sample3DNoise(float3 worldPos, float scale, float lod) {    
    return tex3Dlod(_CutoutTex, float4(worldPos.xyz * scale, lod)).a;
}

void Cutout(half cutout, float3 worldPos, float3 cutoutTexOffset, float cutoutTexScale) {

    float3 objectPos = float3(unity_ObjectToWorld[0][3], unity_ObjectToWorld[1][3], unity_ObjectToWorld[2][3]); // World pos of object
    float cutoutSample = tex3D(_CutoutTex, (worldPos.xyz - objectPos + cutoutTexOffset) * cutoutTexScale).a;
    clip(cutoutSample - cutout * 1.1 + 0.1); 
}

float CutoutWithEdgeBoost(half cutout, float3 worldPos, float3 cutoutTexOffset, float cutoutTexScale) {
 
    float3 objectPos = float3(unity_ObjectToWorld[0][3], unity_ObjectToWorld[1][3], unity_ObjectToWorld[2][3]); // World pos of object
    float cutoutSample = tex3D(_CutoutTex, (worldPos.xyz - objectPos + cutoutTexOffset) * cutoutTexScale).a;
    float clipValue = cutoutSample - cutout * 1.1 + 0.1;
    clip(clipValue);
    return saturate(1.0 - round(clipValue + 0.45));
}

float CutoutWithClipValue(half cutout, float3 worldPos, float3 cutoutTexOffset, float cutoutTexScale) {

    float3 objectPos = float3(unity_ObjectToWorld[0][3], unity_ObjectToWorld[1][3], unity_ObjectToWorld[2][3]); // World pos of object
    float cutoutSample = tex3D(_CutoutTex, (worldPos.xyz - objectPos + cutoutTexOffset) * cutoutTexScale).a;
    float clipValue = cutoutSample - cutout * 1.1 + 0.1;
    clip(clipValue);
    return clipValue;
}

#endif
