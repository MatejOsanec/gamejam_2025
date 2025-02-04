Shader "Hidden/MainEffect" {

    Properties {
        [HideInInspector] _MainTex ("Main Texture", 2D) = "white" {}
        [HideInInspector] _NoiseTex ("Noise Texture", 2D) = "white" {}
    }

    SubShader {

        Pass {

            name "Main"
            Cull Off
            ZWrite Off
            ZTest Always
            Blend Off

            CGPROGRAM
            #include "MainEffect.cginc"
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}