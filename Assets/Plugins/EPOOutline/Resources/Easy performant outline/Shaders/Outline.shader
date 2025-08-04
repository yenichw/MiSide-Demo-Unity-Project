Shader "Hidden/Outline"
{
    SubShader
    {
        Cull [_Cull]
        ZWrite Off
        ZTest [_ZTest]

        Pass
        {
			ColorMask [_ColorMask]

            Stencil
            {
                Ref [_OutlineRef]
                Comp Always
                Pass Replace
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile __ USE_CUTOUT
			#pragma multi_compile __ TEXARRAY_CUTOUT
			#pragma multi_compile __ EPO_HDRP
			#pragma multi_compile __ USE_INFO_BUFFER
			#pragma multi_compile __ BACK_RENDERING
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile __ BACK_OBSTACLE_RENDERING BACK_MASKING_RENDERING

            #include "UnityCG.cginc"
            #include "MiskCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
#if USE_CUTOUT
                float2 uv : TEXCOORD0;
#endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
#if USE_CUTOUT
                float2 uv : TEXCOORD0;
#endif

#if USE_INFO_BUFFER
				float4 screenUV : TEXCOORD1;
#endif

                UNITY_VERTEX_OUTPUT_STEREO
            };

#if USE_INFO_BUFFER
			UNITY_DECLARE_SCREENSPACE_TEXTURE(_InfoBuffer);
			half4 _InfoBuffer_ST;
			half4 _InfoBuffer_TexelSize;
#endif

			DEFINE_CUTOUT
			DefineCoords

            v2f vert (appdata v)
            {
                v2f o;
                
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);

#if USE_INFO_BUFFER
				o.screenUV = ComputeScreenPos(o.vertex);
#endif

                FixDepth
				TRANSFORM_CUTOUT

                return o;
            }

            half4 _EPOColor;

			half4 frag(v2f i) : SV_Target
			{
				CHECK_CUTOUT

#if USE_INFO_BUFFER
				float2 uv = i.screenUV.xy / i.screenUV.w;

				half4 info = FetchTexelAtFrom(_InfoBuffer, uv, _InfoBuffer_ST);

				float scaler = 1.0f;

#if BACK_OBSTACLE_RENDERING
				scaler = abs(info.b - 0.25f) < 0.05f;
#endif

#if BACK_MASKING_RENDERING
				scaler = abs(info.b - 0.6f) > 0.05f;
#endif

#else
				float scaler = 1.0f;
#endif

                half4 resultingColor = _EPOColor * scaler;
                resultingColor.rgb *= resultingColor.a;

                return resultingColor;
            }
            ENDCG
        }
    }
}
