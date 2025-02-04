Shader "Custom/BloomPrePassLine" {

    Properties {
        _MainTex ("Texture", 2D) = "white" {}

        [Space]
        [Enum(UnityEngine.Rendering.BlendMode)] _BlendSrcFactor ("Blend Src Factor", Int) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _BlendDstFactor ("Blend Dst Factor", Int) = 10
        [Enum(UnityEngine.Rendering.BlendMode)] _BlendSrcFactorA ("Blend Src Factor A", Int) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _BlendDstFactorA ("Blend Dst Factor A", Int) = 10

        [Space]
        [Enum(UnityEngine.Rendering.BlendOp)] _BlendOp ("Blend Operation", Int) = 0
    }

    CGINCLUDE

    #include "UnityCG.cginc"
    #include "Assets/Visuals/Shaders/Rendering/CustomFog.cginc"

    sampler2D _MainTex;

    float4x4 _VertexTransformMatrix;

    float _LineIntensityMultiplier;

    struct appdata {
        float4 vertex : POSITION;
        float4 color : COLOR;
        float3 texcoord : TEXCOORD0;
        half3 viewPos : TANGENT;
    };

    struct v2f {
        float4 vertex : SV_POSITION;
        float4 color : COLOR;
        float3 uv : TEXCOORD0;
        half4 viewPos : TEXCOORD1;
    };

    v2f vert(appdata v) {

        v2f o;
        o.vertex = mul(_VertexTransformMatrix, v.vertex);
        o.color = v.color;
        o.color.rgb = GammaToLinearSpace(o.color.rgb);
        o.uv = v.texcoord;

        o.viewPos.xyz = v.viewPos / v.viewPos.z;
        o.viewPos.w = 1 / v.viewPos.z;

        return o;
    }

    float4 frag(v2f i) : SV_Target {

        half3 viewPos = i.viewPos.xyz / i.viewPos.w;
        half fogA = i.color.a;
        i.color.a *= i.color.a;
        i.uv.x /= i.uv.z;
        float4 c = i.color * tex2D(_MainTex, i.uv);
        half fogStrength = FogStrength(dot(viewPos, viewPos), 0.0, FogScaleMultiplierForLight(fogA));
        c.a *= 1.0 - fogStrength;
        c.rgb *= c.a;

        return c;
    }

    ENDCG

    SubShader {

        Blend [_BlendSrcFactor] [_BlendDstFactor], [_BlendSrcFactorA] [_BlendDstFactorA]
        BlendOp [_BlendOp]
        Zwrite Off
        Cull Off

        Tags {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "PreviewType"="Plane"
        }

        Pass {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile _ ENABLE_BLOOM_FOG
            ENDCG
        }
    }
}
