/*           INFINITY CODE          */
/*     https://infinity-code.com    */

Shader "Hidden/InfinityCode/UltimateEditorEnhancer/SceneViewHighlight"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
	
	SubShader
	{
		CGINCLUDE
		#include "UnityCG.cginc"
		struct adt
		{
			float4 pos : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2f
		{
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;

			#if UNITY_UV_STARTS_AT_TOP
			float2 uv2 : TEXCOORD1;
			#endif
		};

		float4 _MainTex_TexelSize;
		float4 _MainTex_ST;

		v2f vertex(adt input)
		{
			v2f output;

			output.pos = UnityObjectToClipPos(input.pos);
			output.uv = UnityStereoScreenSpaceUVAdjust(input.uv, _MainTex_ST);
		#if UNITY_UV_STARTS_AT_TOP
			output.uv2 = output.uv;
			if (_MainTex_TexelSize.y < 0) output.uv.y = 1 - output.uv.y;
		#endif
			return output;
		}
		ENDCG

		Pass // #0
		{
			Cull Off

			CGPROGRAM
				#pragma vertex vertex
				#pragma fragment fragment			
				#include "UnityCG.cginc"

				sampler2D _MainTex;
				sampler2D _Tex;
				fixed4 _ObjectID;

				fixed4 fragment(v2f input) : SV_Target
				{
					fixed4 c = tex2D(_MainTex, input.uv);
					if (c.a < 0.1) discard;
					return _ObjectID;
				}
			ENDCG
		}

		Pass // #1
		{
			ZTest Always
			Cull Off
			ZWrite Off

			CGPROGRAM
				#pragma vertex vertex
				#pragma fragment fragment
				#pragma target 3.0
				#include "UnityCG.cginc"

				sampler2D _MainTex;

				static const half2 o[8] = {
					half2(-1,-1),
					half2(0,-1),
					half2(1,-1),
					half2(-1,0),
					half2(1,0),
					half2(-1,1),
					half2(0,1),
					half2(1,1)
				};

				fixed4 fragment(v2f input) : SV_Target
				{
					fixed4 c1 = tex2D(_MainTex, input.uv);
					if (all(c1.rg == 0)) return fixed4(0, 0, 0, 0);

					fixed4 c = fixed4(1, 1, 1, 1);
					
					[unroll(8)]
					for (int i = 0; i < 8; i++)
					{
						fixed4 c2 = tex2D(_MainTex, input.uv + o[i] * _MainTex_TexelSize.xy);
						if (any(c2.rg != 0) && any(c2.rg != c1.rg))
						{
							c = fixed4(0, 0, 0, 0);
							break;
						}
					}
					return c;
				}
			ENDCG
		}

		Pass // #2
		{
			ZTest Always
			Cull Off
			ZWrite Off

			CGPROGRAM
				#pragma vertex vertex
				#pragma fragment fragment
				#pragma target 3.0
				#include "UnityCG.cginc"

				sampler2D _MainTex;

				static const half w[9] = 
				{ 
					0.0204001988,
					0.0577929595,
					0.1215916882,
					0.1899858519,
					0.2204586031,
					0.1899858519,
					0.1215916882,
					0.0577929595,
					0.0204001988 
				};

				half4 fragment(v2f input) : SV_Target
				{
					int i;
					float2 s = _MainTex_TexelSize.xy * float2(1, 0);
					half4 c = 0;
					float2 uv = input.uv - s * 4;
					for (i = 0; i < 9; i++)
					{
						c += tex2D(_MainTex, uv) * w[i];
						uv += s;
					}

					s = _MainTex_TexelSize.xy * float2(0, 1);
					uv = input.uv - s * 4;
					for (i = 0; i < 9; i++)
					{
						c += tex2D(_MainTex, uv) * w[i];
						uv += s;
					}
					return c;
				}
			ENDCG
		}

		Pass // #3
		{
			ZTest Always
			Cull Off
			ZWrite Off

			CGPROGRAM
				#pragma vertex vertex
				#pragma fragment fragment			
				#include "UnityCG.cginc"

				sampler2D _MainTex;
				sampler2D _OutlineTex;
				sampler2D _FillTex;
				fixed4 _Color;

				fixed4 fragment(v2f input) : SV_Target
				{
				#if UNITY_UV_STARTS_AT_TOP
					fixed4 c = tex2D(_MainTex, input.uv2);
				#else
					fixed4 c = tex2D(_MainTex, input.uv);
				#endif
					fixed4 o = tex2D(_OutlineTex, input.uv);
					fixed4 f = tex2D(_FillTex, input.uv);
					float v = f.r > 0 ? _Color.a : (o.r > 0.05 ? 1 : 0);
					c.rgb = c.rgb * (1 - v) + _Color.rgb * v;
					c.a = 1;
					return c;
				}
			ENDCG
		}
	}
}