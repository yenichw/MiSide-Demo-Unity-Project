Shader "Hidden/EPO/Fill/Basic/Color fill"
{
    Properties
    {
        _PublicColor ("Color", Color) = (1, 0, 0, 0.2)
    }

    SubShader
    {
        Cull [_Cull]
        ZWrite Off
        ZTest Off
        Blend SrcAlpha OneMinusSrcAlpha 

        Pass
        {
            Stencil
            {
                Ref [_FillRef]
                Comp Equal
                Pass Zero
                Fail Keep
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
            #include "../MiskCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
#if USE_CUTOUT
                float2 uv : TEXCOORD0;
#endif

#if USE_INFO_BUFFER
				float4 screenUV : TEXCOORD1;
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

            half4 _PublicColor;

#if USE_INFO_BUFFER
			UNITY_DECLARE_SCREENSPACE_TEXTURE(_InfoBuffer);
			half4 _InfoBuffer_ST;
			half4 _InfoBuffer_TexelSize;
#endif

			half4 frag(v2f i) : SV_Target
			{
				CHECK_CUTOUT

				float4 result = _PublicColor;

#if USE_INFO_BUFFER
				float2 uv = (i.screenUV.xy / i.screenUV.w);
				result.a *= GetScaler(i.screenUV, FetchTexelAtFrom(_InfoBuffer, uv, _InfoBuffer_ST));
#endif

                return result;
            }
            ENDCG
        }
    }
}
