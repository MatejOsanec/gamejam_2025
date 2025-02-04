#ifndef HOLOGRAM_INCLUDED
#define HOLOGRAM_INCLUDED

#include <UnityCG.cginc>
#include <UnityShaderVariables.cginc>

// Concept - currently unused
float2 HologramStripeScan(float worldPosY, float stripeWidth, float stripeFill, float stripeEdge, float stripeSpeed, float scanDistance, float scanWidth, float scanSpeed) {

    worldPosY -= unity_ObjectToWorld[1][3];
    float scan = smoothstep(0.0, scanWidth, frac(worldPosY / scanDistance - _Time.w * scanSpeed));
    scan *= smoothstep(1.0, 0.999, scan);
    float stripe = frac(worldPosY / stripeWidth - _Time.w * stripeSpeed);
    stripe = smoothstep(0.0, stripeEdge, stripe) * (1.0 - smoothstep(stripeFill, stripeFill + stripeEdge, stripe));
    return float2(stripe, scan);
}

// Concept - currently unused
float HologramScan(float worldPosY, float scanDistance, float scanWidth, float scanSpeed) {

    worldPosY -= unity_ObjectToWorld[1][3];
    float scan = smoothstep(0.0, scanWidth, frac(worldPosY / scanDistance - _Time.w * scanSpeed));
    scan *= smoothstep(1.0, 0.999, scan);
    return scan;
}

// Concept - currently unused
float HologramStripe(float worldPosY, float stripeWidth, float stripeFill, float stripeEdge, float stripeSpeed) {

    worldPosY -= unity_ObjectToWorld[1][3];
    float stripe = frac(worldPosY / stripeWidth - _Time.w * stripeSpeed);
    stripe = smoothstep(0.0, stripeEdge, stripe) * (1.0 - smoothstep(stripeFill, stripeFill + stripeEdge, stripe));
    return stripe;
}

// Concept - currently unused
float CubicPulse(float c, float w, float x) {

    x = abs(x - c);
    if (x > w) return 0.0;
    x /= w;
    return (1.0 - x * x *( 3.0 - 2.0 * x));
}

// Concept - currently unused
float HologramCubicPulse(float worldPosY, float pulseWidth, float pulseSpeed, float pulseDistance) {

    worldPosY -= unity_ObjectToWorld[1][3];
    float space = frac(worldPosY / pulseDistance - _Time.w * pulseSpeed);
    float pulse = CubicPulse(0.5, pulseWidth, space);
    return pulse;
}

// Used by HoloSuit and SimpleLitAvatar
float2 HologramDigiGrid(float3 worldPos, float3 fadePos, float gridSize, float stripeSpeed, float scanDistance, float scanFill, float phaseOffset, float time) {

    worldPos -= float3(unity_ObjectToWorld[0][3], unity_ObjectToWorld[1][3], unity_ObjectToWorld[2][3]);
    fadePos.x = abs(fadePos.x);
    float3 gridSpeed = float3(0.0, stripeSpeed, 0.5 * stripeSpeed);

    float gradientSweep = lerp(1.0, 0.0, saturate(frac((worldPos.y + phaseOffset * scanDistance) / scanDistance - time * stripeSpeed)));
    float gradientSweepHi = 0.5 * (1.0 - smoothstep(0.0, 0.6, gradientSweep));
    float scanline = smoothstep(0.975, 1.0, gradientSweep);
    float scanline2 = smoothstep(0.025, 0.0, gradientSweep);
    gradientSweep *= 1.0 - scanline;
    float3 grid = cos(frac(- fadePos * gridSize - time * gridSpeed) + scanFill);
    float digigrid = 0.25 * (scanline + scanline2) + 1.0 - saturate(gradientSweep + grid.y * grid.x * grid.z);
    return float2(digigrid, gradientSweepHi);
}

// Used by several shaders
// Original Hologram
float HologramOverlayValue(float3 worldPos, float gridSize) {

    worldPos -= float3(unity_ObjectToWorld[0][3], unity_ObjectToWorld[1][3], unity_ObjectToWorld[2][3]); // make worldPos relative to transform so movement does not affect the VFX
    float3 gridSpeed = float3(0.0, 1.0, 0.0);

    float gradientSweep = lerp(1.0, 0.0, saturate(frac(0.2 * (worldPos.y * gridSize * 0.33 + _Time.w * 1.0)) * 2.0) );
    gradientSweep *= smoothstep(1.0, 0.95, gradientSweep);

    float3 grid = sin(frac(worldPos * gridSize - _Time.w * gridSpeed) * UNITY_PI) * 1.2;

    float noise = cos(worldPos.y + worldPos.x - worldPos.z * 7.0 + _Time.w * 2.0) * 0.4 + 0.8;

    return gradientSweep * (gradientSweep + grid.y * grid.x * grid.z * noise);
}

// Used by several shaders
float HologramOverlayValue(float3 worldPos) {

    return HologramOverlayValue(worldPos, 3.0);
}

// Used by several shaders
// Used by HologramRays shader
float HologramGrid(float3 worldPos) {

    float gridSize = 3.0;
    float3 gridSpeed = float3(0.0, 1.0, 0.0);

    float3 grid = sin(frac(worldPos * gridSize - _Time.w * gridSpeed) * UNITY_PI) * 1.2;
    return grid.y * grid.x * grid.z;
}

// Grid Only
float HologramDigiGridOnly(float3 worldPos, float3 fadePos, float gridSize, float stripeSpeed, float scanDistance, float scanFill, float phaseOffset, float time) {

    worldPos -= float3(unity_ObjectToWorld[0][3], unity_ObjectToWorld[1][3], unity_ObjectToWorld[2][3]);
    fadePos.x = abs(fadePos.x);
    float3 gridSpeed = float3(0.0, stripeSpeed, 0.5 * stripeSpeed);

    float gradientSweep = lerp(1.0, 0.0, saturate(frac((worldPos.y + phaseOffset * scanDistance) / scanDistance - time * stripeSpeed)));
    float scanline = smoothstep(0.975, 1.0, gradientSweep);
    float scanline2 = smoothstep(0.025, 0.0, gradientSweep);
    gradientSweep *= 1.0 - scanline;
    float3 grid = cos(frac(- fadePos * gridSize - time * gridSpeed) + scanFill);
    float digigrid = 0.25 * (scanline + scanline2) + 1.0 - saturate(gradientSweep + grid.y * grid.x * grid.z);
    return digigrid;
}

// Scanline Only
float HologramDigiScan(float3 worldPos, float stripeSpeed, float scanDistance, float phaseOffset, float time) {

    worldPos -= float3(unity_ObjectToWorld[0][3], unity_ObjectToWorld[1][3], unity_ObjectToWorld[2][3]);
    float gradientSweep = lerp(1.0, 0.0, saturate(frac((worldPos.y + phaseOffset * scanDistance) / scanDistance - time * stripeSpeed)));
    float gradientSweepHi = 1.0 - smoothstep(0.0, 0.6, gradientSweep);
    // gradientSweep *= smoothstep(1.0, 0.95, gradientSweep);
    // gradientSweep = 1.0 - saturate(gradientSweep);
    //return float2(gradientSweep, gradientSweepHi);
    return gradientSweepHi;
}

#endif
