
float4 BillboardVert(float4 vertex) {

    float4 cameraLocalPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0));
    float d = length(cameraLocalPos.xz);
    float3 n;
    n.xz = cameraLocalPos.xz / d;
    n.y = 0;

    // Use camera XZ position in object space. This allows us to rotate only around local Y axis.
    float cosA = dot(float2(0.0, -1.0), n.xz);
    float sinA = dot(float2(1.0, 0.0), n.xz);

    // Simple 2D rotation matrix.
    float2x2 localRot = float2x2(cosA, -sinA, sinA, cosA);

    // Rotate sprite around the Y axes.
    vertex.xz = mul(localRot, vertex.xz);

    return vertex;
 }
