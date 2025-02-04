#ifndef BLOOM_SAMPLING
#define BLOOM_SAMPLING

#include "UnityCG.cginc"
#include "HLSLSupport.cginc"

#if IS_SCREENSPACE_EFFECT
    #if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
        #define ARGS_TEXTURE_SAMPLER(tex) UNITY_ARGS_TEX2DARRAY(tex)
        #define PASS_TEXTURE_SAMPLER(tex) UNITY_PASS_TEX2DARRAY(tex)
    #else
        #define ARGS_TEXTURE_SAMPLER(tex) sampler2D_float tex
        #define PASS_TEXTURE_SAMPLER(tex) tex
    #endif

    #define SAMPLE_TEXTURE(tex, uv) UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex, uv)
#else
    #define ARGS_TEXTURE_SAMPLER(tex) sampler2D tex
    #define PASS_TEXTURE_SAMPLER(tex) tex
    #define SAMPLE_TEXTURE(tex, uv) tex2D(tex, uv)
#endif

// Better, temporally stable box filtering
// [Jimenez14] http://goo.gl/eomGso
// . . . . . . .
// . A . B . C .
// . . D . E . .
// . F . G . H .
// . . I . J . .
// . K . L . M .
// . . . . . . .
half4 DownsampleBox13Tap(ARGS_TEXTURE_SAMPLER(tex), float2 uv, float2 texelSize) {
    
    half4 A = SAMPLE_TEXTURE(tex, uv + texelSize * float2(-1.0, -1.0));
    half4 B = SAMPLE_TEXTURE(tex, uv + texelSize * float2( 0.0, -1.0));
    half4 C = SAMPLE_TEXTURE(tex, uv + texelSize * float2( 1.0, -1.0));
    half4 D = SAMPLE_TEXTURE(tex, uv + texelSize * float2(-0.5, -0.5));
    half4 E = SAMPLE_TEXTURE(tex, uv + texelSize * float2( 0.5, -0.5));
    half4 F = SAMPLE_TEXTURE(tex, uv + texelSize * float2(-1.0,  0.0));
    half4 G = SAMPLE_TEXTURE(tex, uv                                 );
    half4 H = SAMPLE_TEXTURE(tex, uv + texelSize * float2( 1.0,  0.0));
    half4 I = SAMPLE_TEXTURE(tex, uv + texelSize * float2(-0.5,  0.5));
    half4 J = SAMPLE_TEXTURE(tex, uv + texelSize * float2( 0.5,  0.5));
    half4 K = SAMPLE_TEXTURE(tex, uv + texelSize * float2(-1.0,  1.0));
    half4 L = SAMPLE_TEXTURE(tex, uv + texelSize * float2( 0.0,  1.0));
    half4 M = SAMPLE_TEXTURE(tex, uv + texelSize * float2( 1.0,  1.0));

    half2 div = (1.0 / 4.0) * half2(0.5, 0.125);

    half4 o = (D + E + I + J) * div.x;
    o += (A + B + G + F) * div.y;
    o += (B + C + H + G) * div.y;
    o += (F + G + L + K) * div.y;
    o += (G + H + M + L) * div.y;

    return o;
}

// Standard box filtering
half4 DownsampleBox4Tap(ARGS_TEXTURE_SAMPLER(tex), float2 uv, float2 texelSize) {

    float4 d = texelSize.xyxy * float4(-1.0, -1.0, 1.0, 1.0);

    half4 s;
    s =  SAMPLE_TEXTURE(tex, uv + d.xy);
    s += SAMPLE_TEXTURE(tex, uv + d.zy);
    s += SAMPLE_TEXTURE(tex, uv + d.xw);
    s += SAMPLE_TEXTURE(tex, uv + d.zw);

    return s * (1.0 / 4.0);
}

// 9-tap bilinear upsampler (tent filter)
half4 UpsampleTent(ARGS_TEXTURE_SAMPLER(tex), float2 uv, float2 texelSize, float4 sampleScale) {

    float4 d = texelSize.xyxy * float4(1.0, 1.0, -1.0, 0.0) * sampleScale;

    half4 s;
    s =  SAMPLE_TEXTURE(tex, uv - d.xy);
    s += SAMPLE_TEXTURE(tex, uv - d.wy) * 2.0;
    s += SAMPLE_TEXTURE(tex, uv - d.zy);

    s += SAMPLE_TEXTURE(tex, uv + d.zw) * 2.0;
    s += SAMPLE_TEXTURE(tex, uv       ) * 4.0;
    s += SAMPLE_TEXTURE(tex, uv + d.xw) * 2.0;

    s += SAMPLE_TEXTURE(tex, uv + d.zy);
    s += SAMPLE_TEXTURE(tex, uv + d.wy) * 2.0;
    s += SAMPLE_TEXTURE(tex, uv + d.xy);

    return s * (1.0 / 16.0);
}

// Standard box filtering
half4 UpsampleBox(ARGS_TEXTURE_SAMPLER(tex), float2 uv, float2 texelSize, float4 sampleScale) {

    float4 d = texelSize.xyxy * float4(-1.0, -1.0, 1.0, 1.0) * (sampleScale * 0.5);

    half4 s;
    s =  SAMPLE_TEXTURE(tex, uv + d.xy);
    s += SAMPLE_TEXTURE(tex, uv + d.zy);
    s += SAMPLE_TEXTURE(tex, uv + d.xw);
    s += SAMPLE_TEXTURE(tex, uv + d.zw);

    return s * (1.0 / 4.0);
}

#endif