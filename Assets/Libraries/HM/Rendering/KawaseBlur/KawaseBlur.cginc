#ifndef KAWASE_BLUR_INCLUDED
#define KAWASE_BLUR_INCLUDED

#include "UnityCG.cginc"

sampler2D _MainTex;
half4 _MainTex_ST;
float4 _MainTex_TexelSize;

UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

half4 _Offset;
float _Boost;
float _AlphaWeights;
float _AdditiveAlpha;

float _ZOffset;
float _ZScale;

// Vertex
// ------------------------------------------------------------------------------------

struct v2f {

    float4 pos : SV_POSITION;    
    float2 uv : TEXCOORD0;
};  

v2f vertSimple(appdata_img v) {
               
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);    
    return o;
}   

struct v2fOffsets {

    float4 pos : SV_POSITION;    
    float4 offsets0 : TEXCOORD0;
    float4 offsets1 : TEXCOORD1;
};  

v2fOffsets vertOffsets(appdata_img v) {
               
    v2fOffsets o;
    o.pos = UnityObjectToClipPos(v.vertex);
    float2 uv = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);
    float4 offsets = _Offset * _MainTex_TexelSize.xyxy;
    o.offsets0 = uv.xyxy + float4(offsets.xy, -offsets.xy);
    o.offsets1 = uv.xyxy + float4(offsets.zw, -offsets.zw);
    return o;
}        

// Fragment
// ------------------------------------------------------------------------------------

half4 fragAlphaWeights(v2f i) : SV_Target {

    // Sensitivity of the bloom based on alpha.
    half4 c = tex2D(_MainTex, i.uv);
    c.rgb = c.rgb * saturate(c.a);
    return c;
}

half4 fragAlphaAndDepthWeights(v2f i) : SV_Target {   

    float zDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv));
    half4 c = tex2D(_MainTex, i.uv);
    c.rgb = c.rgb * c.a * saturate(zDepth * 0.02 + 0.6);
    return c;
}

// Simple kawase blur
half4 fragKawaseBlur(v2fOffsets i) : SV_Target {           

    half4 c;

    c = tex2D(_MainTex, i.offsets0.xy);
    c += tex2D(_MainTex, i.offsets0.zw);
    c += tex2D(_MainTex, i.offsets1.xy);                                      
    c += tex2D(_MainTex, i.offsets1.zw);

    c *= (0.25 + _Boost);
    c.a = _AdditiveAlpha;

    return c;
}

// Alpha weights blur
half4 fragKawaseBlurWithAlphaWeights(v2fOffsets i) : SV_Target {     

    half4 c;
    half4 col;

    col = tex2D(_MainTex, i.offsets0.xy);
    // col.rgb *= saturate(col.a * _AlphaWeights);
    c = col;

    col = tex2D(_MainTex, i.offsets0.zw);
    // col.rgb *= saturate(col.a * _AlphaWeights);
    c += col;

    col = tex2D(_MainTex, i.offsets1.xy);                                      
    // col.rgb *= saturate(col.a * _AlphaWeights);
    c += col;

    col = tex2D(_MainTex, i.offsets1.zw);    
    // col.rgb *= saturate(col.a * _AlphaWeights);
    c += col;
    
    c *= (0.25 + _Boost);
    c.rgb *= saturate(col.a * _AlphaWeights);

    return c;
}

// Simple kawase blur
half4 fragKawaseBlurGamma(v2fOffsets i) : SV_Target {           

    half4 c;

    c = tex2D(_MainTex, i.offsets0.xy);
    c += tex2D(_MainTex, i.offsets0.zw);
    c += tex2D(_MainTex, i.offsets1.xy);                                      
    c += tex2D(_MainTex, i.offsets1.zw);

    c *= (0.25 + _Boost);
    c.a = _AdditiveAlpha;
    
    c.rgb = GammaToLinearSpace(c.rgb);

    return c;
}

#endif