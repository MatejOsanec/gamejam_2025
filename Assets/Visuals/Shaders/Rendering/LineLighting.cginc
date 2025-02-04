#ifndef LINE_LIGHTING_INCLUDED
#define LINE_LIGHTING_INCLUDED

static const int kMaxNumberOfLineLights = 4;

half3 _LineLightPoints[kMaxNumberOfLineLights];
half3 _LineLightDirs[kMaxNumberOfLineLights];
fixed4 _LineLightColors[kMaxNumberOfLineLights];
half _LineLightDirLengths[kMaxNumberOfLineLights];

float4 LineLightingLinearFalloff(float3 A, float3 N) {	

    float4 c = float4(0.0, 0.0, 0.0, 0.0);

    [unroll]
    for (int j = 0; j < kMaxNumberOfLineLights; j++) {

        // Precompute some values.
        float3 V = _LineLightDirs[j];
        float3 P0 = _LineLightPoints[j]; 
        float3 Pd0 = P0 - A;
        float3 Pd1 = P0 + V - A;
        float Pd0V = dot(Pd0, V);
        float Pd0Pd0 = dot(Pd0, Pd0);
        float VV = dot(V, V);
        float NV = dot(N, V);
        float3 U = V.yxx * V.yxx + V.zzy * V.zzy;
        float3 Q = Pd0.yxx * V.yxx + Pd0.zzy * V.zzy;
        float L = sqrt(dot(Pd0*Pd0, U) - 2 * dot(Pd0 * V, Pd0.zxy * V.zxy));
        float B = 2 * dot(N, Pd0 * U - V * Q);

        float t0 = 0.0;
        float t1 = 1.0;        

        // We need to clamp t values if points are on the other side of the plane defined by N normal and point A.
        float Pd0N = dot(Pd0, N);
        float Pd1N = dot(Pd1, N);
        float tc = - Pd0N / NV;   

        if (Pd0N < 0) {
            t0 = tc;
        }

        if (Pd1N < 0) {
            t1 = tc;
        }        
        
        // Integrating using simple diffuse lighting.        
        float I0 = (NV * log (t0 * 2 * Pd0V + Pd0Pd0 + t0 * t0 * VV) + (B * atan( (Pd0V + t0 * VV) / L)) / L ) / (2 * VV);
        float I1 = (NV * log (t1 * 2 * Pd0V + Pd0Pd0 + t1 * t1 * VV) + (B * atan( (Pd0V + t1 * VV) / L)) / L ) / (2 * VV);        

        float intensity = _LineLightColors[j].a * (I1 - I0);
        
        c.rgb += _LineLightColors[j].rgb * saturate(intensity);
    }

    return c;
}

