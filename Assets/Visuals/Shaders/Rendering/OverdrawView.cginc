#ifndef OVERDRAW_VIEW_INCLUDED
#define OVERDRAW_VIEW_INCLUDED

#define LAYER_OPACITY 0.1

half _TrueOverdrawOn; // Only needed until true Overdraw becomes the standard, until then we need to distinguish between the two
half _TransparentOverdrawOn = 1.0;
half _OpaqueOverdrawOn = 1.0;

float4 _OverdrawColor = float4(1.0f, 1.0f, 1.0f, 0.0f);

inline float4 OverdrawView(half opacity) {

    return _OverdrawColor * (opacity * LAYER_OPACITY * _TransparentOverdrawOn);
}

inline float4 OverdrawViewOpaque() {

    return _OverdrawColor * (LAYER_OPACITY * _TrueOverdrawOn * _OpaqueOverdrawOn);
}

#endif
