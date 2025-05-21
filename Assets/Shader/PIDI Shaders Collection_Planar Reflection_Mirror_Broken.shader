Shader "PIDI Shaders Collection/Planar Reflection/Mirror/Broken" {
	Properties {
		[Space(12)] [Header(Basic Properties)] _Metallic ("Metallic (for overlay )", Range(0, 1)) = 0
		_Glossiness ("Smoothness (for overlay)", Range(0, 1)) = 0
		_OverlayTex ("Overlay Tex", 2D) = "black" {}
		_BumpMap ("Normal map", 2D) = "bump" {}
		[Space(12)] [Header(Reflection Properties)] _RefDistortion ("Reflection Distortion", Range(0, 0.08)) = 0.03
		_ReflectionTint ("Reflection Tint", Vector) = (1,1,1,1)
		_ReflectionTex ("ReflectionTex", 2D) = "white" {}
		_NormalDist ("Surface Distortion", Range(0, 1)) = 0
		[Space(12)] [Header(Broken Reflections Effect)] _BrokenMap ("Broken Map (R=X, G=Y, B=Sign) Height(A)", 2D) = "black" {}
		_BrokenDistortion ("Broken Mirror Strength", Range(-0.25, 0.25)) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = 1;
		}
		ENDCG
	}
	Fallback "Diffuse"
}