float4 LineLightingInverseSquaredFallOff(float3 A, float3 N) {

    half4 c = half4(0.0, 0.0, 0.0, 0.0);

    [unroll]
    for (int j = 0; j < kMaxNumberOfLineLights; j++) {

        half3 V = _LineLightDirs[j];
        half3 P0 = _LineLightPoints[j]; 
        half3 Pd0 = P0 - A;
        half3 Pd1 = P0 + V - A;

        half3 Pd0S = Pd0 * Pd0;
        half Pd0Pd0 = dot(Pd0, Pd0);
        half3 VVS = V * V;
        half3 M = Pd0 * V;
        half3 L = M.yzx + M.zxy;
        half3 O = Pd0S.yzx + Pd0S.zxy;
        half3 Q = VVS.yzx + VVS.zxy;
        half VV = dot(V, V);

        half t0 = 0.0;
        half t1 = 1.0;        

        // We need to clamp t values if points are on the other side of the plane defined by N normal and point A.
        half Pd0N = dot(Pd0, N);
        half Pd1N = dot(Pd1, N);
        half tc = - Pd0N / dot(N, V);

        if (Pd0N < 0) {
            t0 = tc;
        }

        if (Pd1N < 0) {
            t1 = tc;
        }

        half3 S = Pd0 * Q - V * L;
        half3 R = Pd0 * L - V * O;
        half3 E = dot(Pd0S, Q) - 2 * dot(M, M.yzx);
        half F = 2 * dot (Pd0, V);

        half I0 = dot(N, t0 * S + R) / (E * sqrt(t0 * F + t0 * t0 * VV + Pd0Pd0));
        half I1 = dot(N, t1 * S + R) / (E * sqrt(t1 * F + t1 * t1 * VV + Pd0Pd0));

        half intensity = _LineLightColors[j].a * (I1 - I0) * _LineLightDirLengths[j];
        
        c.rgb += _LineLightColors[j].rgb * saturate(intensity);
    }

    return c;

    // Wolfram Alpha source
    // integrate (n0*(p0 + t * v0) + n1*(p1 + t * v1) + n2*(p2 + t * v2)) / (c*((p0 + t * v0)^2 + (p1 + t * v1)^2 + (p2 + t * v2)^2)^1.5) dt

    // Raw version of primitive function.
    // (
    //     nx (t (p.x ((v.y)^2 + (v.z)^2) - v.x(p.y v.y + p.z v.z )) + p.x (p.y v.y + p.z v.z) - v.x ((p.y)^2 + (p.z)^2) ) +      
    //     n.y (t (p.y ((v.z)^2 + (v.x)^2) - v.y(p.z v.z + p.x v.x )) + p.y (p.z v.z + p.x v.x) - v.y ((p.z)^2 + (p.x)^2) ) +        
    //     n.z (t (p.z ((v.x)^2 + (v.y)^2) - v.z(p.x v.x + p.y v.y )) + p.z (p.x v.x + p.y v.y) - v.z ((p.x)^2 + (p.y)^2) ) +            
    // )
    // /
    // (
    //     (
    //           (p.x)^2 ((v.y)^2 + (v.z)^2) 
    //         + (p.y)^2 ((v.x)^2 + (v.z)^2)
    //         + (p.z)^2 ((v.x)^2 + (v.y)^2) 
    //         - 2 (p.y v.y p.x v.x + p.y v.y p.z v.z + p.x p.z v.x v.z)
    //     ) *
    //     sqrt(
    //           2 t (p.x v.x + p.y v.y + p.z v.z) 
    //         + (p.x)^2 + (p.y)^2 + (p.z)^2 
    //         + t^2 ((v.x)^2 + (v.y)^2 + (v.z)^2)
    //     )
    // )

}

half4 LineLightingInverseSquaredFallOffNoClamp(half3 A, half3 N) {

    half4 c = half4(0.0, 0.0, 0.0, 0.0);

    [unroll]
    for (int j = 0; j < kMaxNumberOfLineLights; j++) {

        half3 V = _LineLightDirs[j];
        half3 P0 = _LineLightPoints[j]; 
        half3 Pd0 = P0 - A;
        half3 Pd1 = P0 + V - A;

        half3 Pd0S = Pd0 * Pd0;
        half Pd0Pd0 = dot(Pd0, Pd0);
        half3 VVS = V * V;
        half3 M = Pd0 * V;
        half3 L = M.yzx + M.zxy;
        half3 O = Pd0S.yzx + Pd0S.zxy;
        half3 Q = VVS.yzx + VVS.zxy;
        half VV = dot(V, V); 

        half3 S = Pd0 * Q - V * L;
        half3 R = Pd0 * L - V * O;
        half3 E = dot(Pd0S, Q) - 2 * dot(M, M.yzx);
        half F = 2 * dot (Pd0, V);

        half I0 = dot(N, R) / (E * sqrt(Pd0Pd0));
        half I1 = dot(N, S + R) / (E * sqrt(F + VV + Pd0Pd0));

        half intensity = _LineLightColors[j].a * (I1 - I0) * _LineLightDirLengths[j];
        
        c.rgb += _LineLightColors[j].rgb * saturate(intensity);
    }

    return c;
}

float4 FakeLineLighting(float3 A, float3 N) {

    half4 c = half4(0.0, 0.0, 0.0, 0.0);

    [unroll]
    for (int j = 0; j < kMaxNumberOfLineLights; j++) {

        half3 V = normalize(_LineLightDirs[j]);
        half3 P0 = _LineLightPoints[j]; 
        
        half3 X = P0 + V * dot(A - P0, V);
        
        half dist = length(X - A);
        
        //half intensity = _LineLightColors[j].a * max(0.0, max(abs(dot(V, N)), dot((X - A) / dist, N))) / (dist * dist);
        
        //half intensity = _LineLightColors[j].a * max(  abs(dot(V, N)) / (1.0 + dist),  max(0.0, dot((X - A) / dist, N)) / (1.0 + dist) );        
        half intensity = _LineLightColors[j].a * max(0.0, dot((X - A) / dist, N)) / (dist * dist);
        
        c.rgb += _LineLightColors[j].rgb * saturate(intensity);
    }

    return c;

}


#endif