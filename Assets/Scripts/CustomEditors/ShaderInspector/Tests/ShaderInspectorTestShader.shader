// Used by ShaderInspectorTests.cs to execute it's tests
Shader "Hidden/ShaderInspectorTestShader" {

	Properties {

	}

	SubShader{

		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass {

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma shader_feature A
			#pragma shader_feature B
			#pragma shader_feature C

			struct appdata{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f{};

			v2f vert (appdata v){

				v2f o;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target{
				return fixed4(1,1,1,1);
			}
			ENDCG
		}
	}
}
