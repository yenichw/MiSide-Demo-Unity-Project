Shader "Hidden/EPO/Fill/Utils/Fill mask"
{
    SubShader
    {
        ColorMask 0

        Cull [_Cull]
        ZWrite Off
        ZTest [_ZTest]

        Pass
        {
            Stencil
            {
                Ref [_FillRef]
				Comp Always
                Pass Replace
                Fail Keep
                ZFail Keep
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile __ USE_CUTOUT
			#pragma multi_compile __ TEXARRAY_CUTOUT
			#pragma multi_compile __ EPO_HDRP
			#pragma multi_compile __ BACK_RENDERING
			#pragma multi_compile __ USE_INFO_BUFFER
			#pragma fragmentoption ARB_precision_hint_fastest

            #include "UnityCG.cginc"
            #include "../MiskCG.cginc"

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
			
			DEFINE_CUTOUT
			DefineCoords

#if USE_INFO_BUFFER
			UNITY_DECLARE_SCREENSPACE_TEXTURE(_InfoBuffer);
			half4 _InfoBuffer_ST;
			half4 _InfoBuffer_TexelSize;
#endif

            v2f vert (appdata v)
            {
                v2f o;
                
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);

#if USE_INFO_BUFFER
				o.screenUV = ComputeScreenPos(o.vertex);
				o.screenUV.xy *= _Scale;
#endif

                FixDepth
				TRANSFORM_CUTOUT

                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
#if USE_INFO_BUFFER
				float2 uv = i.screenUV.xy / i.screenUV.w;

				half4 info = FetchTexelAtFrom(_InfoBuffer, uv, _InfoBuffer_ST);

#if BACK_RENDERING
				if (info.b <= 0.0f || info.b >= 0.5f)
					discard;
#endif
#endif

				CHECK_CUTOUT

                return half4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
}
