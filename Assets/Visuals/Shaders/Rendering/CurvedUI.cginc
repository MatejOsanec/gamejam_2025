#ifndef CURVED_UI_INCLUDED
    #define CURVED_UI_INCLUDED

    inline float4 CurveVertex(float4 vertex, float radius) {

        if (radius == 0) {
            return vertex;
        }

        return float4(sin(vertex.x / radius) * radius, vertex.y, cos(vertex.x / radius) * radius - radius + vertex.z, vertex.w);
    }

#endif
